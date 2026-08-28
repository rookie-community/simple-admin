using Admin.Desktop.ViewModel.Permissions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Admin.Desktop.View.Permissions
{
    /// <summary>
    /// RoleView.xaml 的交互逻辑
    /// </summary>
    public partial class RoleView : UserControl
    {
        private readonly RoleVM vm;
        private bool _isLoaded = false; // 标记是否已经加载过数据
        public RoleView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<RoleVM>()!;
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
