using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;

namespace Admin.Areas.Account.Models;

public class UserLoginInfo
{
    [Required]
    [StringLength(255)]
    public string UserNameOrEmailAddress { get; set; } = null!;

    [Required]
    [StringLength(32)]
    [DataType(DataType.Password)]
    [DisableAuditing]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}
