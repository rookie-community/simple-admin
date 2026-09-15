using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Admin.Web.Models;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;

namespace Admin.Web.Areas.Admin.Controllers;

/// <summary>用户管理。</summary>
[Area("Admin")]
[Authorize]
public class UsersController : AdminAreaController
{
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly IPermissionChecker _permissionChecker;

    public UsersController(
        IIdentityUserAppService identityUserAppService,
        IPermissionChecker permissionChecker,
        ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _identityUserAppService = identityUserAppService;
        _permissionChecker = permissionChecker;
    }

    public async Task<IActionResult> Index()
    {
        var model = new UserIndexViewModel
        {
            CanCreate = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Users.Create),
            CanUpdate = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Users.Update),
            CanDelete = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Users.Delete),
            CanManagePermissions = await _permissionChecker.IsGrantedAsync(IdentityPermissions.Users.ManagePermissions),
        };

        return View(model);
    }

    /// <summary>分页查询用户列表。</summary>
    [HttpGet]
    public Task<IActionResult> GetList(UserListQueryInput input)
    {
        return ExecuteAsync(async () =>
        {
            var result = await _identityUserAppService.GetListAsync(new GetIdentityUsersInput
            {
                Filter = input.Filter,
                Sorting = BuildSorting(input.Field, input.Order),
                SkipCount = (input.Page - 1) * input.Limit,
                MaxResultCount = input.Limit
            });

            return LayuiResult.OkTable(result.TotalCount, result.Items);
        });
    }

    /// <summary>获取可分配角色列表；编辑时携带当前用户已分配角色（以 IsDefault 标记选中）。</summary>
    [HttpGet]
    public Task<IActionResult> GetAssignableRoles(Guid? userId)
    {
        return ExecuteAsync(async () =>
        {
            var roles = await _identityUserAppService.GetAssignableRolesAsync();

            if (userId.HasValue)
            {
                var userRoles = await _identityUserAppService.GetRolesAsync(userId.Value);
                var assignedIds = userRoles.Items.Select(r => r.Id).ToHashSet();
                foreach (var role in roles.Items)
                {
                    role.IsDefault = assignedIds.Contains(role.Id);
                }
            }

            return LayuiResult.OkData(roles.Items);
        });
    }

    /// <summary>新增用户。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Create(UserSaveInput input)
    {
        return ExecuteAsync(async () =>
        {
            await _identityUserAppService.CreateAsync(new IdentityUserCreateDto
            {
                UserName = input.UserName,
                Password = input.Password,
                Name = input.Name,
                Surname = input.Surname,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                IsActive = input.IsActive,
                LockoutEnabled = input.LockoutEnabled,
                RoleNames = ParseRoleNames(input.RoleNames),
            });

            return LayuiResult.Ok("新增用户成功");
        });
    }

    /// <summary>更新用户。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Update(UserSaveInput input)
    {
        return ExecuteAsync(async () =>
        {
            if (!input.Id.HasValue)
            {
                return LayuiResult.Error("缺少用户 Id");
            }

            await _identityUserAppService.UpdateAsync(input.Id.Value, new IdentityUserUpdateDto
            {
                UserName = input.UserName,
                Password = string.IsNullOrWhiteSpace(input.Password) ? null : input.Password,
                Name = input.Name,
                Surname = input.Surname,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                IsActive = input.IsActive,
                LockoutEnabled = input.LockoutEnabled,
                RoleNames = ParseRoleNames(input.RoleNames),
            });

            return LayuiResult.Ok("更新用户成功");
        });
    }

    /// <summary>删除用户。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Delete(Guid id)
    {
        return ExecuteAsync(async () =>
        {
            await _identityUserAppService.DeleteAsync(id);
            return LayuiResult.Ok("删除用户成功");
        });
    }

    /// <summary>批量删除用户。</summary>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> BatchDelete(string ids)
    {
        return ExecuteAsync(async () =>
        {
            var userIds = (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();

            if (userIds.Count == 0)
            {
                return LayuiResult.Error("未选择任何用户");
            }

            foreach (var userId in userIds)
            {
                await _identityUserAppService.DeleteAsync(userId);
            }

            return LayuiResult.Ok($"已删除 {userIds.Count} 个用户");
        });
    }

    private static string[] ParseRoleNames(string? roleNames)
    {
        return string.IsNullOrWhiteSpace(roleNames)
            ? Array.Empty<string>()
            : roleNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static readonly Dictionary<string, string> SortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["userName"] = "UserName",
        ["name"] = "Name",
        ["email"] = "Email",
        ["phoneNumber"] = "PhoneNumber",
        ["isActive"] = "IsActive",
        ["creationTime"] = "CreationTime",
        ["lastModificationTime"] = "LastModificationTime",
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
