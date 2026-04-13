using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaVisibilityAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

public interface ICustomerSeaVisibilityBoardRepository : IRepository<CustomerSeaVisibilityBoard, CustomerId>
{
    Task<CustomerSeaVisibilityBoard?> GetWithEntriesAsync(CustomerId customerId, CancellationToken cancellationToken = default);
}

public class CustomerSeaVisibilityBoardRepository(ApplicationDbContext context)
    : RepositoryBase<CustomerSeaVisibilityBoard, CustomerId, ApplicationDbContext>(context), ICustomerSeaVisibilityBoardRepository
{
    public async Task<CustomerSeaVisibilityBoard?> GetWithEntriesAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        return await DbContext.CustomerSeaVisibilityBoards
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
    }
}
