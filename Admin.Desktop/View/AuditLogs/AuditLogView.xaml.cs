using Admin.Desktop.ViewModel.AuditLogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Admin.Desktop.View.AuditLogs
{
    /// <summary>
    /// AuditLogView.xaml 的交互逻辑
    /// </summary>
    public partial class AuditLogView : UserControl
    {
        private readonly AuditLogVM vm;
        private bool _isLoaded;

        public AuditLogView()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<AuditLogVM>()!;
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
