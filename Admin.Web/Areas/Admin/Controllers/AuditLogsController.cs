using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Admin.Web.Areas.Admin.Controllers;

/// 审计日志（占位页）。
[Area("Admin")]
[Authorize]
public class AuditLogsController : AbpController
{
    public IActionResult Index()
    {
        return View();
    }
}
