using Admin.Desktop.View.SettingManagement.EmailSettings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Admin.Desktop.ViewModel.SettingManagement.EmailSettings
{
    public partial class EmailSettingVM : ObservableRecipient, ITransientDependency
    {
        [ObservableProperty]
        public partial EmailSettingsDto EmailSettings { get; set; } = null!;

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial Dictionary<string, Visibility> BtnPerms { get; set; } = new Dictionary<string, Visibility>();

        public EmailSettingView Owner { get; private set; } = null!;

        private readonly IEmailSettingsAppService _emailSettingsAppService;
        private readonly ILogger<EmailSettingVM> _logger;

        public EmailSettingVM(IEmailSettingsAppService emailSettingsAppService, ILogger<EmailSettingVM> logger)
        {
            _emailSettingsAppService = emailSettingsAppService;
            _logger = logger;
            LoadButtonPermissions();
        }

        internal async Task InitialAsync(EmailSettingView owner)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                EmailSettings = await _emailSettingsAppService.GetAsync();
                await Task.Delay(1);
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
                { "EmailingTest",SettingManagementPermissions.EmailingTest}
            };

            foreach (var btnTemp in btnTemps)
            {
                var isGranted = App.PermissionChecker(btnTemp.Value);
                BtnPerms.TryAdd(btnTemp.Key, isGranted);
            }
        }

        [RelayCommand]
        public void SendTestEmail()
        {
            var view = new SendTestEmailView();
            var result = view.ShowDialog();
            if (result == true)
            {
                Growl.Success("发送成功");
            }
        }

        [RelayCommand]
        public async Task SubmitAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                await _emailSettingsAppService.UpdateAsync(new UpdateEmailSettingsDto
                {
                    DefaultFromDisplayName = EmailSettings.DefaultFromDisplayName,
                    DefaultFromAddress = EmailSettings.DefaultFromAddress,
                    SmtpHost = EmailSettings.SmtpHost,
                    SmtpPort = EmailSettings.SmtpPort,
                    SmtpEnableSsl = EmailSettings.SmtpEnableSsl,
                    SmtpUseDefaultCredentials = EmailSettings.SmtpUseDefaultCredentials,
                    SmtpDomain = EmailSettings.SmtpDomain,
                    SmtpUserName = EmailSettings.SmtpUserName,
                    SmtpPassword = EmailSettings.SmtpPassword,
                });
                Growl.Success("保存成功");
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
