using Admin.Desktop.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Admin.Desktop.View
{
    /// <summary>
    /// Console.xaml 的交互逻辑
    /// </summary>
    public partial class ConsoleView : UserControl
    {
        private readonly ConsoleVM vm;
        private bool _isLoaded;
        public ConsoleView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<ConsoleVM>()!;
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
    }
}
