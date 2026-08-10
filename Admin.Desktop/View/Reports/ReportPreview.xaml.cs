using System.IO;
using Admin.Desktop.ViewModel.Reports;
using Microsoft.Extensions.DependencyInjection;
using Window = HandyControl.Controls.Window;

namespace Admin.Desktop.View.Reports
{
    /// <summary>
    /// ReportPreview.xaml 的交互逻辑
    /// </summary>
    public partial class ReportPreview : Window
    {
        private readonly ReportPreviewVM vm;

        private bool _isLoaded = false;

        public ReportPreview(string reportName)
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<ReportPreviewVM>()!;
            Loaded += async (s, e) =>
            {
                if (_isLoaded)
                {
                    // 避免重复加载数据
                    return;
                }
                _isLoaded = true;
                if (string.IsNullOrWhiteSpace(reportName))
                {
                    reportName = "About Microsoft Chart.frx";
                }
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Reports", reportName);
                await vm.InitialAsync(this, filePath);
            };

            DataContext = vm;
        }
    }
}
