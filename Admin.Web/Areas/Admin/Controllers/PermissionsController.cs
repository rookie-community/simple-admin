using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Admin.Web.Areas.Admin.Controllers;

/// 权限管理（占位页）。
[Area("Admin")]
[Authorize]
public class PermissionsController : AbpController
{
    public IActionResult Index()
    {
        return View();
    }
}
