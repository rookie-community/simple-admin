namespace Admin.Web.Models;

/// <summary>
/// 角色管理列表页视图模型：携带当前登录用户的按钮级权限，用于控制工具栏与行操作按钮显隐。
/// </summary>
public class RoleIndexViewModel
{
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanManagePermissions { get; set; }
}
