using Admin.Desktop.ViewModel.Identity.Users;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.Identity.Users
{
    /// <summary>
    /// UserAddView.xaml 的交互逻辑
    /// </summary>
    public partial class UserAddView : Window
    {
        private readonly UserAddVM vm;
        public UserAddView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<UserAddVM>()!;
            Loaded += async (s, e) =>
            {
                await vm.InitialAsync(this);
            };
            DataContext = vm;
        }
    }
}
