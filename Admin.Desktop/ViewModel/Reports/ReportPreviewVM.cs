using Admin.Desktop.View.Reports;
using CommunityToolkit.Mvvm.ComponentModel;
using FastReport;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace Admin.Desktop.ViewModel.Reports
{
    public partial class ReportPreviewVM : ObservableObject, ITransientDependency
    {
        private readonly ILogger<ReportPreviewVM> _logger;

        [ObservableProperty]
        public partial Report Report { get; set; } = new Report();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public ReportPreview Owner { get; private set; } = null!;

        public ReportPreviewVM(ILogger<ReportPreviewVM> logger)
        {
            _logger = logger;
        }

        public async Task InitialAsync(ReportPreview owner, string reportName)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                await Task.Run(() =>
                {
                    Report.Load(reportName);
                    Report.PrepareAsync(Owner.previewControl);
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
    }
}
