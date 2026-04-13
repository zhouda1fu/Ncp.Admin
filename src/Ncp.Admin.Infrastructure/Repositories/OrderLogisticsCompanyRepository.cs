using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsCompanyAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 订单物流公司仓储接口
/// </summary>
public interface IOrderLogisticsCompanyRepository : IRepository<OrderLogisticsCompany, OrderLogisticsCompanyId>
{
}

/// <summary>
/// 订单物流公司仓储实现
/// </summary>
public class OrderLogisticsCompanyRepository(ApplicationDbContext context)
    : RepositoryBase<OrderLogisticsCompany, OrderLogisticsCompanyId, ApplicationDbContext>(context), IOrderLogisticsCompanyRepository
{
}
