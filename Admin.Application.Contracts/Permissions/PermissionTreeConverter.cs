using System.Collections.Generic;
using System.Linq;
using Volo.Abp.PermissionManagement;

namespace Admin.Permissions
{
    public static class PermissionTreeConverter
    {
        public static List<PermissionGroupTreeDto> ConvertToTree(List<PermissionGroupDto> sourceGroups)
        {
            var groupTreeList = new List<PermissionGroupTreeDto>();

            foreach (var group in sourceGroups)
            {
                // 1. 当前分组所有权限转树形扁平对象
                var allPermissionNodes = group.Permissions
                    .Select(p => new PermissionTreeDto
                    {
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        ParentName = string.IsNullOrWhiteSpace(p.ParentName) ? string.Empty : p.ParentName.Trim(),
                        GroupName = group.Name,
                        IsEditable = p.IsEditable,
                        IsGranted = p.IsGranted
                    })
                    .ToList();

                // 字典缓存：Name → 节点，快速查找父节点
                var nodeDict = allPermissionNodes.ToDictionary(x => x.Name);

                // 2. 先把所有子节点挂载到父节点Children，并记录父引用（供级联回溯）
                foreach (var node in allPermissionNodes)
                {
                    // 存在父权限，挂载到父节点
                    if (!string.IsNullOrEmpty(node.ParentName) && nodeDict.TryGetValue(node.ParentName, out PermissionTreeDto? parentNode))
                    {
                        parentNode.Children.Add(node);
                        node.Parent = parentNode;
                    }
                }

                // 2.1 归一化父授权：兼容历史上“子权限已授权但父权限未授权”的数据，加载后自动补选父链
                foreach (var node in allPermissionNodes)
                {
                    if (node.IsGranted)
                    {
                        node.EnsureAncestorsGranted();
                    }
                }

                // 3. 筛选顶层节点（ParentName 为空/null 的节点）
                var rootNodes = allPermissionNodes
                    .Where(x => string.IsNullOrEmpty(x.ParentName))
                    .ToList();

                groupTreeList.Add(new PermissionGroupTreeDto
                {
                    GroupName = group.Name,
                    DisplayName = group.DisplayName,
                    Permissions = rootNodes
                });
            }

            return groupTreeList;
        }

        /// <summary>
        /// 原生分组权限 List&lt;PermissionGroupDto&gt; 直接拉平成一维所有权限
        /// </summary>
        public static List<PermissionGrantInfoDto> ToFlatPermissionList(List<PermissionGroupTreeDto> groupList)
        {
            var allPermission = new List<PermissionGrantInfoDto>();
            foreach (var group in groupList)
            {
                RecursiveAdd(group.Permissions, allPermission);
            }
            return allPermission;
        }

        /// <summary>
        /// 递归遍历权限，父子全部加入集合
        /// </summary>
        private static void RecursiveAdd(List<PermissionTreeDto> source, List<PermissionGrantInfoDto> result)
        {
            foreach (var item in source)
            {
                result.Add(new PermissionGrantInfoDto
                {
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    IsEditable = item.IsEditable,
                    IsGranted = item.IsGranted,
                    ParentName = item.ParentName,
                });
                // ABP PermissionDto 自带 Children 子权限集合
                if (item.Children.Count != 0)
                {
                    RecursiveAdd(item.Children, result);
                }
            }
        }
    }
}
