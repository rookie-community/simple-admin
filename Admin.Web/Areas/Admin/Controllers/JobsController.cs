using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Admin.Web.Areas.Admin.Controllers;

/// 定时任务（占位页）。
[Area("Admin")]
[Authorize]
public class JobsController : AbpController
{
    public IActionResult Index()
    {
        return View();
    }
}
