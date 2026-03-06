using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

public record CustomerSourceDto(CustomerSourceId Id, string Name, int SortOrder, CustomerSourceUsageScene UsageScene);

public class CustomerSourceQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 获取客户来源列表，可按使用场景过滤（null 表示不过滤，返回全部）。
    /// </summary>
    public async Task<IReadOnlyList<CustomerSourceDto>> GetListAsync(CustomerSourceUsageScene? scene = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.CustomerSources.AsNoTracking();
        if (scene.HasValue)
        {
            query = scene.Value switch
            {
                CustomerSourceUsageScene.Sea => query.Where(x => x.UsageScene == CustomerSourceUsageScene.Sea || x.UsageScene == CustomerSourceUsageScene.Both),
                CustomerSourceUsageScene.List => query.Where(x => x.UsageScene == CustomerSourceUsageScene.List || x.UsageScene == CustomerSourceUsageScene.Both),
                _ => query
            };
        }
        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CustomerSourceDto(x.Id, x.Name, x.SortOrder, x.UsageScene))
            .ToListAsync(cancellationToken);
    }
}
