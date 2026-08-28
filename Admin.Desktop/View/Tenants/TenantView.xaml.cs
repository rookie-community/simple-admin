using Admin.Desktop.ViewModel.Tenants;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Admin.Desktop.View.Tenants
{
    /// <summary>
    /// TenantView.xaml 的交互逻辑
    /// </summary>
    public partial class TenantView : UserControl
    {
        private readonly TenantVM vm;
        private bool _isLoaded;

        public TenantView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<TenantVM>()!;
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
