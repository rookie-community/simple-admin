using Admin.Desktop.ViewModel.Identity.OrganizationUnits;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.Identity.OrganizationUnits
{
    /// <summary>
    /// OrganizationUnitAddUserView.xaml 的交互逻辑
    /// </summary>
    public partial class OrganizationUnitAddUserView : Window
    {
        private readonly OrganizationUnitAddUserVM vm;

        public OrganizationUnitAddUserView(Guid organizationUnitId)
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<OrganizationUnitAddUserVM>()!;
            Loaded += async (s, e) =>
            {
                await vm.InitialAsync(this, organizationUnitId);
            };
            DataContext = vm;
        }
    }
}
