using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Web.Application.IntegrationEvents;

public record OrderPaidIntegrationEvent(OrderId OrderId);
