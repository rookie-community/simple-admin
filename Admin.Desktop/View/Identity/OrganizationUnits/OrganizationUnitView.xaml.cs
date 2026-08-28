using Admin.Desktop.ViewModel.Identity.OrganizationUnits;
using Admin.OrganizationUnits;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace Admin.Desktop.View.Identity.OrganizationUnits
{
    /// <summary>
    /// OrganizationUnitView.xaml 的交互逻辑
    /// </summary>
    public partial class OrganizationUnitView : UserControl
    {
        private readonly OrganizationUnitVM vm;
        private bool _isLoaded;

        public OrganizationUnitView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<OrganizationUnitVM>()!;
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

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is OrganizationUnitDto selectedUnit)
            {
                vm.SelectedOrganizationUnit = selectedUnit;
            }
        }
    }
}
