using System.Text.Json;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Order;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 订单审批工作流变量构建器（应用层）：负责将订单聚合快照映射为工作流 Variables JSON 所需结构。
/// </summary>
public static class OrderWorkflowVariablesBuilder
{
    public static OrderWorkflowVariables Build(Order order)
    {
        return new OrderWorkflowVariables
        {
            OrderId = order.Id.ToString(),
            OrderNumber = order.OrderNumber,
            Type = (int)order.Type,
            Amount = order.Amount,
            CustomerName = order.CustomerName,
            OwnerName = order.OwnerName,
            PaymentStatus = order.PaymentStatus,
            CategoryDiscountPoints = BuildCategoryDiscountPoints(order),
            ContractAmount = order.ContractAmount,
            ContractNotCompanyTemplate = order.ContractNotCompanyTemplate,
            NeedInvoice = order.NeedInvoice,
            IsShipped = order.IsShipped,
            LogisticsPaymentMethodId = order.LogisticsPaymentMethodId,
            WarehouseStatus = order.WarehouseStatus,
        };
    }

    public static string BuildJson(Order order)
    {
        var variables = Build(order);
        return JsonSerializer.Serialize(variables);
    }

    private static Dictionary<string, decimal> BuildCategoryDiscountPoints(Order order)
    {
        var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in order.Categories)
        {
            var key = c.ProductCategoryId.ToString();
            if (dict.ContainsKey(key))
            {
                throw new KnownException($"订单按分类合同优惠存在重复分类：{key}");
            }
            dict[key] = c.DiscountPoints;
        }
        return dict;
    }
}

