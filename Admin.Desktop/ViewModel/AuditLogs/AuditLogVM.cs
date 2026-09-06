using Admin.AuditLogs;
using Admin.Desktop.Tools;
using Admin.Desktop.View.AuditLogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Windows;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Admin.Desktop.ViewModel.AuditLogs
{
    public partial class AuditLogVM : ObservableValidator, ITransientDependency
    {
        private readonly IAuditLogAppService _auditLogAppService;
        private readonly ILogger<AuditLogVM> _logger;

        [ObservableProperty]
        public partial DateTime? StartTime { get; set; }

        [ObservableProperty]
        public partial DateTime? EndTime { get; set; }

        [ObservableProperty]
        public partial string UserName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Url { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int MinDuration { get; set; }

        [ObservableProperty]
        [CustomValidation(typeof(AuditLogVM), nameof(ValidateDuration))]
        public partial int MaxDuration { get; set; }

        [ObservableProperty]
        public partial string HttpMethod { get; set; } = string.Empty;

        [ObservableProperty]
        public partial HttpStatusCode? HttpStatusCode { get; set; }

        [ObservableProperty]
        public partial string ApplicationName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ClientIpAddress { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Dictionary<string, HttpStatusCode?> HttpStatusCodes { get; set; } = new Dictionary<string, HttpStatusCode?>();

        [ObservableProperty]
        public partial ObservableCollection<AuditLogDto> AuditLogs { get; set; } = new ObservableCollection<AuditLogDto>();

        [ObservableProperty]
        public partial int PageIndex { get; set; } = 1;

        [ObservableProperty]
        public partial long TotalCount { get; set; }

        [ObservableProperty]
        public partial int DataCountPerPage { get; set; } = 30;

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial Dictionary<string, Visibility> BtnPerms { get; set; } = new Dictionary<string, Visibility>();

        public AuditLogView Owner { get; private set; } = null!;

        public AuditLogVM(IAuditLogAppService auditLogAppService, ILogger<AuditLogVM> logger)
        {
            _auditLogAppService = auditLogAppService;
            _logger = logger;
            BtnPerms = ButtonPermissionBuilder.Build(BtnPermissionMap);
        }

        internal async Task InitialAsync(AuditLogView owner)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                var httpStatusCodeList = new Dictionary<string, HttpStatusCode?>
                {
                    { "-", null }
                };
                var httpStatusCodeTemps = Enum.GetValues<HttpStatusCode>();
                foreach (var item in httpStatusCodeTemps)
                {
                    var temp = $"{(int)item} - {item}";
                    httpStatusCodeList.TryAdd(temp, item);
                }
                HttpStatusCodes = httpStatusCodeList;
                await LoadDataAsync();
            }
            catch (AbpRemoteCallException abpRemoteCallException)
            {
                _logger.LogException(abpRemoteCallException);
                MessageBox.Error(abpRemoteCallException.Details, abpRemoteCallException.Message);
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

        /// <summary>
        /// 页面按钮键名 -> 权限编码 的映射。
        /// 键名需与 AuditLogView 中 Visibility="{Binding BtnPerms[键名]}" 的绑定一致。
        /// 权限编码为空表示暂未绑定权限，按钮默认可见。
        /// </summary>
        private static readonly Dictionary<string, string> BtnPermissionMap = new()
        {
            { "Export", string.Empty },
            { "Detail", string.Empty },
        };

        [RelayCommand]
        private async Task SearchAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                await LoadDataAsync();
            }
            catch (AbpRemoteCallException abpEx)
            {
                _logger.LogException(abpEx);
                MessageBox.Error(abpEx.Details, abpEx.Message);
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

        [RelayCommand]
        private void Reset()
        {
            StartTime = default;
            EndTime = default;
            UserName = string.Empty;
            Url = string.Empty;
            MinDuration = 0;
            MaxDuration = 0;
            HttpMethod = string.Empty;
            HttpStatusCode = default;
            ApplicationName = string.Empty;
            ClientIpAddress = string.Empty;
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            MessageBox.Warning("功能暂未开放");
        }

        [RelayCommand]
        private void Detail(AuditLogDto auditLog)
        {
            MessageBox.Warning("功能暂未开放");
        }

        [RelayCommand]
        private async Task PageChangedAsync()
        {
            await SearchCommand.ExecuteAsync(null);
        }

        private async Task LoadDataAsync()
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                return;
            }

            var result = await _auditLogAppService.GetListAsync(new GetAuditLogListInput
            {
                StartTime = StartTime,
                EndTime = EndTime,
                UserName = UserName,
                Url = Url,
                MinDuration = MinDuration,
                MaxDuration = MaxDuration,
                HttpMethod = HttpMethod,
                HttpStatusCode = HttpStatusCode,
                ApplicationName = ApplicationName,
                ClientIpAddress = ClientIpAddress,
                SkipCount = (PageIndex - 1) * DataCountPerPage,
                MaxResultCount = DataCountPerPage,
            });
            TotalCount = result.TotalCount;
            AuditLogs = new ObservableCollection<AuditLogDto>(result.Items);
        }

        public static ValidationResult ValidateDuration(int maxDurationValue, ValidationContext context)
        {
            var instance = (AuditLogVM)context.ObjectInstance;
            var minDurationValue = instance.MinDuration;
            if (maxDurationValue > 0 && minDurationValue > 0 && maxDurationValue < minDurationValue)
            {
                return new ValidationResult("最大耗时必须大于或等于最小耗时");
            }
            return ValidationResult.Success!;
        }
    }
}
