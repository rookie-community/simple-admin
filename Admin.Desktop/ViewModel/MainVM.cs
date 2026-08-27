using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Admin.Commons;
using Admin.Desktop.Resources.Langs;
using Admin.Desktop.Tools;
using Admin.Desktop.Tools.Messages;
using Admin.Desktop.View;
using Admin.Desktop.View.Accounts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FastReport.Utils;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Wpf;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using MessageBox = HandyControl.Controls.MessageBox;
using TabItem = HandyControl.Controls.TabItem;

namespace Admin.Desktop.ViewModel
{
    public partial class MainVM : ObservableRecipient, IRecipient<NavDto>, IRecipient<LogoutMessage>, ITransientDependency
    {
        private readonly string TitlePrefix = "Admin";
        private readonly IPermissionAppService _permissionAppService;
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly ILogger<MainVM> _logger;

        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int TabSelectedIndex { get; set; } = 0;

        [ObservableProperty]
        public partial string Version { get; set; } = null!;

        [ObservableProperty]
        public partial string UserName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<NavDto> NavItems { get; set; } = new ObservableCollection<NavDto>();

        [ObservableProperty]
        public partial ObservableCollection<TabItem> TabItems { get; set; } = new ObservableCollection<TabItem>();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial string NotifyIconToken { get; set; } = Guid.NewGuid().ToString();

        [ObservableProperty]
        public partial Visibility NotifyIconVisibility { get; set; } = Visibility.Visible;

        [ObservableProperty]
        public partial bool NotifyIconBlink { get; set; }

        public MainWindow Owner { get; private set; } = null!;

        public MainVM(IPermissionAppService permissionAppService, IIdentityUserAppService identityUserAppService, ILogger<MainVM> logger)
        {
            _permissionAppService = permissionAppService;
            _identityUserAppService = identityUserAppService;
            _logger = logger;
            IsActive = true;
        }

        public async Task InitialAsync(MainWindow owner)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                UserName = App.CurrentUser?.UserName ?? string.Empty;
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

                var navs = await BuiderNavItems();
                NavItems = new ObservableCollection<NavDto>(navs);
                var home = NavItems.FirstOrDefault(x => x.Type != NavType.Group);
                if (home != null)
                {
                    SetCurrentTabItem(home);
                }
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
        private void SwitchItem(NavDto navItem)
        {
            if (navItem.Type == NavType.Group)
            {
                if (Owner.NavigationView.ItemContainerGenerator.ContainerFromItem(navItem) is TreeViewItem treeViewItem)
                {
                    // 切换展开状态
                    treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                }
                return;
            }

            if (navItem?.Content == null)
            {
                //菜单为空不做处理
                return;
            }
            SetCurrentTabItem(navItem);
        }

        [RelayCommand]
        private void SwitchTheme(SkinType skinType)
        {
            try
            {
                var app = (App)Application.Current;

                var temps = app.Resources.MergedDictionaries.ToList();
                app.Resources.MergedDictionaries.Clear();

                app.Resources.MergedDictionaries.Add(ResourceHelper.GetSkin(skinType));
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
                });

                foreach (var item in temps)
                {
                    if (!item.Source.LocalPath.Contains("HandyControl"))
                    {
                        app.Resources.MergedDictionaries.Add(item);
                    }
                }

                // 夜间深色
                if (skinType == SkinType.Dark)
                {
                    LiveCharts.DefaultSettings.AddDarkTheme();
                    Config.UIStyle = UIStyle.Dark;
                }
                else
                {
                    // 白天浅色
                    LiveCharts.DefaultSettings.AddLightTheme();
                    // FastReport 
                    Config.UIStyle = UIStyle.LightBlue;
                }

                Application.Current.MainWindow?.OnApplyTemplate();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
        }

        [RelayCommand]
        private void SwitchLang(string langName)
        {
            try
            {
                if (langName == LangProvider.Culture.Name)
                {
                    return;
                }

                ConfigHelper.Instance.SetLang(langName);
                LangProvider.Culture = new CultureInfo(langName);
                //更新配置文件
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["Language"].Value = langName;
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Warning(ex.InnerException?.Message ?? ex.Message, "切换语言失败");
            }
        }

        [RelayCommand]
        private void MyProfile()
        {
            var nav = new NavDto
            {
                Id = Guid.Parse("F12CD9F3-2F14-4D13-965B-EFA60F207C3B"),
                Icon = "\xf013;",
                Name = "我的账户",
                Type = NavType.UserControl,
                Content = typeof(MyProfileView).FullName,
            };
            SwitchItem(nav);
        }

        [RelayCommand]
        private void Logout()
        {
            var view = App.Current.Services.GetService<LoginView>() ?? throw new ArgumentNullException(nameof(LoginView));
            view.Show();
            NotifyIconVisibility = Visibility.Collapsed;
            Owner.Close();
            Owner = null!;
        }

        private async Task<List<NavDto>> BuiderNavItems()
        {
            var allNavItems = NavProvider.GetNavConfigs();
            var permissions = new List<PermissionGrantInfoDto>();
            try
            {
                //根据用户权限过滤菜单
                var userPermissionResult = await _permissionAppService.GetAsync("U", UserName);
                permissions = userPermissionResult.Groups.SelectMany(x => x.Permissions).ToList();
                var rolesResult = await _identityUserAppService.GetRolesAsync(App.CurrentUser.Id);
                foreach (var role in rolesResult.Items)
                {
                    var permissionResult = await _permissionAppService.GetAsync("R", role.Name);
                    var data = permissionResult.Groups.SelectMany(x => x.Permissions).ToList();
                    permissions.AddRange(data);
                }
                App.SetCurrentUserPermission(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                if (ex is AbpRemoteCallException abpRemoteCallException && !string.IsNullOrEmpty(abpRemoteCallException.Details))
                {
                    Growl.Warning(abpRemoteCallException.Details);
                }
                else
                {
                    Growl.Warning(ex.Message);
                }
            }
            //var permissionResult2 = await _permissionAppService.GetByGroupAsync(IdentityPermissions.GroupName, providerName, providerKey);
            var permissionNames = permissions.Where(x => x.IsGranted).DistinctBy(x => x.Name).Select(x => x.Name).ToList();
            var navItems = FilterPermissionTree(allNavItems, node => string.IsNullOrEmpty(node.PermissionName) || permissionNames.Contains(node.PermissionName));
            return navItems;
        }

        public static List<NavDto> FilterPermissionTree(IReadOnlyList<NavDto> nodes, Func<NavDto, bool> condition)
        {
            if (nodes == null)
            {
                return new List<NavDto>();
            }

            var result = new List<NavDto>();

            foreach (var node in nodes)
            {
                // 递归过滤子节点
                node.Items = FilterPermissionTree(node.Items, condition);

                // 满足条件 或 有子节点 → 保留
                if (condition(node) || node.Items.Any())
                {
                    result.Add(node);
                }
            }
            return result;
        }

        private void SetCurrentTabItem(NavDto navItem, object[]? parameters = null)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (navItem == null)
                    {
                        return;
                    }

                    if (navItem.Content == null)
                    {
                        return;
                    }

                    //如果已经打开了，就切换到对应的Tab
                    var tabItemIdx = TabItems.FindIndex(x => x.Tag.ToString() == navItem.Id.ToString());
                    if (tabItemIdx >= 0)
                    {
                        TabSelectedIndex = tabItemIdx;
                        Title = $"{TitlePrefix} - {navItem.DisplayName}";
                        return;
                    }

                    var isHome = navItem.Content.Equals(typeof(ConsoleView).FullName);
                    var tabHeaderText = new TextBlock();
                    if (!string.IsNullOrEmpty(navItem.Icon))
                    {
                        var iconRun = new Run
                        {
                            Text = navItem.Icon,
                            Foreground = ResourceHelper.GetResource<SolidColorBrush>(ResourceToken.DarkInfoBrush),
                            FontFamily = (FontFamily)Application.Current.FindResource("FA_Regular"),
                        };
                        tabHeaderText.Inlines.Add(iconRun);
                        tabHeaderText.Inlines.Add(new Run
                        {
                            Text = "\u00A0",
                        });
                    }
                    var tabHeaderRun = new Run();
                    if (!string.IsNullOrEmpty(navItem.LangKey))
                    {
                        // 直接绑定Dto的DisplayName，替代LangProvider.SetLang
                        Binding displayNameBinding = new Binding(nameof(navItem.DisplayName))
                        {
                            Source = navItem,
                            Mode = BindingMode.OneWay
                        };
                        tabHeaderRun.SetBinding(Run.TextProperty, displayNameBinding);
                    }
                    else
                    {
                        tabHeaderRun.Text = navItem.Name;
                    }
                    tabHeaderText.Inlines.Add(tabHeaderRun);

                    var tabItem = new TabItem
                    {
                        Header = tabHeaderText,
                        Tag = navItem.Id,
                        ShowCloseButton = !isHome,
                        ShowContextMenu = !isHome,
                    };

                    tabItem.Closing += (sender, args) =>
                    {
                        if (sender is TabItem && args is CancelRoutedEventArgs cancelArgs)
                        {
                            if (isHome)
                            {
                                cancelArgs.Cancel = true;
                                return;
                            }
                        }
                    };

                    if (navItem.Type == NavType.Content)
                    {
                        tabItem.Content = navItem.Content;
                        Title = $"{TitlePrefix} - {navItem.DisplayName}";
                        TabItems.Add(tabItem);
                        TabSelectedIndex = TabItems.IndexOf(tabItem);
                        return;
                    }

                    if (navItem.Type == NavType.UserControl || navItem.Type == NavType.Page)
                    {
                        var contentType = Type.GetType(navItem.Content.ToString() ?? string.Empty);
                        var constructor = contentType?.GetConstructor(Type.EmptyTypes);
                        if (typeof(UserControl).IsAssignableFrom(contentType))
                        {
                            tabItem.Content = (UserControl)constructor?.Invoke(parameters)!;
                        }
                        else if (typeof(Page).IsAssignableFrom(contentType))
                        {
                            tabItem.Content = new Frame
                            {
                                Content = (Page)constructor?.Invoke(parameters)!,
                                NavigationUIVisibility = NavigationUIVisibility.Hidden,
                            };
                        }
                        else
                        {
                            Growl.Error($"无法打开页面，类型错误：{navItem.Content}");
                            return;
                        }
                        Title = $"{TitlePrefix} - {navItem.DisplayName}";
                        TabItems.Add(tabItem);
                        TabSelectedIndex = TabItems.IndexOf(tabItem);
                        return;
                    }

                    if (navItem.Type == NavType.Web)
                    {
                        var webView = new WebView2
                        {
                            Source = new Uri(navItem.Content.ToString() ?? string.Empty),
                        };

                        var dialogContainer = new DialogContainer
                        {
                            Child = webView
                        };
                        var webViewToken = Guid.NewGuid().ToString();
                        Dialog.SetToken(dialogContainer, webViewToken);
                        webView.NavigationStarting += (s, e) =>
                        {
                            Dialog.Show<LoadingCircle>(webViewToken);
                        };

                        webView.NavigationCompleted += (s, e) =>
                        {
                            Dialog.Close(webViewToken);
                            if (!e.IsSuccess)
                            {
                                Growl.Error($"[{navItem.DisplayName}] {e.WebErrorStatus}");
                            }
                        };

                        tabItem.Content = dialogContainer;
                        Title = $"{TitlePrefix} - {navItem.DisplayName}";
                        TabItems.Add(tabItem);
                        TabSelectedIndex = TabItems.IndexOf(tabItem);
                        return;
                    }

                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Growl.Fatal($"打开页面失败：{ex.Message}");
            }
        }

        #region Message 通知

        public void Receive(NavDto message)
        {
            SwitchItemCommand.Execute(message);
        }

        public void Receive(LogoutMessage message)
        {
            LogoutCommand.Execute(null);
        }

        #endregion
    }
}
