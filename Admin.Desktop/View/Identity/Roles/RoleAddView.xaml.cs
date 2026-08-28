using Admin.Desktop.ViewModel.Identity.Roles;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.Identity.Roles
{
    /// <summary>
    /// RoleAddView.xaml 的交互逻辑
    /// </summary>
    public partial class RoleAddView : Window
    {
        private readonly RoleAddVM vm;
        public RoleAddView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<RoleAddVM>()!;
            Loaded += (s, e) =>
            {
                vm.Initial(this);
            };
            DataContext = vm;
        }
    }
}
