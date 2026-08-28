using Admin.AuditLogs;
using Admin.OrganizationUnits;
using Riok.Mapperly.Abstractions;
using Volo.Abp.AuditLogging;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;

namespace Admin;

/*
 * You can add your own mappings here.
 * [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
 * public partial class AdminApplicationMappers : MapperBase<BookDto, CreateUpdateBookDto>
 * {
 *    public override partial CreateUpdateBookDto Map(BookDto source);
 * 
 *    public override partial void Map(BookDto source, CreateUpdateBookDto destination);
 * }
 */

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AdminApplicationMappers : MapperBase<AuditLog, AuditLogDto>
{
    public override partial AuditLogDto Map(AuditLog source);
    public override partial void Map(AuditLog source, AuditLogDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class OrganizationUnitMappers : MapperBase<OrganizationUnit, OrganizationUnitDto>
{
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.SortOrder))]
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.Children))]
    public override partial OrganizationUnitDto Map(OrganizationUnit source);

    [MapperIgnoreTarget(nameof(OrganizationUnitDto.SortOrder))]
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.Children))]
    public override partial void Map(OrganizationUnit source, OrganizationUnitDto destination);
}
