using Admin.Commons;
using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Admin.Desktop.UserControls
{
    /// <summary>
    /// LogControl.xaml 的交互逻辑
    /// </summary>
    public partial class LogControl : UserControl
    {
        public LogControl()
        {
            InitializeComponent();
        }

        public ObservableCollection<LogInfo> ItemsSource
        {
            get { return (ObservableCollection<LogInfo>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogInfos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<LogInfo>), typeof(LogControl), new PropertyMetadata(default(ObservableCollection<LogInfo>)));

        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var sb = new StringBuilder();
                foreach (var item in lst_log.SelectedItems)
                {
                    if (item is LogInfo log)
                    {
                        sb.AppendLine(log.LogMessages);
                    }
                }
                Clipboard.SetText(sb.ToString());
                e.Handled = true; // 防止其他控件处理此事件
            }
        }

        private void CopyLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = lst_log.SelectedItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("未选中任何日志", "提示");
                    return;
                }

                // 获取选中的日志实体
                if (selectedItem is LogInfo log)
                {
                    string logText = log.LogMessages;
                    if (!string.IsNullOrEmpty(logText))
                    {
                        Clipboard.SetText(logText); // 复制到剪贴板
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败：{ex.Message}");
            }
        }
    }

    public sealed class LogInfo : LogDto
    {
        public SolidColorBrush BackgroundBrush => Level switch
        {
            LogLevel.Information => ResourceHelper.GetResource<SolidColorBrush>(ResourceToken.DarkSuccessBrush),
            LogLevel.Warning => ResourceHelper.GetResource<SolidColorBrush>(ResourceToken.DarkWarningBrush),
            LogLevel.Error => ResourceHelper.GetResource<SolidColorBrush>(ResourceToken.DarkDangerBrush),
            LogLevel.Critical => ResourceHelper.GetResource<SolidColorBrush>(ResourceToken.DarkDangerBrush),
            _ => ResourceHelper.GetResource<SolidColorBrush>(ResourceToken.ReverseTextBrush),
        };

        public Style BorderTipStyle => Level switch
        {
            LogLevel.Information => ResourceHelper.GetResource<Style>("BorderTipSuccess"),
            LogLevel.Warning => ResourceHelper.GetResource<Style>("BorderTipWarning"),
            LogLevel.Error => ResourceHelper.GetResource<Style>("BorderTipDanger"),
            LogLevel.Critical => ResourceHelper.GetResource<Style>("BorderTipDanger"),
            _ => ResourceHelper.GetResource<Style>("BorderTipInfo"),
        };

        public Style TextBlockStyle => Level switch
        {
            LogLevel.Information => ResourceHelper.GetResource<Style>("TextBlockDefaultInfo"),
            LogLevel.Warning => ResourceHelper.GetResource<Style>("TextBlockDefaultWarning"),
            LogLevel.Error => ResourceHelper.GetResource<Style>("TextBlockDefaultAccent"),
            LogLevel.Critical => ResourceHelper.GetResource<Style>("TextBlockDefaultDanger"),
            _ => ResourceHelper.GetResource<Style>("TextBlockDefault")
        };

        public string LogMessages => $"[{CreateTime}]:{Messages}";
    }
}
