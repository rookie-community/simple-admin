using Admin.Desktop.View.Reports;
using CommunityToolkit.Mvvm.ComponentModel;
using FastReport;
using FastReport.Design;
using FastReport.Utils;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.IO;
using Volo.Abp.DependencyInjection;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Admin.Desktop.ViewModel.Reports
{
    public partial class ReportDesignVM : ObservableObject, ITransientDependency
    {
        private readonly ILogger<ReportDesignVM> _logger;

        [ObservableProperty]
        public partial Report Report { get; set; } = new Report();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public ReportDesign Owner { get; private set; } = null!;

        public ReportDesignVM(ILogger<ReportDesignVM> logger)
        {
            _logger = logger;
        }

        public async Task InitialAsync(ReportDesign owner, string? fileName = null)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                InitialDesignerSettings();
                await Task.Run(() =>
                {
                    if (fileName == null)
                    {
                        //加载报表
                        var reportName = "About Microsoft Chart.frx";
                        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Reports", reportName);
                        Report.Load(filePath);
                        //创建数据源
                        //var list = CreateBusinessObject();
                        //绑定数据源
                        //report.RegisterData(list, "Categories");
                    }
                });
                Owner.designerControl.Report = Report;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog.Close();
            }
        }

        public async Task InitialAsync(ReportDesign owner, Stream stream)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                InitialDesignerSettings();
                await Task.Run(() =>
                {
                    Report.Load(stream);
                    Owner.designerControl.Report = Report;
                });
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog.Close();
            }
        }

        private void InitialDesignerSettings()
        {
            //配置是全局的，每次打开重新初始化一下，防止事件重复触发
            Config.DesignerSettings = new DesignerSettings();
            Config.DesignerSettings.CustomSaveReport += (s, e) =>
            {
                var templateBase64 = e.Report.SaveToStringBase64();
                MessageBox.Info("这是自定义保存方法");
            };
        }
    }
}
