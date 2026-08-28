using System.Windows;
using Admin.Desktop.ViewModel.Identity.Roles;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Desktop.View.Identity.Roles
{
    /// <summary>
    /// RoleEditView.xaml 的交互逻辑
    /// </summary>
    public partial class RoleEditView : Window
    {
        private readonly RoleEditVM vm;
        public RoleEditView(Guid roleId)
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<RoleEditVM>() ?? throw new ArgumentNullException(nameof(RoleEditVM));
            Loaded += async (s, e) =>
            {
               await vm.InitialAsync(this, roleId);
            };
            DataContext = vm;
        }
    }
}
