using System.Windows;

namespace Admin.Desktop.Tools
{
    /// <summary>
    /// 页面按钮权限绑定的统一构建入口。
    /// 负责将 “按钮键名 -> 权限编码” 的映射转换为 XAML 可直接绑定的 “按钮键名 -> Visibility” 字典，
    /// 各页面 ViewModel 无需再各自实现 LoadButtonPermissions/循环赋值等重复逻辑。
    /// </summary>
    internal static class ButtonPermissionBuilder
    {
        /// <summary>
        /// 构建按钮显隐绑定字典。
        /// 规则：
        ///  1) 权限编码为空：视为“暂未绑定权限”的占位按钮，默认可见（如：导出/详情等）；
        ///  2) 权限已授予：可见；
        ///  3) 权限未授予或编码未知：折叠（权限不足时不暴露操作按钮）。
        /// </summary>
        /// <param name="buttonPermissionMap">按钮键名（对应 XAML 中 Visibility="{Binding BtnPerms[键名]}"）与权限编码的映射。</param>
        public static Dictionary<string, Visibility> Build(IDictionary<string, string> buttonPermissionMap)
        {
            var result = new Dictionary<string, Visibility>(buttonPermissionMap.Count);
            foreach (var item in buttonPermissionMap)
            {
                result[item.Key] = App.PermissionChecker(item.Value);
            }
            return result;
        }

        /// <summary>
        /// 生成“全部按钮折叠”的默认绑定字典，供页面初始化前占位使用，避免按钮在权限解析前闪现。
        /// </summary>
        public static Dictionary<string, Visibility> BuildCollapsed(IDictionary<string, string> buttonPermissionMap)
            => buttonPermissionMap.ToDictionary(x => x.Key, _ => Visibility.Collapsed);
    }
}
