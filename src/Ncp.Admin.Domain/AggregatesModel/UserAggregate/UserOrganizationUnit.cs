using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// 用户组织架构关系实体
/// 表示用户与组织架构的一对一关系
/// </summary>
public class UserOrganizationUnit
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// 组织架构ID
    /// </summary>
    public OrganizationUnitId OrganizationUnitId { get; private set; } = default!;

    /// <summary>
    /// 组织架构名称
    /// </summary>
    public string OrganizationUnitName { get; private set; } = string.Empty;

    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTimeOffset AssignedAt { get; init; }

    protected UserOrganizationUnit()
    {
    }

    /// <summary>
    /// 创建用户组织架构关系
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="organizationUnitId">组织架构ID</param>
    /// <param name="organizationUnitName">组织架构名称</param>
    public UserOrganizationUnit(UserId userId, OrganizationUnitId organizationUnitId, string organizationUnitName)
    {
        UserId = userId;
        OrganizationUnitId = organizationUnitId;
        AssignedAt = DateTimeOffset.UtcNow;
        OrganizationUnitName = organizationUnitName;
    }
}

