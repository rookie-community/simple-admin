using Admin.Desktop.View.Identity.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Identity;

namespace Admin.Desktop.ViewModel.Identity.Users
{
    public partial class UserAddVM : ObservableObject, ITransientDependency
    {
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly ILogger<UserAddVM> _logger;

        [ObservableProperty]
        public partial string UserName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Surname { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CurrentPassword { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string PhoneNumber { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsActive { get; set; }

        [ObservableProperty]
        public partial bool LockoutEnabled { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<IdentityRoleDto> Roles { get; set; } = new ObservableCollection<IdentityRoleDto>();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public UserAddView Owner { get; private set; } = null!;

        public UserAddVM(IIdentityUserAppService identityUserAppService, ILogger<UserAddVM> logger)
        {
            _identityUserAppService = identityUserAppService;
            _logger = logger;
        }

        internal async Task InitialAsync(UserAddView owner)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                var result = await _identityUserAppService.GetAssignableRolesAsync();
                Roles = new ObservableCollection<IdentityRoleDto>(result.Items);
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
        private async Task SaveAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                var roleNames = Roles.Where(x => x.IsDefault).Select(x => x.Name).ToArray();
                var result = await _identityUserAppService.CreateAsync(new IdentityUserCreateDto
                {
                    UserName = UserName,
                    Name = Name,
                    Surname = Surname,
                    Password = CurrentPassword,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    IsActive = IsActive,
                    LockoutEnabled = LockoutEnabled,
                    RoleNames = roleNames,
                });
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
    }
}
