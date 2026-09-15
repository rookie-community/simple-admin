namespace Admin.Web.Models;

/// <summary>
/// 用户列表查询条件，字段名对齐 layui table 的默认分页 / 排序参数。
/// </summary>
public class UserListQueryInput
{
    public string? Filter { get; set; }
    public string? Field { get; set; }
    public string? Order { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}
