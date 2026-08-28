using Admin.Commons;
using Admin.Desktop.Resources.Langs;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Volo.Abp.Application.Dtos;

namespace Admin.Desktop.Tools
{
    public class NavDto : EntityDto<Guid>, INotifyPropertyChanged, IDisposable
    {
        private bool _disposed;
        public NavDto()
        {
            Id = Guid.NewGuid();
            LangProvider.Instance.PropertyChanged += OnGlobalLangChange;
        }

        public string? Icon { get; set; }

        public string Name { get; set; } = null!;

        public string LangKey { get; set; } = null!;

        public string DisplayName => GetDisplayName();

        public NavType Type { get; set; }

        public object? Content { get; set; }

        public string? PermissionName { get; set; }

        public IReadOnlyList<NavDto> Items { get; set; } = new List<NavDto>();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetDisplayName()
        {
            if (string.IsNullOrEmpty(LangKey))
            {
                return Name;
            }
            return LangProvider.GetLang(LangKey);
        }

        private void OnGlobalLangChange(object? sender, PropertyChangedEventArgs e)
        {
            // 语言切换，通知界面刷新DisplayName
            OnPropertyChanged(nameof(DisplayName));
        }

        // 正确释放全局事件
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // 移除全局语言监听，防止内存泄漏
                LangProvider.Instance.PropertyChanged -= OnGlobalLangChange;
            }
            _disposed = true;
        }
    }
}
