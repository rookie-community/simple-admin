using Admin.Desktop.ViewModel.SettingManagement.EmailSettings;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.SettingManagement.EmailSettings
{
    /// <summary>
    /// SendTestEmailView.xaml 的交互逻辑
    /// </summary>
    public partial class SendTestEmailView : Window
    {
        private readonly SendTestEmailVM vm;

        public SendTestEmailView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<SendTestEmailVM>()!;
            Loaded += (s, e) =>
            {
                vm.Initial(this);
            };
            DataContext = vm;
        }
    }
}
