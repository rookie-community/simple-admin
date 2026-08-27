using Admin.Desktop.ViewModel.Accounts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Window = HandyControl.Controls.Window;

namespace Admin.Desktop.View.Accounts
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : Window, ITransientDependency
    {
        private readonly LoginVM? vm;
        public LoginView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<LoginVM>();
            Loaded += (s, e) =>
            {
                vm?.Initial(this);
            };
            DataContext = vm;
        }
    }
}
