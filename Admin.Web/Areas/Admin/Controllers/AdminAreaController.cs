using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Admin.Web.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Authorization;
using Volo.Abp.Validation;

namespace Admin.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin 区域管理页控制器基类：以 layui 约定的 <c>{code, msg, data, count}</c> 结构返回结果，并统一捕获异常。
/// </summary>
public abstract class AdminAreaController : AbpController
{
    private readonly ILogger _logger;

    protected AdminAreaController(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    /// <summary>
    /// 执行指定动作并序列化为 <see cref="LayuiResult"/>；对校验、授权、业务及未知异常统一转换。
    /// </summary>
    protected async Task<IActionResult> ExecuteAsync(Func<Task<LayuiResult>> action)
    {
        try
        {
            return Json(await action());
        }
        catch (AbpValidationException ex)
        {
            var message = string.Join("；", ex.ValidationErrors.Select(e => e.ErrorMessage).Distinct());
            _logger.LogWarning(ex, "参数校验失败：{Message}", message);
            return Json(LayuiResult.Error(message));
        }
        catch (AbpAuthorizationException ex)
        {
            _logger.LogWarning(ex, "无权访问：{Path}", Request.Path);
            return Json(LayuiResult.Error("您没有执行该操作的权限", 403));
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "业务异常：{Code}", ex.Code);
            return Json(LayuiResult.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理请求失败：{Path}", Request.Path);
            return Json(LayuiResult.Error("服务器内部错误，请稍后重试"));
        }
    }
}
