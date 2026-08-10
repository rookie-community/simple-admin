using System.Collections.ObjectModel;
using System.Windows;
using Admin.Desktop.View.Tenants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.TenantManagement;
using Volo.Abp.Validation;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Admin.Desktop.ViewModel.Tenants
{
    public partial class TenantVM : ObservableObject, ITransientDependency
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly ILogger<TenantVM> _logger;

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<TenantDto> Tenants { get; set; } = new ObservableCollection<TenantDto>();

        [ObservableProperty]
        public partial int PageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long TotalCount { get; set; }

        [ObservableProperty]
        public partial int DataCountPerPage { get; set; } = 30;

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial Dictionary<string, Visibility> BtnPerms { get; set; } = new Dictionary<string, Visibility>();

        public TenantView Owner { get; private set; } = null!;

        public TenantVM(ITenantAppService tenantAppService, ILogger<TenantVM> logger)
        {
            _tenantAppService = tenantAppService;
            _logger = logger;
            LoadButtonPermissions();
        }

        internal async Task InitialAsync(TenantView owner)
        {
            Owner = owner;
            await SearchCommand.ExecuteAsync(null);
        }

        private void LoadButtonPermissions()
        {
            var btnTemps = new Dictionary<string, string>
            {
                { "Create",TenantManagementPermissions.Tenants.Create},
                { "Update",TenantManagementPermissions.Tenants.Update },
                { "ManageFeatures", TenantManagementPermissions.Tenants.ManageFeatures },
                { "Delete", TenantManagementPermissions.Tenants.Delete },
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
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                await LoadDataAsync();
            }
            catch (AbpValidationException abpEx)
            {
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
        private async Task AddAsync()
        {
            try
            {
                var view = new TenantAddView();
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
        private async Task EditAsync(TenantDto tenant)
        {
            try
            {
                var view = new TenantEditView(tenant.Id);
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
        private void ManageFeature()
        {
            MessageBox.Warning("暂未开放");
        }

        [RelayCommand]
        private async Task DeleteAsync(TenantDto tenant)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                await _tenantAppService.DeleteAsync(tenant.Id);
                await LoadDataAsync();
            }
            catch (AbpValidationException abpEx)
            {
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
        private async Task PageChangedAsync()
        {
            await SearchCommand.ExecuteAsync(null);
        }

        private async Task LoadDataAsync()
        {
            var result = await _tenantAppService.GetListAsync(new GetTenantsInput
            {
                Filter = Name,
                SkipCount = (PageIndex - 1) * DataCountPerPage,
                MaxResultCount = DataCountPerPage
            });
            TotalCount = result.TotalCount;
            Tenants = new ObservableCollection<TenantDto>(result.Items);
        }
    }
}
