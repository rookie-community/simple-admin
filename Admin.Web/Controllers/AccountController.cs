using Admin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Admin.Web.Controllers;

/// 账户登录/注销：MVC 层直接调用 Identity 的 SignInManager。
[AllowAnonymous]
public class AccountController : AbpController
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("~/");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "请输入用户名和密码" });
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.UserNameOrEmailAddress,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return Json(new { success = true });
        }

        if (result.IsLockedOut)
        {
            return Json(new { success = false, message = "账户已被锁定，请稍后再试" });
        }

        if (result.RequiresTwoFactor)
        {
            return Json(new { success = false, message = "需要进行两步验证" });
        }

        return Json(new { success = false, message = "用户名或密码错误" });
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
