namespace Admin.Web.Models;

/// <summary>
/// 新增 / 编辑角色的表单输入（Web 层专用）。
/// </summary>
public class RoleSaveInput
{
    /// <summary>为空表示新增，否则为待更新角色的 Id。</summary>
    public Guid? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsPublic { get; set; }

    /// <summary>编辑时用于乐观并发控制，新增时无需提供。</summary>
    public string? ConcurrencyStamp { get; set; }
}
