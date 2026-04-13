using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Primitives;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 添加客户联系记录请求
/// </summary>
/// <param name="CustomerId">客户 ID</param>
/// <param name="RecordAt">联系时间</param>
/// <param name="RecordType">联系类型</param>
/// <param name="Title">标题</param>
/// <param name="Content">内容</param>
/// <param name="NextVisitAt">下次回访时间</param>
/// <param name="Status">状态</param>
/// <param name="CustomerContactIds">关联联系人 ID 列表</param>
/// <param name="OwnerId">负责人用户 ID（为空则使用当前用户）</param>
/// <param name="Remark">备注</param>
/// <param name="ReminderIntervalDays">提醒间隔（天）</param>
/// <param name="ReminderCount">提醒次数</param>
/// <param name="FilePath">附件路径</param>
/// <param name="CustomerAddress">客户地址</param>
/// <param name="VisitAddress">拜访地址</param>
public record AddCustomerContactRecordRequest(
    CustomerId CustomerId,
    DateTimeOffset RecordAt,
    CustomerContactRecordType RecordType,
    string? Title,
    string? Content,
    DateTimeOffset? NextVisitAt,
    CustomerContactRecordStatus Status,
    IReadOnlyList<CustomerContactId>? CustomerContactIds,
    UserId? OwnerId,
    string? Remark,
    int ReminderIntervalDays,
    int ReminderCount,
    string? FilePath,
    string? CustomerAddress,
    string? VisitAddress);

/// <summary>
/// 添加客户联系记录响应
/// </summary>
/// <param name="Id">新建联系记录 ID</param>
public record AddCustomerContactRecordResponse(CustomerContactRecordId Id);

/// <summary>
/// 添加客户联系记录
/// </summary>
public class AddCustomerContactRecordEndpoint(IMediator mediator) : Endpoint<AddCustomerContactRecordRequest, ResponseData<AddCustomerContactRecordResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{customerId}/contact-records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
        Description(b => b.AutoTagOverride("Customer").WithSummary("添加客户联系记录"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(AddCustomerContactRecordRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var creatorId))
            throw new KnownException("未登录或用户标识无效");
        var ownerId = req.OwnerId ?? creatorId;
        var interval = CustomerContactRecordReminderInterval.IsValid(req.ReminderIntervalDays)
            ? req.ReminderIntervalDays
            : 1;
        var count = req.ReminderCount < 1 ? 1 : req.ReminderCount;
        var cmd = new AddCustomerContactRecordCommand(
            req.CustomerId,
            req.RecordAt,
            req.RecordType,
            req.Title ?? string.Empty,
            req.Content ?? string.Empty,
            req.NextVisitAt,
            req.Status,
            req.CustomerContactIds,
            ownerId,
            creatorId,
            req.Remark ?? string.Empty,
            interval,
            count,
            req.FilePath ?? string.Empty,
            req.CustomerAddress ?? string.Empty,
            req.VisitAddress ?? string.Empty);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AddCustomerContactRecordResponse(id).AsResponseData(), cancellation: ct);
    }
}
