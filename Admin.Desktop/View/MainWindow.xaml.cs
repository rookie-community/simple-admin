using Admin.Desktop.Tools;
using Admin.Desktop.ViewModel;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows.Controls;
using TabItem = HandyControl.Controls.TabItem;
using Window = HandyControl.Controls.Window;

namespace Admin.Desktop.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainVM vm;
        private bool _isLoaded;

        public MainWindow()
        {
            InitializeComponent();
            vm = App.Current.Services.GetService<MainVM>()!;
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

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            NotifyIcon.ShowBalloonTip("Admin", "已最小化到任务栏", NotifyIconInfoType.Info, vm.NotifyIconToken);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is TabItem tabItem)
            {
                if (tabItem.Tag != null && Guid.TryParse(tabItem.Tag.ToString(), out Guid targetId))
                {
                    // 查找并选中TreeView中对应的项
                    foreach (var item in NavigationView.Items)
                    {
                        var treeViewItem = NavigationView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                        if (treeViewItem != null && FindAndSelectTreeViewItem(treeViewItem, targetId))
                        {
                            break;
                        }
                    }
                }
            }
        }

        private bool FindAndSelectTreeViewItem(TreeViewItem? item, Guid targetId)
        {
            if (item == null) return false;

            // 检查当前项是否匹配
            if (item.Header is NavDto nav && nav.Id == targetId)
            {
                item.IsSelected = true;
                item.BringIntoView();
                return true;
            }

            // 递归检查子项
            foreach (object subItem in item.Items)
            {
                var subTreeViewItem = item.ItemContainerGenerator.ContainerFromItem(subItem) as TreeViewItem;
                if (subTreeViewItem != null && FindAndSelectTreeViewItem(subTreeViewItem, targetId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
