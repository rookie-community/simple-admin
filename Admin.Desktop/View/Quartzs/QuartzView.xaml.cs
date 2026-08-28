using Admin.Desktop.ViewModel.Quartzs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Admin.Desktop.View.Quartzs
{
    /// <summary>
    /// QuartzView.xaml 的交互逻辑
    /// </summary>
    public partial class QuartzView : UserControl
    {
        private readonly QuartzVM vm;
        private bool _isLoaded;
        public QuartzView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<QuartzVM>() ?? throw new ArgumentNullException(nameof(QuartzVM));
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
