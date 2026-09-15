using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Admin.Web.Models;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;

namespace Admin.Web.Areas.Admin.Controllers;

/// <summary>角色管理。</summary>
[Area("Admin")]
[Authorize]
public class RolesController : AdminAreaController
{
    private readonly IIdentityRoleAppService _identityRoleAppService;
    private readonly IPermissionChecker _permissionChecker;

    public RolesController(
        IIdentityRoleAppService identityRoleAppService,
        IPermissionChecker permissionChecker,
        ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _identityRoleAppService = identityRoleAppService;
        _permissionChecker = permissionChecker;
    }

    public async Task<IActionResult> Index()
    {
        var model = new RoleIndexViewModel
        {
            CanCreate = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Roles.Create),
            CanUpdate = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Roles.Update),
            CanDelete = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Roles.Delete),
            CanManagePermissions = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Roles.ManagePermissions),
        };

        return View(model);
    }

    /// <summary>分页查询角色列表。</summary>
    [HttpGet]
    public Task<IActionResult> GetList(RoleListQueryInput input)
    {
        return ExecuteAsync(async () =>
        {
            var result = await _identityRoleAppService.GetListAsync(new GetIdentityRolesInput
            {
                Filter = input.Filter,
                Sorting = BuildSorting(input.Field, input.Order),
                SkipCount = (input.Page - 1) * input.Limit,
                MaxResultCount = input.Limit
            });

            return LayuiResult.OkTable(result.TotalCount, result.Items);
        });
    }

    /// <summary>新增角色。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Create(RoleSaveInput input)
    {
        return ExecuteAsync(async () =>
        {
            await _identityRoleAppService.CreateAsync(new IdentityRoleCreateDto
            {
                Name = input.Name,
                IsDefault = input.IsDefault,
                IsPublic = input.IsPublic,
            });

            return LayuiResult.Ok("新增角色成功");
        });
    }

    /// <summary>更新角色。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Update(RoleSaveInput input)
    {
        return ExecuteAsync(async () =>
        {
            if (!input.Id.HasValue)
            {
                return LayuiResult.Error("缺少角色 Id");
            }

            await _identityRoleAppService.UpdateAsync(input.Id.Value, new IdentityRoleUpdateDto
            {
                Name = input.Name,
                IsDefault = input.IsDefault,
                IsPublic = input.IsPublic,
                ConcurrencyStamp = input.ConcurrencyStamp,
            });

            return LayuiResult.Ok("更新角色成功");
        });
    }

    /// <summary>删除角色。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Delete(Guid id)
    {
        return ExecuteAsync(async () =>
        {
            await _identityRoleAppService.DeleteAsync(id);
            return LayuiResult.Ok("删除角色成功");
        });
    }

    /// <summary>批量删除角色。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> BatchDelete(string ids)
    {
        return ExecuteAsync(async () =>
        {
            var roleIds = (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();

            if (roleIds.Count == 0)
            {
                return LayuiResult.Error("未选择任何角色");
            }

            foreach (var roleId in roleIds)
            {
                await _identityRoleAppService.DeleteAsync(roleId);
            }

            return LayuiResult.Ok($"已删除 {roleIds.Count} 个角色");
        });
    }

    private static readonly Dictionary<string, string> SortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = "Name",
        ["isDefault"] = "IsDefault",
        ["isPublic"] = "IsPublic",
        ["isStatic"] = "IsStatic",
    };

    private static string? BuildSorting(string? field, string? order)
    {
        if (string.IsNullOrWhiteSpace(field) || !SortFields.TryGetValue(field, out var column))
        {
            return null;
        }

        var direction = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
        return $"{column} {direction}";
    }
}
