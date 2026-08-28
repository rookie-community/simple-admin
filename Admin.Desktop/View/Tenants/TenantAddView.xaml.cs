using Admin.Desktop.ViewModel.Tenants;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.Tenants
{
    /// <summary>
    /// TenantAddView.xaml 的交互逻辑
    /// </summary>
    public partial class TenantAddView : Window
    {
        private readonly TenantAddVM vm;
        public TenantAddView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<TenantAddVM>()!;
            Loaded += (s, e) =>
            {
                vm.Initial(this);
            };
            DataContext = vm;
        }
    }
}
