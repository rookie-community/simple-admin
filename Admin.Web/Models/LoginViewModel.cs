using System.ComponentModel.DataAnnotations;

namespace Admin.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "请输入用户名或邮箱")]
    public string UserNameOrEmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入密码")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    // 预留字段：多租户/语言切换尚未启用
    public string? Tenant { get; set; }

    public string Language { get; set; } = "zh-Hans";
}
