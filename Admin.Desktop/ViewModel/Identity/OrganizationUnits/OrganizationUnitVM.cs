using Admin.Desktop.View.Identity.OrganizationUnits;
using Admin.OrganizationUnits;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Admin.Desktop.ViewModel.Identity.OrganizationUnits
{
    public partial class OrganizationUnitVM : ObservableObject, ITransientDependency
    {
        private readonly IOrganizationUnitAppService _organizationUnitAppService;
        private readonly ILogger<OrganizationUnitVM> _logger;

        [ObservableProperty]
        public partial ObservableCollection<OrganizationUnitDto> OrganizationUnits { get; set; } = new ObservableCollection<OrganizationUnitDto>();

        [ObservableProperty]
        public partial OrganizationUnitDto? SelectedOrganizationUnit { get; set; }

        [ObservableProperty]
        public partial int UserPageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long UserTotalCount { get; set; }

        [ObservableProperty]
        public partial int UserDataCountPerPage { get; set; } = 30;

        [ObservableProperty]
        public partial ObservableCollection<IdentityUserDto> Users { get; set; } = new ObservableCollection<IdentityUserDto>();

        [ObservableProperty]
        public partial int RolePageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long RoleTotalCount { get; set; }

        [ObservableProperty]
        public partial int RoleDataCountPerPage { get; set; } = 30;

        [ObservableProperty]
        public partial ObservableCollection<IdentityRoleDto> Roles { get; set; } = new ObservableCollection<IdentityRoleDto>();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial Dictionary<string, Visibility> BtnPerms { get; set; } = new Dictionary<string, Visibility>();

        public OrganizationUnitView Owner { get; private set; } = null!;

        public OrganizationUnitVM(IOrganizationUnitAppService organizationUnitAppService, ILogger<OrganizationUnitVM> logger)
        {
            this._organizationUnitAppService = organizationUnitAppService;
            _logger = logger;
        }

        internal async Task InitialAsync(OrganizationUnitView owner)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                await LoadOrganizationUnitsAsync();
            }
            catch (AbpRemoteCallException abpRemoteCallException)
            {
                _logger.LogException(abpRemoteCallException);
                MessageBox.Error(abpRemoteCallException.Details, abpRemoteCallException.Message);
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
        private async Task SearchAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                await LoadOrganizationUnitsAsync();
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
        private async Task AddRootAsync()
        {
            try
            {
                var view = new OrganizationUnitAddView();
                var result = view.ShowDialog();
                if (result == true)
                {
                    Growl.Success("新增成功");
                    await LoadOrganizationUnitsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task AddChildAsync(OrganizationUnitDto parent)
        {
            try
            {
                // TODO: 打开添加子机构对话框
                Growl.Warning("功能开发中");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task EditAsync(OrganizationUnitDto organizationUnit)
        {
            try
            {
                // TODO: 打开编辑机构对话框
                Growl.Warning("功能开发中");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task DeleteAsync(OrganizationUnitDto organizationUnit)
        {
            Dialog? loadDialog = null;
            try
            {
                var result = MessageBox.Ask($"确认删除机构【{organizationUnit.DisplayName}】？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>();
                // TODO: 调用删除接口
                // await _organizationUnitAppService.DeleteAsync(organizationUnit.Id);
                await LoadOrganizationUnitsAsync();
                Growl.Success("删除成功");
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
        private async Task AddMemberAsync()
        {
            try
            {
                if (SelectedOrganizationUnit == null)
                {
                    MessageBox.Warning("请先选择组织机构");
                    return;
                }
                var view = new OrganizationUnitAddUserView(SelectedOrganizationUnit.Id);
                var result = view.ShowDialog();
                if (result == true)
                {
                    Growl.Success("添加成功");
                    UserPageIndex = 1;
                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task DeleteMemberAsync(IdentityUserDto user)
        {
            Dialog? loadDialog = null;
            try
            {
                if (SelectedOrganizationUnit == null)
                {
                    MessageBox.Warning("请先选择组织机构");
                    return;
                }
                var result = MessageBox.Ask($"确认从机构中移除用户【{user.UserName}】？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>();
                await _organizationUnitAppService.RemoveUserAsync(SelectedOrganizationUnit.Id, user.Id);
                await LoadUsersAsync();
                Growl.Success("移除成功");
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
        private async Task AddRoleAsync()
        {
            try
            {
                if (SelectedOrganizationUnit == null)
                {
                    MessageBox.Warning("请先选择组织机构");
                    return;
                }
                var view = new OrganizationUnitAddRoleView(SelectedOrganizationUnit.Id);
                var result = view.ShowDialog();
                if (result == true)
                {
                    Growl.Success("添加成功");
                    RolePageIndex = 1;
                    await LoadRolesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private async Task DeleteRoleAsync(IdentityRoleDto role)
        {
            Dialog? loadDialog = null;
            try
            {
                if (SelectedOrganizationUnit == null)
                {
                    MessageBox.Warning("请先选择组织机构");
                    return;
                }
                var result = MessageBox.Ask($"确认从机构中移除角色【{role.Name}】？");
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                loadDialog = Dialog.Show<LoadingCircle>();
                await _organizationUnitAppService.RemoveRoleAsync(SelectedOrganizationUnit.Id, role.Id);
                await LoadRolesAsync();
                Growl.Success("移除成功");
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
        private async Task UserPageChangedAsync()
        {
            await LoadUsersAsync();
        }

        [RelayCommand]
        private async Task RolePageChangedAsync()
        {
            await LoadRolesAsync();
        }

        partial void OnSelectedOrganizationUnitChanged(OrganizationUnitDto? value)
        {
            if (value != null)
            {
                UserPageIndex = 1;
                RolePageIndex = 1;
                _ = LoadUsersAsync();
                _ = LoadRolesAsync();
            }
        }

        private async Task LoadOrganizationUnitsAsync()
        {
            var result = await _organizationUnitAppService.GetListAsync(new GetOrganizationUnitsInput
            {
                // TODO: 添加过滤条件支持
            });
            OrganizationUnits = new ObservableCollection<OrganizationUnitDto>(result.Items);
        }

        private async Task LoadUsersAsync()
        {
            if (SelectedOrganizationUnit == null)
            {
                Users = new ObservableCollection<IdentityUserDto>();
                UserTotalCount = 0;
                return;
            }

            var result = await _organizationUnitAppService.GetUsersAsync(new GetOrganizationUnitUsersInput
            {
                OrganizationUnitId = SelectedOrganizationUnit.Id,
                SkipCount = (UserPageIndex - 1) * UserDataCountPerPage,
                MaxResultCount = UserDataCountPerPage
            });
            UserTotalCount = result.TotalCount;
            Users = new ObservableCollection<IdentityUserDto>(result.Items);
        }

        private async Task LoadRolesAsync()
        {
            if (SelectedOrganizationUnit == null)
            {
                Roles = new ObservableCollection<IdentityRoleDto>();
                RoleTotalCount = 0;
                return;
            }

            var result = await _organizationUnitAppService.GetRolesAsync(new GetOrganizationUnitRolesInput
            {
                OrganizationUnitId = SelectedOrganizationUnit.Id,
                SkipCount = (RolePageIndex - 1) * RoleDataCountPerPage,
                MaxResultCount = RoleDataCountPerPage
            });
            RoleTotalCount = result.TotalCount;
            Roles = new ObservableCollection<IdentityRoleDto>(result.Items);
        }
    }
}
