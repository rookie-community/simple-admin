using System.Windows.Controls;
using Admin.Desktop.ViewModel.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Desktop.View.Reports
{
    /// <summary>
    /// ReportDesign.xaml 的交互逻辑
    /// </summary>
    public partial class ReportDesign : UserControl
    {
        private readonly ReportDesignVM vm;
        private bool _isLoaded = false;
        public ReportDesign()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<ReportDesignVM>()!;
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
