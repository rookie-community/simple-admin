using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Admin.Web.Areas.Admin.Controllers;

/// 组织架构（占位页）。
[Area("Admin")]
[Authorize]
public class OrganizationUnitsController : AbpController
{
    public IActionResult Index()
    {
        return View();
    }
}
