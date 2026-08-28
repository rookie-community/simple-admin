using Admin.Desktop.View.Quartzs;
using Admin.Permissions;
using Admin.Quartzs;
using Admin.Quartzs.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Admin.Desktop.ViewModel.Quartzs
{
    public partial class QuartzVM : ObservableObject, ITransientDependency
    {
        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<QrtzJobDto> QrtzJobs { get; set; } = new ObservableCollection<QrtzJobDto>();

        [ObservableProperty]
        public partial ObservableCollection<QrtzTriggerDto> QrtzTriggers { get; set; } = new ObservableCollection<QrtzTriggerDto>();

        [ObservableProperty]
        public partial int MainPageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long MainTotalCount { get; set; }

        [ObservableProperty]
        public partial int MainDataCountPerPage { get; set; } = 10;

        [ObservableProperty]
        public partial string MainDialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial int DetailPageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long DetailTotalCount { get; set; }

        [ObservableProperty]
        public partial int DetailDataCountPerPage { get; set; } = 5;

        [ObservableProperty]
        public partial string DetailDialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial Dictionary<string, Visibility> BtnPerms { get; set; } = new Dictionary<string, Visibility>();

        public QuartzView Owner { get; private set; } = null!;

        private readonly IQuartzJobAppService _quartzJobAppService;
        private readonly IQuartzTriggerAppService _quartzTriggerAppService;
        private readonly IQuartzCronTriggerAppService _quartzCronTriggerAppService;
        private readonly IQuartzSimpleTriggerAppService _quartzSimpleTriggerAppService;
        private readonly IQuartzSimPropTriggerAppService _quartzSimPropTriggerAppService;
        private readonly ILogger<QuartzVM> _logger;

        public QuartzVM(
            IQuartzJobAppService quartzAppService,
            IQuartzTriggerAppService quartzTriggerAppService,
            IQuartzCronTriggerAppService quartzCronTriggerAppService,
            IQuartzSimpleTriggerAppService quartzSimpleTriggerAppService,
            IQuartzSimPropTriggerAppService quartzSimPropTriggerAppService,
            ILogger<QuartzVM> logger)
        {
            _quartzJobAppService = quartzAppService;
            _quartzTriggerAppService = quartzTriggerAppService;
            _quartzCronTriggerAppService = quartzCronTriggerAppService;
            _quartzSimpleTriggerAppService = quartzSimpleTriggerAppService;
            _quartzSimPropTriggerAppService = quartzSimPropTriggerAppService;
            _logger = logger;
            LoadButtonPermissions();
        }

        internal async Task InitialAsync(QuartzView owner)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(MainDialogContainerToken);
            try
            {
                Owner = owner;
                await LoadMainDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog.Close();
            }
        }

        private void LoadButtonPermissions()
        {
            var btnTemps = new Dictionary<string, string>
            {
                { "MainCreate",QuartzPermissions.Jobs.Create},
                { "MainEdit",QuartzPermissions.Jobs.Edit },
                { "MainDelete", QuartzPermissions.Jobs.Delete },
                { "DetailCreate", QuartzPermissions.Triggers.Create },
                { "DetailEdit", QuartzPermissions.Triggers.Edit },
                { "DetailDelete", QuartzPermissions.Triggers.Delete },
            };

            foreach (var btnTemp in btnTemps)
            {
                var isGranted = App.PermissionChecker(btnTemp.Value);
                BtnPerms.TryAdd(btnTemp.Key, isGranted);
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(MainDialogContainerToken);
            try
            {
                await LoadMainDataAsync();
            }
            catch (AbpValidationException abpEx)
            {
                _logger.LogException(abpEx);
                var errorMessages = abpEx.ValidationErrors.Select(x => x.ErrorMessage);
                MessageBox.Error(string.Join('.', errorMessages), abpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog.Close();
            }
        }

        [RelayCommand]
        private void Reset()
        {
            Name = string.Empty;
        }

        [RelayCommand]
        private async Task AddJobAsync()
        {
            try
            {
                var view = new QuartzJobAddView();
                var result = view.ShowDialog();
                if (result == true)
                {
                    Growl.Success("新增成功");
                    await SearchCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task EditJobAsync(IdentityUserDto user)
        {
            try
            {
                var view = new QuartzJobEditView(user.Id);
                var result = view.ShowDialog();
                if (result == true)
                {
                    Growl.Success("更新成功");
                    await SearchCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task DeleteJob(QrtzJobDto qrtzJob)
        {
            Dialog? loadDialog = null;
            try
            {
                var result = MessageBox.Ask($"确认删除数据？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>();
                await _quartzJobAppService.DeleteAsync(qrtzJob.Id);
                await SearchCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog?.Close();
            }
        }

        [RelayCommand]
        private async Task BatchDeleteJob(IList sender)
        {
            Dialog? loadDialog = null;
            try
            {
                var qrtzJobIds  = sender.Cast<QrtzJobDto>().Select(x => x.Id).ToList();
                if (qrtzJobIds.Count == 0)
                {
                    MessageBox.Warning("未选中任何数据！");
                    return;
                }
                var result = MessageBox.Ask($"确认删除选中数据？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>();
                foreach (var qrtzJobId in qrtzJobIds)
                {
                    await _quartzJobAppService.DeleteAsync(qrtzJobId);
                }
                await SearchCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog?.Close();
            }
        }

        [RelayCommand]
        private async Task MainPageChangedAsync()
        {
            await SearchCommand.ExecuteAsync(null);
        }

        private async Task LoadMainDataAsync()
        {
            var result = await _quartzJobAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
            {
                //filter = name,
                SkipCount = (MainPageIndex - 1) * MainDataCountPerPage,
                MaxResultCount = MainDataCountPerPage
            });
            MainTotalCount = result.TotalCount;
            QrtzJobs = new ObservableCollection<QrtzJobDto>(result.Items);
        }

        [RelayCommand]
        private async Task AddTriggerAsync()
        {
            try
            {
                var view = new QuartzTriggerAddView();
                var result = view.ShowDialog();
                if (result == true)
                {
                    Growl.Success("新增成功");
                    await SearchCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task EditTriggerAsync(IdentityUserDto user)
        {
            try
            {
                var view = new QuartzTriggerEditView(user.Id);
                var result = view.ShowDialog();
                if (result == true)
                {
                    Growl.Success("更新成功");
                    await SearchCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task DetailPageChangedAsync()
        {
            await LoadDetailDataAsync();
        }

        [RelayCommand]
        private async Task DeleteTrigger(QrtzTriggerDto qrtzTrigger)
        {
            Dialog? loadDialog = null;
            try
            {
                var result = MessageBox.Ask($"确认删除数据？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>();
                await _quartzTriggerAppService.DeleteAsync(qrtzTrigger.Id);
                await SearchCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog?.Close();
            }
        }

        [RelayCommand]
        private async Task BatchDeleteTrigger(IList sender)
        {
            Dialog? loadDialog = null;
            try
            {
                var qrtzTriggerIds = sender.Cast<QrtzTriggerDto>().Select(x => x.Id).ToList();
                if (qrtzTriggerIds.Count == 0)
                {
                    MessageBox.Warning("未选中任何数据！");
                    return;
                }
                var result = MessageBox.Ask($"确认删除选中数据？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>();
                foreach (var qrtzTriggerId in qrtzTriggerIds)
                {
                    await _quartzTriggerAppService.DeleteAsync(qrtzTriggerId);
                }
                await SearchCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog?.Close();
            }
        }

        private async Task LoadDetailDataAsync()
        {
            var result = await _quartzTriggerAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
            {
                //Filter = Name,
                SkipCount = (DetailPageIndex - 1) * DetailDataCountPerPage,
                MaxResultCount = DetailDataCountPerPage
            });
            DetailTotalCount = result.TotalCount;
            QrtzTriggers = new ObservableCollection<QrtzTriggerDto>(result.Items);
        }
    }
}
