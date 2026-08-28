using Admin.Desktop.View.Identity.Roles;
using Admin.Desktop.View.Permissions;
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

namespace Admin.Desktop.ViewModel.Permissions
{
    public partial class RoleVM : ObservableObject, ITransientDependency
    {
        private readonly IIdentityRoleAppService _identityRoleAppService;
        private readonly ILogger<RoleVM> _logger;

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int PageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long TotalCount { get; set; }

        [ObservableProperty]
        public partial int DataCountPerPage { get; set; } = 30;

        [ObservableProperty]
        public partial ObservableCollection<IdentityRoleDto> Roles { get; set; } = new ObservableCollection<IdentityRoleDto>();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial Dictionary<string, Visibility> BtnPerms { get; set; } = new Dictionary<string, Visibility>();

        public RoleView Owner { get; private set; } = null!;

        public RoleVM(IIdentityRoleAppService identityRoleAppService, ILogger<RoleVM> logger)
        {
            _identityRoleAppService = identityRoleAppService;
            _logger = logger;
            LoadButtonPermissions();
        }

        internal async Task InitialAsync(RoleView owner)
        {
            Owner = owner;
            await SearchCommand.ExecuteAsync(null);
        }

        private void LoadButtonPermissions()
        {
            var btnTemps = new Dictionary<string, string>
            {
                { "Create",IdentityPermissions.Roles.Create},
                { "Update",IdentityPermissions.Roles.Update },
                { "ManagePermissions", IdentityPermissions.Roles.ManagePermissions },
                { "Delete", IdentityPermissions.Roles.Delete },
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
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog.Close();
            }
        }

        [RelayCommand]
        private async Task PageChangedAsync()
        {
            await SearchCommand.ExecuteAsync(null);
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
                var view = new RoleAddView();
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
        private async Task EditAsync(IdentityRoleDto role)
        {
            try
            {
                var view = new RoleEditView(role.Id);
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
        private void EditPerm(IdentityRoleDto role)
        {
            var view = new PermissionEditView("R", role.Name);
            var result = view.ShowDialog();
            if (result == true)
            {
                Growl.Success("更新权限成功");
            }
        }

        [RelayCommand]
        private async Task Delete(IdentityRoleDto role)
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
                await _identityRoleAppService.DeleteAsync(role.Id);
                await LoadDataAsync();
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
                var roleIds = sender.Cast<IdentityRoleDto>().Select(x => x.Id).ToList();
                if (roleIds.Count == 0)
                {
                    MessageBox.Warning("未选中任何数据！");
                    return;
                }
                var result = MessageBox.Ask($"确认删除选中数据？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
                foreach (var roleId in roleIds)
                {
                    await _identityRoleAppService.DeleteAsync(roleId);
                }
                await LoadDataAsync();
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

        private async Task LoadDataAsync()
        {
            var result = await _identityRoleAppService.GetListAsync(new GetIdentityRolesInput
            {
                Filter = Name,
                SkipCount = (PageIndex - 1) * DataCountPerPage,
                MaxResultCount = DataCountPerPage
            });
            TotalCount = result.TotalCount;
            Roles = new ObservableCollection<IdentityRoleDto>(result.Items);
        }
    }
}
