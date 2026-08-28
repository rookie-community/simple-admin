using Admin.Desktop.ViewModel.Tenants;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.Tenants
{
    /// <summary>
    /// TenantEditView.xaml 的交互逻辑
    /// </summary>
    public partial class TenantEditView : Window
    {
        private readonly TenantEditVM vm;
        public TenantEditView(Guid tenantId)
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<TenantEditVM>()!;
            Loaded += async (s, e) =>
            {
                await vm.InitialAsync(this, tenantId);
            };
            DataContext = vm;
        }
    }
}
