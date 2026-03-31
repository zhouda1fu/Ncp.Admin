using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Domain;

/// <summary>
/// 订单物流公司仓储接口
/// </summary>
public interface IOrderLogisticsCompanyRepository : IRepository<OrderLogisticsCompany, OrderLogisticsCompanyId>
{
}
