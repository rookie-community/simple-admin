using Admin.Desktop.ViewModel.Permissions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.Permissions
{
    /// <summary>
    /// RolePermissionEditView.xaml 的交互逻辑
    /// </summary>
    public partial class PermissionEditView : Window
    {
        private readonly PermissionEditVM vm;
        private bool _isLoaded = false;
        public PermissionEditView(string providerName, string providerKey)
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<PermissionEditVM>()!;
            Loaded += async (s, e) =>
            {
                if (_isLoaded)
                {
                    // 避免重复加载数据
                    return;
                }
                _isLoaded = true;
                await vm.InitialAsync(this, providerName, providerKey);
            };
            DataContext = vm;
        }
    }
}
