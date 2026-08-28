using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Admin.OrganizationUnits
{
    /// <summary>
    /// 组织单元应用服务实现
    /// </summary>
    [Authorize]
    public class OrganizationUnitAppService : ApplicationService, IOrganizationUnitAppService
    {
        private readonly IOrganizationUnitRepository _organizationUnitRepository;
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly IIdentityRoleRepository _identityRoleRepository;

        public OrganizationUnitAppService(
            IOrganizationUnitRepository organizationUnitRepository,
            IIdentityUserRepository identityUserRepository,
            IIdentityRoleRepository identityRoleRepository)
        {
            _organizationUnitRepository = organizationUnitRepository;
            _identityUserRepository = identityUserRepository;
            _identityRoleRepository = identityRoleRepository;
        }

        /// <summary>
        /// 获取组织单元列表（树形结构）
        /// </summary>
        public async Task<PagedResultDto<OrganizationUnitDto>> GetListAsync(GetOrganizationUnitsInput input)
        {
            if (string.IsNullOrEmpty(input.Sorting))
            {
                input.Sorting = nameof(OrganizationUnit.Code);
            }

            var totalCount = await _organizationUnitRepository.GetCountAsync();
            var organizationUnits = await _organizationUnitRepository.GetListAsync(input.Sorting);

            // 构建树形结构
            var dtos = organizationUnits.Select(ou => new OrganizationUnitDto
            {
                Id = ou.Id,
                ParentId = ou.ParentId,
                DisplayName = ou.DisplayName,
                Code = ou.Code,
                CreationTime = ou.CreationTime,
                Children = new List<OrganizationUnitDto>()
            }).ToList();

            var tree = BuildTree(dtos, input.ParentId);

            return new PagedResultDto<OrganizationUnitDto>
            {
                TotalCount = totalCount,
                Items = tree
            };
        }

        /// <summary>
        /// 获取单个组织单元详情
        /// </summary>
        public async Task<OrganizationUnitDto> GetAsync(Guid id)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(id);
            return new OrganizationUnitDto
            {
                Id = organizationUnit.Id,
                ParentId = organizationUnit.ParentId,
                DisplayName = organizationUnit.DisplayName,
                Code = organizationUnit.Code,
                CreationTime = organizationUnit.CreationTime,
                Children = new List<OrganizationUnitDto>()
            };
        }

        /// <summary>
        /// 创建组织单元
        /// </summary>
        public async Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input)
        {
            var organizationUnit = new OrganizationUnit(
                GuidGenerator.Create(),
                input.DisplayName,
                input.ParentId,
                CurrentTenant.Id
            );

            await _organizationUnitRepository.InsertAsync(organizationUnit);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new OrganizationUnitDto
            {
                Id = organizationUnit.Id,
                ParentId = organizationUnit.ParentId,
                DisplayName = organizationUnit.DisplayName,
                Code = organizationUnit.Code,
                CreationTime = organizationUnit.CreationTime,
                Children = new List<OrganizationUnitDto>()
            };
        }

        /// <summary>
        /// 更新组织单元
        /// </summary>
        public async Task<OrganizationUnitDto> UpdateAsync(UpdateOrganizationUnitInput input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.Id);

            // 使用反射或 ExtraProperties 来更新 DisplayName
            // 由于 ABP Identity 的 OrganizationUnit 是受保护的，我们暂时不实现更新
            throw new NotImplementedException("更新组织单元功能暂未实现");
        }

        /// <summary>
        /// 删除组织单元
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            await _organizationUnitRepository.DeleteAsync(id);
        }

        /// <summary>
        /// 获取组织单元成员列表
        /// </summary>
        public async Task<PagedResultDto<IdentityUserDto>> GetUsersAsync(GetOrganizationUnitUsersInput input)
        {
            // TODO: 实现获取成员的逻辑
            return new PagedResultDto<IdentityUserDto>
            {
                TotalCount = 0,
                Items = new List<IdentityUserDto>()
            };
        }

        /// <summary>
        /// 添加成员到组织单元
        /// </summary>
        public async Task AddUserAsync(Guid organizationUnitId, Guid userId)
        {
            // TODO: 实现添加成员的逻辑
            await Task.CompletedTask;
        }

        /// <summary>
        /// 从组织单元移除成员
        /// </summary>
        public async Task RemoveUserAsync(Guid organizationUnitId, Guid userId)
        {
            // TODO: 实现移除成员的逻辑
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取组织单元角色列表
        /// </summary>
        public async Task<PagedResultDto<IdentityRoleDto>> GetRolesAsync(GetOrganizationUnitRolesInput input)
        {
            // TODO: 实现获取角色的逻辑
            return new PagedResultDto<IdentityRoleDto>
            {
                TotalCount = 0,
                Items = new List<IdentityRoleDto>()
            };
        }

        /// <summary>
        /// 添加角色到组织单元
        /// </summary>
        public async Task AddRoleAsync(Guid organizationUnitId, Guid roleId)
        {
            // TODO: 实现添加角色的逻辑
            await Task.CompletedTask;
        }

        /// <summary>
        /// 从组织单元移除角色
        /// </summary>
        public async Task RemoveRoleAsync(Guid organizationUnitId, Guid roleId)
        {
            // TODO: 实现移除角色的逻辑
            await Task.CompletedTask;
        }

        /// <summary>
        /// 构建树形结构
        /// </summary>
        private List<OrganizationUnitDto> BuildTree(List<OrganizationUnitDto> allUnits, Guid? parentId)
        {
            var result = new List<OrganizationUnitDto>();

            foreach (var unit in allUnits.Where(u => u.ParentId == parentId))
            {
                var dto = new OrganizationUnitDto
                {
                    Id = unit.Id,
                    ParentId = unit.ParentId,
                    DisplayName = unit.DisplayName,
                    SortOrder = unit.SortOrder,
                    Code = unit.Code,
                    CreationTime = unit.CreationTime,
                    Children = BuildTree(allUnits, unit.Id)
                };
                result.Add(dto);
            }

            return result.OrderBy(x => x.SortOrder).ToList();
        }
    }
}
