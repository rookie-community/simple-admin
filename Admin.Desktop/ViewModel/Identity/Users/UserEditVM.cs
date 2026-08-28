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
    public partial class UserEditVM : ObservableObject, ITransientDependency
    {
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly ILogger<UserEditVM> _logger;
        private Guid _userId;

        [ObservableProperty]
        public partial string CurrentPassword { get; set; } = string.Empty;

        [ObservableProperty]
        public partial IdentityUserDto User { get; set; } = null!;

        [ObservableProperty]
        public partial IdentityUserDto CreatorUser { get; set; } = null!;

        [ObservableProperty]
        public partial IdentityUserDto LastModifierUser { get; set; } = null!;

        [ObservableProperty]
        public partial ObservableCollection<IdentityRoleDto> Roles { get; set; } = new ObservableCollection<IdentityRoleDto>();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public UserEditView Owner { get; private set; } = null!;

        public UserEditVM(IIdentityUserAppService identityUserAppService, ILogger<UserEditVM> logger)
        {
            _identityUserAppService = identityUserAppService;
            _logger = logger;
        }

        internal async Task InitialAsync(UserEditView owner, Guid userId)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                _userId = userId;
                User = await _identityUserAppService.GetAsync(userId);
                if (User.CreatorId != null)
                {
                    CreatorUser = await _identityUserAppService.GetAsync((Guid)User.CreatorId);
                }
                if (User.LastModifierId != null)
                {
                    LastModifierUser = await _identityUserAppService.GetAsync((Guid)User.LastModifierId);
                }

                var assignableRoleResult = await _identityUserAppService.GetAssignableRolesAsync();
                var roleResult = await _identityUserAppService.GetRolesAsync(userId);
                var allRoles = assignableRoleResult.Items;
                var userRoles = roleResult.Items;
                foreach (var roleItem in allRoles)
                {
                    if (userRoles.Any(x => x.Id == roleItem.Id))
                    {
                        roleItem.IsDefault = true;
                    }
                }

                Roles = new ObservableCollection<IdentityRoleDto>(allRoles);
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
                var result = await _identityUserAppService.UpdateAsync(_userId, new IdentityUserUpdateDto
                {
                    UserName = User.UserName,
                    Name = User.Name,
                    Surname = User.Surname,
                    Password = CurrentPassword,
                    Email = User.Email,
                    PhoneNumber = User.PhoneNumber,
                    IsActive = User.IsActive,
                    LockoutEnabled = User.LockoutEnabled,
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
