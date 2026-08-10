using System.Windows;
using Admin.Desktop.ViewModel.Identity.OrganizationUnits;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Desktop.View.Identity.OrganizationUnits
{
    /// <summary>
    /// OrganizationUnitAddRoleView.xaml 的交互逻辑
    /// </summary>
    public partial class OrganizationUnitAddRoleView : Window
    {
        private readonly OrganizationUnitAddRoleVM vm;

        public OrganizationUnitAddRoleView(Guid organizationUnitId)
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<OrganizationUnitAddRoleVM>()!;
            Loaded += async (s, e) =>
            {
                await vm.InitialAsync(this, organizationUnitId);
            };
            DataContext = vm;
        }
    }
}
