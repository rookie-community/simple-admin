using Admin.Desktop.ViewModel.SettingManagement.EmailSettings;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Admin.Desktop.View.SettingManagement.EmailSettings
{
    /// <summary>
    /// EmailSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class EmailSettingView : UserControl
    {
        private readonly EmailSettingVM vm;
        private bool _isLoaded;
        public EmailSettingView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<EmailSettingVM>()!;
            Loaded += async (s, e) =>
            {
                if (_isLoaded)
                {
                    // 避免重复加载数据
                    return;
                }
                _isLoaded = true;
                await vm.InitialAsync(this);
            };
            DataContext = vm;
        }
    }
}
