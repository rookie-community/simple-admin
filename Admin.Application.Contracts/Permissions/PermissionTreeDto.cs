using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Volo.Abp.Application.Dtos;

namespace Admin.Permissions
{
    /// <summary>权限树形节点</summary>
    public class PermissionTreeDto : EntityDto, INotifyPropertyChanged
    {
        private bool _isGranted;

        /// <summary>权限唯一标识（PermissionName）</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>权限显示名称</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>父权限Name，顶层权限为空</summary>
        public string ParentName { get; set; } = string.Empty;

        /// <summary>所属分组名称</summary>
        public string GroupName { get; set; } = string.Empty;

        public bool IsEditable { get; set; }

        /// <summary>是否已授权（勾选/取消时自动级联子孙与祖先）</summary>
        public bool IsGranted
        {
            get => _isGranted;
            set
            {
                if (_isGranted == value)
                {
                    return;
                }
                if (!IsEditable)
                {
                    // 不可编辑节点仅展示服务端值，不参与级联
                    SetIsGranted(value);
                    return;
                }
                if (value)
                {
                    GrantTree();
                }
                else
                {
                    RevokeTree();
                }
            }
        }

        /// <summary>子权限集合（递归树形）</summary>
        public List<PermissionTreeDto> Children { get; set; } = new();

        /// <summary>
        /// 父节点引用（仅内存树回溯使用，internal 避免被序列化/拷贝带入循环）。
        /// </summary>
        internal PermissionTreeDto? Parent { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 勾选当前节点：本级勾选 → 递归勾选所有子孙 → 沿父链自动勾选祖先。
        /// </summary>
        private void GrantTree()
        {
            SetIsGranted(true);
            foreach (var child in Children)
            {
                child.GrantDescendantsRecursively();
            }
            CascadeGrantToAncestors();
        }

        private void GrantDescendantsRecursively()
        {
            if (!IsEditable)
            {
                return;
            }
            SetIsGranted(true);
            foreach (var child in Children)
            {
                child.GrantDescendantsRecursively();
            }
        }

        /// <summary>
        /// 加载后归一化：若当前权限已授权，则确保其祖先链上的可编辑父权限同步授权。
        /// 用于兼容历史上“只勾子不勾父”保存下来的数据。
        /// </summary>
        internal void EnsureAncestorsGranted()
        {
            CascadeGrantToAncestors();
        }

        /// <summary>
        /// 勾选子孙后，将祖先链上所有可编辑父节点一并勾选。
        /// </summary>
        private void CascadeGrantToAncestors()
        {
            var ancestor = Parent;
            while (ancestor != null && ancestor.IsEditable)
            {
                ancestor.SetIsGranted(true);
                ancestor = ancestor.Parent;
            }
        }

        /// <summary>
        /// 取消勾选当前节点：本级取消 → 递归取消所有子孙 → 沿父链自动更新祖先。
        /// </summary>
        private void RevokeTree()
        {
            SetIsGranted(false);
            foreach (var child in Children)
            {
                child.RevokeDescendantsRecursively();
            }
            CascadeRevokeToAncestors();
        }

        private void RevokeDescendantsRecursively()
        {
            if (!IsEditable)
            {
                return;
            }
            SetIsGranted(false);
            foreach (var child in Children)
            {
                child.RevokeDescendantsRecursively();
            }
        }

        /// <summary>
        /// 取消子孙后沿父链向上重算：
        /// 只要仍有任意可编辑子节点处于勾选状态，父节点就保持勾选；
        /// 若全部子节点都未勾选，则父节点随之取消。
        /// </summary>
        private void CascadeRevokeToAncestors()
        {
            var ancestor = Parent;
            while (ancestor != null && ancestor.IsEditable)
            {
                var anyGrantedChild = ancestor.Children.Any(child => child.IsEditable && child.IsGranted);
                if (anyGrantedChild)
                {
                    break;
                }
                ancestor.SetIsGranted(false);
                ancestor = ancestor.Parent;
            }
        }

        private void SetIsGranted(bool value)
        {
            if (_isGranted == value)
            {
                return;
            }
            _isGranted = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGranted)));
        }
    }
}
