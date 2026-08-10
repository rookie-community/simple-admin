using System.Windows;
using Admin.Desktop.ViewModel.Identity.OrganizationUnits;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Desktop.View.Identity.OrganizationUnits
{
    /// <summary>
    /// OrganizationUnitAddView.xaml 的交互逻辑
    /// </summary>
    public partial class OrganizationUnitAddView : Window
    {
        private readonly OrganizationUnitAddVM vm;

        public OrganizationUnitAddView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<OrganizationUnitAddVM>()!;
            Loaded += (s, e) =>
            {
                vm.Initial(this);
            };
            DataContext = vm;
        }
    }
}
