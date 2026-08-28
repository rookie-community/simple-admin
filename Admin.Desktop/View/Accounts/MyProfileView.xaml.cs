using Admin.Desktop.ViewModel.Accounts;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Admin.Desktop.View.Accounts
{
    /// <summary>
    /// ManageView.xaml 的交互逻辑
    /// </summary>
    public partial class MyProfileView : UserControl
    {
        private readonly MyProfileVM vm;
        private bool _isLoaded;

        public MyProfileView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<MyProfileVM>()!;
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
