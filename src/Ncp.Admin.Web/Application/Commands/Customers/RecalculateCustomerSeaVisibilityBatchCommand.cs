using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 在公海片区分配表变更后，对所有需按片区匹配可见性的客户重新计算可见性授权：
/// 未作废且在公海中的客户，或曾建立过可见性板（含领用后仍保留片区可见性的客户）。
/// </summary>
public record RecalculateCustomerSeaVisibilityBatchCommand : ICommand<int>;

public class RecalculateCustomerSeaVisibilityBatchCommandHandler(
    ApplicationDbContext dbContext,
    IMediator mediator) : ICommandHandler<RecalculateCustomerSeaVisibilityBatchCommand, int>
{
    public async Task<int> Handle(RecalculateCustomerSeaVisibilityBatchCommand request, CancellationToken cancellationToken)
    {
        var ids = await dbContext.Customers.AsNoTracking()
            .Where(c =>
                !c.IsVoided
                && (c.IsInSea || dbContext.CustomerSeaVisibilityBoards.Any(b => b.Id == c.Id)))
            .Select(c => c.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var id in ids)
            await mediator.Send(new SyncCustomerSeaVisibilityCommand(id), cancellationToken);

        return ids.Count;
    }
}
