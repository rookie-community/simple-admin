using HandyControl.Tools;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Admin.Desktop.Resources.Langs
{
    public class LangProvider : INotifyPropertyChanged
    {
        internal static LangProvider Instance { get; } = ResourceHelper.GetResource<LangProvider>("AdminLangs");

        private static string CultureInfoStr = string.Empty;

        public static CultureInfo Culture
        {
            get => Lang.Culture;
            set
            {
                if (value == null) return;
                if (Equals(CultureInfoStr, value.EnglishName)) return;
                Lang.Culture = value;
                CultureInfoStr = value.EnglishName;

                Instance.UpdateLangs();
            }
        }

        public static string GetLang(string key)
        {
            return Lang.ResourceManager.GetString(key, Culture) ?? key;
        }

        public static void SetLang(DependencyObject dependencyObject, DependencyProperty dependencyProperty, string key)
        {
            BindingOperations.SetBinding(dependencyObject, dependencyProperty, new Binding(key)
            {
                Source = Instance,
                Mode = BindingMode.OneWay,
            });
        }

        private void UpdateLangs()
        {
            CultureInfo.CurrentCulture = Lang.Culture;
            CultureInfo.CurrentUICulture = Lang.Culture;

            OnPropertyChanged(nameof(About));
            OnPropertyChanged(nameof(Account));
            OnPropertyChanged(nameof(DataAcquisition));
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(Tenant));
            OnPropertyChanged(nameof(Version));
        }


        /// <summary>
        /// 关于
        /// </summary>
		public string About => Lang.About;

        /// <summary>
        /// 账号
        /// </summary>
		public string Account => Lang.Account;

        /// <summary>
        /// 数据采集
        /// </summary>
		public string DataAcquisition => Lang.DataAcquisition;

        /// <summary>
        /// 语言
        /// </summary>
		public string Language => Lang.Language;

        /// <summary>
        /// 登录
        /// </summary>
		public string Login => Lang.Login;

        /// <summary>
        /// 密码
        /// </summary>
		public string Password => Lang.Password;

        /// <summary>
        /// 租户
        /// </summary>
		public string Tenant => Lang.Tenant;

        /// <summary>
        /// 版本
        /// </summary>
		public string Version => Lang.Version;


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class LangKeys
    {

        /// <summary>
        /// 关于
        /// </summary>
	    public static string About = nameof(About);

        /// <summary>
        /// 账号
        /// </summary>
	    public static string Account = nameof(Account);

        /// <summary>
        /// 数据采集
        /// </summary>
	    public static string DataAcquisition = nameof(DataAcquisition);

        /// <summary>
        /// 语言
        /// </summary>
	    public static string Language = nameof(Language);

        /// <summary>
        /// 登录
        /// </summary>
	    public static string Login = nameof(Login);

        /// <summary>
        /// 密码
        /// </summary>
	    public static string Password = nameof(Password);

        /// <summary>
        /// 租户
        /// </summary>
	    public static string Tenant = nameof(Tenant);

        /// <summary>
        /// 版本
        /// </summary>
	    public static string Version = nameof(Version);

    }
}
