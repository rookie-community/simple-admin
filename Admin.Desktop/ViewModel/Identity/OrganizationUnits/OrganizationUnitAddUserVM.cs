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
    public partial class OrganizationUnitAddUserVM : ObservableObject, ITransientDependency
    {
        private readonly IOrganizationUnitAppService _organizationUnitAppService;
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly ILogger<OrganizationUnitAddUserVM> _logger;

        [ObservableProperty]
        public partial string FilterText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<IdentityUserDto> AvailableUsers { get; set; } = new ObservableCollection<IdentityUserDto>();

        [ObservableProperty]
        public partial int PageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long TotalCount { get; set; }

        [ObservableProperty]
        public partial int DataCountPerPage { get; set; } = 20;

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public OrganizationUnitAddUserView Owner { get; private set; } = null!;
        public Guid OrganizationUnitId { get; private set; }

        public OrganizationUnitAddUserVM(
            IOrganizationUnitAppService organizationUnitAppService,
            IIdentityUserAppService identityUserAppService,
            ILogger<OrganizationUnitAddUserVM> logger)
        {
            _organizationUnitAppService = organizationUnitAppService;
            _identityUserAppService = identityUserAppService;
            _logger = logger;
        }

        internal async Task InitialAsync(OrganizationUnitAddUserView owner, Guid organizationUnitId)
        {
            Owner = owner;
            OrganizationUnitId = organizationUnitId;
            await LoadUsersAsync();
        }

        [RelayCommand]
        private async Task SearchAsync(string text)
        {
            PageIndex = 1;
            FilterText = text;
            await LoadUsersAsync();
        }

        [RelayCommand]
        private void Reset()
        {
            FilterText = string.Empty;
            PageIndex = 1;
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task SaveAsync(IList sender)
        {
            var users = sender.Cast<IdentityUserDto>().ToList();
            if (users.Count == 0)
            {
                MessageBox.Warning("请至少选择一个要添加的成员");
                return;
            }

            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                foreach (var user in users)
                {
                    await _organizationUnitAppService.AddUserAsync(OrganizationUnitId, user.Id);
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
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                var result = await _identityUserAppService.GetListAsync(new GetIdentityUsersInput
                {
                    Filter = FilterText,
                    SkipCount = (PageIndex - 1) * DataCountPerPage,
                    MaxResultCount = DataCountPerPage
                });

                TotalCount = result.TotalCount;
                AvailableUsers = new ObservableCollection<IdentityUserDto>(result.Items);
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
