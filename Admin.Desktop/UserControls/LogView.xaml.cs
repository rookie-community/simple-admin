using HandyControl.Data;
using HandyControl.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Admin.Desktop.UserControls
{
    /// <summary>
    /// LogView.xaml 的交互逻辑
    /// </summary>
    public partial class LogView : UserControl
    {
        private readonly Lock _appendLog = new Lock();
        public LogView()
        {
            InitializeComponent();
        }

        public int MaxLogCount
        {
            get => (int)GetValue(MaxLogCountProperty);
            set => SetValue(MaxLogCountProperty, value);
        }

        public static readonly DependencyProperty MaxLogCountProperty =
            DependencyProperty.Register(
                nameof(MaxLogCount),
                typeof(int),
                typeof(LogView),
                new PropertyMetadata(100)); // 默认100行

        /// <summary>
        /// Gets or sets a value indicating whether to add new event at the end.
        /// </summary>
        public bool AddAtTail
        {
            get { return (bool)this.GetValue(AddAtTailProperty); }
            set { this.SetValue(AddAtTailProperty, value); }
        }

        /// <summary>
        /// Dependency property for <c>AddAtTail</c>.
        /// </summary>
        public static readonly DependencyProperty AddAtTailProperty =
          DependencyProperty.Register(
              nameof(AddAtTail),
              typeof(bool),
              typeof(LogView),
              new PropertyMetadata(true));

        /// <summary>
        /// 通用日志输出
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="dateTime"></param>
        /// <param name="color"></param>
        private void AppendLog(string msg, DateTime? dateTime = null, SolidColorBrush? color = null)
        {
            Dispatcher.Invoke(() =>
            {
                lock (_appendLog)
                {
                    // 记录当前滚动位置
                    var currentOffset = RtbLog.VerticalOffset;

                    var blocks = RtbLog.Document.Blocks;
                    // 超出最大行删除最旧一条
                    while (blocks.Count > MaxLogCount)
                    {
                        var waitRemoveBlock = AddAtTail ? blocks.FirstBlock : blocks.LastBlock;
                        blocks.Remove(waitRemoveBlock);
                    }

                    var para = new Paragraph
                    {
                        Margin = new Thickness(0),
                        Padding = new Thickness(0, 0.5, 0, 0.5)
                    };

                    // 时间
                    para.Inlines.Add(new Run($"[{dateTime ?? DateTime.Now}] ")
                    {
                        Foreground = GetHcBrush(ResourceToken.SecondaryTextBrush)
                        //Foreground = Foreground = Brushes.Gray
                    });

                    var contentRun = new Run(msg);
                    if (color != null)
                    {
                        contentRun.Foreground = color;
                    }
                    // 日志内容 
                    para.Inlines.Add(contentRun);
                    if (AddAtTail)
                    {
                        blocks.Add(para);
                    }
                    else
                    {
                        blocks.InsertBefore(blocks.FirstBlock, para);
                    }

                    //滚动条位置保持不变
                    RtbLog.ScrollToVerticalOffset(currentOffset);
                }
            });
        }

        public void Info(string msg, DateTime? dateTime = null) => AppendLog(msg, dateTime, null);

        public void Success(string msg, DateTime? dateTime = null) => AppendLog(msg, dateTime, GetHcBrush(ResourceToken.DarkSuccessBrush));

        public void Warn(string msg, DateTime? dateTime = null) => AppendLog(msg, dateTime, GetHcBrush(ResourceToken.DarkWarningBrush));

        public void Error(string msg, DateTime? dateTime = null) => AppendLog(msg, dateTime, GetHcBrush(ResourceToken.DarkDangerBrush));

        /// <summary>
        /// 清空全部日志
        /// </summary>
        public void Clear()
        {
            RtbLog.Document.Blocks.Clear();
        }

        private static SolidColorBrush? GetHcBrush(string resourceToken)
        {
            var brush = ResourceHelper.GetResource<SolidColorBrush>(resourceToken);
            // 资源找不到时给默认黑色，防止null报错
            return brush ?? Brushes.Black;
        }
    }
}
