using Admin.Desktop.Tools;
using Admin.Desktop.View.Identity.Users;
using Admin.Desktop.View.Permissions;
using Admin.Desktop.View.Users;
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

namespace Admin.Desktop.ViewModel.Users
{
    public partial class UserVM : ObservableObject, ITransientDependency
    {
        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<IdentityUserDto> Users { get; set; } = new ObservableCollection<IdentityUserDto>();

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

        public UserView Owner { get; private set; } = null!;

        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly ILogger<UserVM> _logger;

        public UserVM(IIdentityUserAppService identityUserAppService, ILogger<UserVM> logger)
        {
            _identityUserAppService = identityUserAppService;
            _logger = logger;
            BtnPerms = ButtonPermissionBuilder.Build(BtnPermissionMap);
        }

        internal async Task InitialAsync(UserView owner)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                await LoadDataAsync();
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

        /// <summary>
        /// 页面按钮键名 -> 权限编码 的映射。
        /// 键名需与 UserView 中 Visibility="{Binding BtnPerms[键名]}" 的绑定一致。
        /// </summary>
        private static readonly Dictionary<string, string> BtnPermissionMap = new()
        {
            { "Create", IdentityPermissions.Users.Create },
            { "Update", IdentityPermissions.Users.Update },
            { "ManagePermissions", IdentityPermissions.Users.ManagePermissions },
            { "Delete", IdentityPermissions.Users.Delete },
        };

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
        private async Task AddAsync()
        {
            try
            {
                var view = new UserAddView();
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
        private async Task EditAsync(IdentityUserDto user)
        {
            try
            {
                var view = new UserEditView(user.Id);
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
        private void EditPerm(IdentityUserDto user)
        {
            var view = new PermissionEditView("U", user.UserName);
            var result = view.ShowDialog();
            if (result == true)
            {
                Growl.Success("更新权限成功");
            }
        }

        [RelayCommand]
        private async Task Delete(IdentityUserDto user)
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
                await _identityUserAppService.DeleteAsync(user.Id);
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
        private async Task BatchDelete(IList sender)
        {
            Dialog? loadDialog = null;
            try
            {
                var userIds = sender.Cast<IdentityUserDto>().Select(x => x.Id).ToList();
                if (userIds.Count == 0)
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
                foreach (var userId in userIds)
                {
                    await _identityUserAppService.DeleteAsync(userId);
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
        private async Task PageChangedAsync()
        {
            await SearchCommand.ExecuteAsync(null);
        }

        private async Task LoadDataAsync()
        {
            var result = await _identityUserAppService.GetListAsync(new GetIdentityUsersInput
            {
                Filter = Name,
                SkipCount = (PageIndex - 1) * DataCountPerPage,
                MaxResultCount = DataCountPerPage
            });
            TotalCount = result.TotalCount;
            Users = new ObservableCollection<IdentityUserDto>(result.Items);
        }
    }
}
