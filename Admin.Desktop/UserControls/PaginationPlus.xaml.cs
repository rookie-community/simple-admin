using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Admin.Desktop.UserControls
{
    public partial class PaginationPlus : UserControl
    {
        /// <summary>
        /// 同步锁：防止双向绑定死循环重复触发事件
        /// </summary>
        private bool _isSyncingFromInnerControl;

        #region 对外暴露属性
        /// <summary>
        /// 最大总页数 MaxPageCount
        /// </summary>
        public int MaxPageCount
        {
            get => (int)GetValue(MaxPageCountProperty);
            set => SetValue(MaxPageCountProperty, value);
        }
        public static readonly DependencyProperty MaxPageCountProperty =
            DependencyProperty.Register(nameof(MaxPageCount), typeof(int), typeof(PaginationPlus),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((PaginationPlus)d).PaginationMain.MaxPageCount = (int)e.NewValue));

        /// <summary>
        /// 每页数据量 DataCountPerPage
        /// </summary>
        public int DataCountPerPage
        {
            get => (int)GetValue(DataCountPerPageProperty);
            set => SetValue(DataCountPerPageProperty, value);
        }
        public static readonly DependencyProperty DataCountPerPageProperty =
            DependencyProperty.Register(nameof(DataCountPerPage), typeof(int), typeof(PaginationPlus),
                new FrameworkPropertyMetadata(30, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                        var ctrl = (PaginationPlus)d;
                        var newValue = (int)e.NewValue;
                        // 如果是内部控件同步过来的赋值，只同步UI，不重复计算、不触发事件
                        if (ctrl._isSyncingFromInnerControl)
                        {
                            ctrl.PaginationMain.DataCountPerPage = newValue;
                            return;
                        }
                        // 外部VM/下拉手动修改每页条数：同步控件 + 重算最大页 + 触发分页事件
                        ctrl.PaginationMain.DataCountPerPage = newValue;
                        ctrl.CalcMaxPageCount();
                        ctrl.FirePageUpdatedEvent();
                    }));

        /// <summary>
        /// 当前页码 PageIndex
        /// </summary>
        public int PageIndex
        {
            get => (int)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }
        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(PaginationPlus),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                        var ctrl = (PaginationPlus)d;
                        var newValue = (int)e.NewValue;
                        if (ctrl._isSyncingFromInnerControl)
                        {
                            ctrl.PaginationMain.PageIndex = newValue;
                            return;
                        }
                        ctrl.PaginationMain.PageIndex = newValue;
                    }));

        /// <summary>
        /// 页码省略间隔 MaxPageInterval
        /// </summary>
        public int MaxPageInterval
        {
            get => (int)GetValue(MaxPageIntervalProperty);
            set => SetValue(MaxPageIntervalProperty, value);
        }
        public static readonly DependencyProperty MaxPageIntervalProperty =
            DependencyProperty.Register(nameof(MaxPageInterval), typeof(int), typeof(PaginationPlus),
                new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((PaginationPlus)d).PaginationMain.MaxPageInterval = (int)e.NewValue));

        /// <summary>
        /// 是否显示跳转输入框 IsJumpEnabled
        /// </summary>
        public bool IsJumpEnabled
        {
            get => (bool)GetValue(IsJumpEnabledProperty);
            set => SetValue(IsJumpEnabledProperty, value);
        }
        public static readonly DependencyProperty IsJumpEnabledProperty =
            DependencyProperty.Register(nameof(IsJumpEnabled), typeof(bool), typeof(PaginationPlus),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((PaginationPlus)d).PaginationMain.IsJumpEnabled = (bool)e.NewValue));

        /// <summary>
        /// 数据总条数（扩展属性，变更自动刷新MaxPageCount）
        /// </summary>
        public int TotalCount
        {
            get => (int)GetValue(TotalCountProperty);
            set => SetValue(TotalCountProperty, value);
        }
        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(PaginationPlus),
                new PropertyMetadata(0, (d, e) =>
                {
                    var ctrl = (PaginationPlus)d;
                    ctrl.CalcMaxPageCount();
                }));

        /// <summary>
        /// 每页条数下拉选项集合
        /// </summary>
        public ObservableCollection<int> LimitOptions
        {
            get => (ObservableCollection<int>)GetValue(LimitOptionsProperty);
            set => SetValue(LimitOptionsProperty, value);
        }
        public static readonly DependencyProperty LimitOptionsProperty =
            DependencyProperty.Register(nameof(LimitOptions), typeof(ObservableCollection<int>), typeof(PaginationPlus),
                new PropertyMetadata(new ObservableCollection<int>() { 10, 20, 30, 40, 50, 60, 70, 80, 90 }));

        #endregion

        #region MVVM 无参Command绑定（无入参）
        public ICommand PageUpdatedCommand
        {
            get => (ICommand)GetValue(PageUpdatedCommandProperty);
            set => SetValue(PageUpdatedCommandProperty, value);
        }
        public static readonly DependencyProperty PageUpdatedCommandProperty =
            DependencyProperty.Register(nameof(PageUpdatedCommand), typeof(ICommand), typeof(PaginationPlus),
                new PropertyMetadata(null));
        #endregion

        #region 对外PageUpdated事件（对齐HandyControl原生格式：标准EventHandler）
        /// <summary>页码变更、每页条数变更触发，格式同hc:Pagination.PageUpdated</summary>
        public event EventHandler? PageUpdated;
        #endregion

        public PaginationPlus()
        {
            InitializeComponent();
            PaginationMain.PageUpdated += OnInnerPageUpdated;
            CalcMaxPageCount();

            // 监听内部分页控件Visibility依赖属性变更
            var desc = DependencyPropertyDescriptor.FromProperty(VisibilityProperty, typeof(UIElement));
            desc.AddValueChanged(PaginationMain, (sender, args) =>
            {
                if (PaginationMain.Visibility != Visibility.Visible)
                {
                    // SetCurrentValue 不会覆盖XAML本地值、不会重复触发监听
                    PaginationMain.SetCurrentValue(VisibilityProperty, Visibility.Visible);
                }
            });
            // 初始化强制置为可见
            PaginationMain.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }

        #region 核心：自动计算最大页码逻辑
        /// <summary>根据TotalCount和DataCountPerPage自动计算并赋值MaxPageCount</summary>
        private void CalcMaxPageCount()
        {
            if (DataCountPerPage <= 0)
                return;

            int maxPage = TotalCount == 0
                ? 1
                : (int)Math.Ceiling((double)TotalCount / DataCountPerPage);

            MaxPageCount = maxPage;

            // 边界保护：当前页码超出最大页则切回第一页并触发事件
            if (PageIndex > MaxPageCount)
            {
                PageIndex = 1;
                FirePageUpdatedEvent();
            }
        }
        #endregion

        #region 统一触发分页事件/无参Command
        private void FirePageUpdatedEvent()
        {
            // 1. 触发标准EventHandler事件（和HandyControl格式完全一致）
            PageUpdated?.Invoke(this, EventArgs.Empty);

            // 2. 执行无参Command，不需要传递PaginationArgs
            if (PageUpdatedCommand?.CanExecute(null) == true)
            {
                PageUpdatedCommand.Execute(null);
            }
        }
        #endregion

        /// <summary>内部hc分页控件页码/条数变更回调（点击页码、跳转框回车）</summary>
        private void OnInnerPageUpdated(object? sender, EventArgs e)
        {
            // 同步锁：防止双向绑定循环重复触发
            _isSyncingFromInnerControl = true;
            try
            {
                PageIndex = PaginationMain.PageIndex;
                DataCountPerPage = PaginationMain.DataCountPerPage;
            }
            finally
            {
                _isSyncingFromInnerControl = false;
            }
            FirePageUpdatedEvent();
        }
    }
}