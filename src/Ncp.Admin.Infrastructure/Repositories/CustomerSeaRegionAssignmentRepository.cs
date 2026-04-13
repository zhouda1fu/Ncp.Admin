using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface ICustomerSeaRegionAssignmentRepository : IRepository<CustomerSeaRegionAssignment, CustomerSeaRegionAssignmentId>
{
    Task<CustomerSeaRegionAssignment?> GetByTargetUserIdWithRegionsAsync(
        UserId targetUserId,
        CancellationToken cancellationToken = default);
}

public interface ICustomerSeaRegionAssignmentAuditRepository : IRepository<CustomerSeaRegionAssignmentAudit, CustomerSeaRegionAssignmentAuditId>
{
    Task<CustomerSeaRegionAssignmentAudit?> GetByIdWithDetailsAsync(
        CustomerSeaRegionAssignmentAuditId id,
        CancellationToken cancellationToken = default);
}

public class CustomerSeaRegionAssignmentRepository(ApplicationDbContext context) :
    RepositoryBase<CustomerSeaRegionAssignment, CustomerSeaRegionAssignmentId, ApplicationDbContext>(context),
    ICustomerSeaRegionAssignmentRepository
{
    public async Task<CustomerSeaRegionAssignment?> GetByTargetUserIdWithRegionsAsync(
        UserId targetUserId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.CustomerSeaRegionAssignments
            .Include(x => x.Regions)
            .FirstOrDefaultAsync(x => x.TargetUserId == targetUserId, cancellationToken);
    }
}

public class CustomerSeaRegionAssignmentAuditRepository(ApplicationDbContext context) :
    RepositoryBase<CustomerSeaRegionAssignmentAudit, CustomerSeaRegionAssignmentAuditId, ApplicationDbContext>(context),
    ICustomerSeaRegionAssignmentAuditRepository
{
    public async Task<CustomerSeaRegionAssignmentAudit?> GetByIdWithDetailsAsync(
        CustomerSeaRegionAssignmentAuditId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.CustomerSeaRegionAssignmentAudits
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}

