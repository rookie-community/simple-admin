using Admin.Desktop.View.Accounts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Account;
using Volo.Abp.DependencyInjection;

namespace Admin.Desktop.ViewModel.Accounts
{
    public partial class MyProfileVM : ObservableValidator, ITransientDependency
    {
        private readonly IProfileAppService _profileAppService;
        private readonly ILogger<MyProfileVM> _logger;

        [Required]
        [ObservableProperty]
        public partial string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "请输入新密码")]
        [MinLength(6, ErrorMessage = "密码长度至少6位")]
        [ObservableProperty]
        public partial string NewPassword { get; set; } = null!;

        [Required]
        [CustomValidation(typeof(MyProfileVM), nameof(ValidateConfirmPassword))]
        [ObservableProperty]
        public partial string ConfirmPassword { get; set; } = null!;

        [ObservableProperty]
        public partial ProfileDto Profile { get; set; } = null!;

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public MyProfileView Owner { get; private set; } = null!;

        public MyProfileVM(IProfileAppService profileAppService, ILogger<MyProfileVM> logger)
        {
            _profileAppService = profileAppService;
            _logger = logger;
        }

        internal async Task InitialAsync(MyProfileView owner)
        {
            Owner = owner;
            await LoadProfileAsync();
        }

        [RelayCommand]
        private async Task UpdatePasswordAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                ValidateAllProperties();
                if (HasErrors)
                {
                    return;
                }
                await _profileAppService.ChangePasswordAsync(new ChangePasswordInput
                {
                    CurrentPassword = CurrentPassword,
                    NewPassword = NewPassword,
                });
                Growl.Success("更新成功");
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
        private async Task UpdateProfile()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                await _profileAppService.UpdateAsync(new UpdateProfileDto
                {
                    UserName = Profile.UserName,
                    Name = Profile.Name,
                    Surname = Profile.Surname,
                    Email = Profile.Email,
                    PhoneNumber = Profile.PhoneNumber,
                });
                Growl.Success("更新成功");
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
        private async Task ResetProfileAsync()
        {
            await LoadProfileAsync();
        }

        private async Task LoadProfileAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Profile = await _profileAppService.GetAsync();
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

        public static ValidationResult ValidateConfirmPassword(string confirmPasswordValue, ValidationContext context)
        {
            var instance = (MyProfileVM)context.ObjectInstance;
            if (confirmPasswordValue != instance.NewPassword)
            {
                return new ValidationResult("两次输入的密码不一致！");
            }
            return ValidationResult.Success!;
        }
    }
}
