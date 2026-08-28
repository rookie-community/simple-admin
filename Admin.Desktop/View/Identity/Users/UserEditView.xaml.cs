using Admin.Desktop.ViewModel.Identity.Users;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Admin.Desktop.View.Identity.Users
{
    /// <summary>
    /// UserEditView.xaml 的交互逻辑
    /// </summary>
    public partial class UserEditView : Window
    {
        private readonly UserEditVM vm;

        public UserEditView(Guid userId)
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<UserEditVM>()!;
            Loaded += async (s, e) =>
            {
                await vm.InitialAsync(this, userId);
            };
            DataContext = vm;
        }
    }
}
