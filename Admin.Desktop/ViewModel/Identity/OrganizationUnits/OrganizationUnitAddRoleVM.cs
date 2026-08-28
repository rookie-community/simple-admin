using Admin.Desktop.View.Identity.OrganizationUnits;
using Admin.OrganizationUnits;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.ObjectModel;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Identity;

namespace Admin.Desktop.ViewModel.Identity.OrganizationUnits
{
    public partial class OrganizationUnitAddRoleVM : ObservableObject, ITransientDependency
    {
        private readonly IOrganizationUnitAppService _organizationUnitAppService;
        private readonly IIdentityRoleAppService _identityRoleAppService;
        private readonly ILogger<OrganizationUnitAddRoleVM> _logger;

        [ObservableProperty]
        public partial string FilterText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<IdentityRoleDto> AvailableRoles { get; set; } = new ObservableCollection<IdentityRoleDto>();

        [ObservableProperty]
        public partial int PageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long TotalCount { get; set; }

        [ObservableProperty]
        public partial int DataCountPerPage { get; set; } = 20;

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public OrganizationUnitAddRoleView Owner { get; private set; } = null!;
        public Guid OrganizationUnitId { get; private set; }

        public OrganizationUnitAddRoleVM(
            IOrganizationUnitAppService organizationUnitAppService,
            IIdentityRoleAppService identityRoleAppService,
            ILogger<OrganizationUnitAddRoleVM> logger)
        {
            _organizationUnitAppService = organizationUnitAppService;
            _identityRoleAppService = identityRoleAppService;
            _logger = logger;
        }

        internal async Task InitialAsync(OrganizationUnitAddRoleView owner, Guid organizationUnitId)
        {
            Owner = owner;
            OrganizationUnitId = organizationUnitId;
            await LoadRolesAsync();
        }

        [RelayCommand]
        private async Task SearchAsync(string text)
        {
            PageIndex = 1;
            FilterText = text;
            await LoadRolesAsync();
        }

        [RelayCommand]
        private void Reset()
        {
            FilterText = string.Empty;
            PageIndex = 1;
            _ = LoadRolesAsync();
        }

        [RelayCommand]
        private async Task SaveAsync(IList sender)
        {
            var roles = sender.Cast<IdentityRoleDto>().ToList();
            if (roles.Count == 0)
            {
                MessageBox.Warning("请至少选择一个要添加的角色");
                return;
            }

            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                foreach (var role in roles)
                {
                    await _organizationUnitAppService.AddRoleAsync(OrganizationUnitId, role.Id);
                }
                Owner.DialogResult = true;
                Owner.Close();
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
        private void Cancel()
        {
            Owner.Close();
        }

        [RelayCommand]
        private async Task PageChangedAsync()
        {
            await LoadRolesAsync();
        }

        private async Task LoadRolesAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                var result = await _identityRoleAppService.GetListAsync(new GetIdentityRolesInput
                {
                    Filter = FilterText,
                    SkipCount = (PageIndex - 1) * DataCountPerPage,
                    MaxResultCount = DataCountPerPage
                });

                TotalCount = result.TotalCount;
                AvailableRoles = new ObservableCollection<IdentityRoleDto>(result.Items);
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
    }
}
