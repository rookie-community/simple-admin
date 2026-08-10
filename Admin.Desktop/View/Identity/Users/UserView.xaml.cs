using System.Windows.Controls;
using Admin.Desktop.ViewModel.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Desktop.View.Users
{
    /// <summary>
    /// UserView.xaml 的交互逻辑
    /// </summary>
    public partial class UserView : UserControl
    {
        private readonly UserVM vm;
        private bool _isLoaded;
        public UserView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<UserVM>()!;
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
