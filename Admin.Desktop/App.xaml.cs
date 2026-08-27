using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Admin.Desktop.Resources.Langs;
using Admin.Desktop.View.Accounts;
using HandyControl.Tools;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Admin.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IdentityUserDto CurrentUser { get; private set; } = null!;

        public static Dictionary<string, bool> CurrentUserPermissions { get; private set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services => _abpApplication?.ServiceProvider ?? throw new ArgumentNullException(nameof(IAbpApplicationWithInternalServiceProvider));

        private IAbpApplicationWithInternalServiceProvider? _abpApplication;

        /// <summary>
        /// Mutex全局唯一的名称，建议使用GUID或程序标识符
        /// </summary>
        private const string MutexName = "92D1DFAA-650A-40A4-A14F-ED85D6D2A5D5";
        private static Mutex _mutex = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .CreateLogger();
            try
            {
                //配置语言环境
                var langName = ConfigurationManager.AppSettings["Language"]?.ToString() ?? CultureInfo.CurrentCulture.Name;
                ConfigHelper.Instance.SetLang(langName);
                ConfigHelper.Instance.SetWindowDefaultStyle();
                ConfigHelper.Instance.SetNavigationWindowDefaultStyle();
                LangProvider.Culture = new CultureInfo(langName);

                //调试环境下编译多语言环境
                if (Debugger.IsAttached)
                {
                    //LangProviderGenerator.Generator();
                }

                //配置主题
                LiveCharts.Configure(cfg => cfg.AddSkiaSharp()
                                               .AddDefaultMappers()
                                               .AddDefaultTheme()
                                            );

                Log.Information("Starting WPF host.");
                _abpApplication = await AbpApplicationFactory.CreateAsync<AdminDesktopModule>(options =>
                {
                    options.UseAutofac();
                    options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
                });

                await _abpApplication.InitializeAsync();
                var view = _abpApplication.Services.GetRequiredService<LoginView>() ?? throw new ArgumentNullException(nameof(LoginView));
                view.Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_abpApplication != null)
            {
                await _abpApplication.ShutdownAsync();
            }
            Log.CloseAndFlush();
        }

        internal static void SetCurrentUser(IdentityUserDto user) => CurrentUser = user;

        internal static void SetCurrentUserPermission(IEnumerable<PermissionGrantInfoDto> permissionGrantInfos)
        {
            CurrentUserPermissions = permissionGrantInfos.ToLookup(x => x.Name).ToDictionary(k => k.Key, v => v.OrderByDescending(x => x.IsGranted).Select(x => x.IsGranted).First());
        }

        internal static Visibility PermissionChecker(string permissionName)
        {
            if (CurrentUserPermissions.TryGetValue(permissionName, out var isGranted))
            {
                if (!isGranted)
                {
                    //无权限
                    return Visibility.Collapsed;
                }
            }
            //有权限或未配置则显示
            return Visibility.Visible;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //UI线程未捕获异常处理事件（UI主线程）
            DispatcherUnhandledException += App_DispatcherUnhandledException; ;
            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // 创建Mutex实例
            _mutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                // Mutex已存在，说明程序已经在运行
                // 激活已运行程序的窗口并退出
                ActivateExistingWindow();
                _mutex.Close(); // 关闭新创建的（但未获取到）的Mutex
                Shutdown();
                return; // 退出新实例
            }
            Application.Current.Exit += (s, args) =>
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
            };
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Log.Error(ex, $"Task线程异常{ex.Message}");
            MessageBox.Fatal(ex.Message, "Task线程异常");
            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Error(ex, $"非UI线程异常{ex}");
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            Log.Error(ex, $"程序异常{ex.Message}");
            MessageBox.Fatal(ex.Message, "程序异常");
            e.Handled = true;//表示异常已处理，可以继续运行
        }

        /// <summary>
        /// 尝试激活已存在程序的主窗口
        /// </summary>
        private static void ActivateExistingWindow()
        {
            // 使用Process获取当前进程的进程名
            var currentProcess = Process.GetCurrentProcess();
            var processName = currentProcess.ProcessName;
            // 遍历所有同名进程，排除自身（新创建的进程）
            var processs = Process.GetProcessesByName(processName).Where(process => process.Id != currentProcess.Id);
            foreach (var process in processs)
            {
                // 尝试获取进程的主窗口句柄
                IntPtr handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    // 如果窗口最小化，则恢复
                    if (IsIconic(handle))
                    {
                        ShowWindow(handle, SW_RESTORE);
                    }
                    // 将窗口置前并激活
                    SetForegroundWindow(handle);
                    break; // 找到第一个就退出循环
                }
            }
        }

        // 引入Windows API函数用于窗口操作
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
    }
}
