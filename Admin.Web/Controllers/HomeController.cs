using Admin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.UI.Navigation;

namespace Admin.Web.Controllers
{
    [Authorize]
    public class HomeController : AbpController
    {
        private readonly IMenuManager _menuManager;

        public HomeController(IMenuManager menuManager)
        {
            _menuManager = menuManager;
        }

        public async Task<IActionResult> Index()
        {
            var menu = await _menuManager.GetAsync(StandardMenus.Main);
            return View(menu);
        }

        public IActionResult Console()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
