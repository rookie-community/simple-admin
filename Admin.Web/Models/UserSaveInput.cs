namespace Admin.Web.Models;

/// <summary>
/// 新增 / 编辑用户的表单输入（Web 层专用）。
/// 使用扁平结构避免直接绑定 ABP 复杂 DTO（如 ExtraProperties、List 绑定等）。
/// </summary>
public class UserSaveInput
{
    /// <summary>为空表示新增，否则为待更新用户的 Id。</summary>
    public Guid? Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    /// <summary>新增时必填；编辑时留空表示不修改密码。</summary>
    public string Password { get; set; } = string.Empty;

    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool LockoutEnabled { get; set; }

    /// <summary>角色名列表，逗号分隔。</summary>
    public string? RoleNames { get; set; }
}
