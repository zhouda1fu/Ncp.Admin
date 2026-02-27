using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class UpdateCustomerRequest
{
    public Guid Id { get; set; }
    public long? OwnerId { get; set; }
    public long? DeptId { get; set; }
    public Guid CustomerSourceId { get; set; }
    public int StatusId { get; set; }
    public string FullName { get; set; } = "";
    public string? ShortName { get; set; }
    public string? Nature { get; set; }
    public string? ProvinceCode { get; set; }
    public string? CityCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? CoverRegion { get; set; }
    public string? RegisterAddress { get; set; }
    public string? MainContactName { get; set; }
    public string? MainContactPhone { get; set; }
    public string? WechatStatus { get; set; }
    public string? Remark { get; set; }
    public bool IsKeyAccount { get; set; }
    public bool IsHidden { get; set; }
    public IList<string>? IndustryIds { get; set; }
}

public class UpdateCustomerEndpoint(IMediator mediator) : Endpoint<UpdateCustomerRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerEdit);
    }

    public override async Task HandleAsync(UpdateCustomerRequest req, CancellationToken ct)
    {
        UserId? ownerId = req.OwnerId.HasValue ? new UserId(req.OwnerId.Value) : null;
        DeptId? deptId = req.DeptId.HasValue ? new DeptId(req.DeptId.Value) : null;
        IReadOnlyList<IndustryId>? industryIds = null;
        if (req.IndustryIds is { Count: > 0 })
            industryIds = req.IndustryIds.Select(x => new IndustryId(Guid.Parse(x))).ToList();
        var cmd = new UpdateCustomerCommand(
            new CustomerId(req.Id), ownerId, deptId, new CustomerSourceId(req.CustomerSourceId), req.StatusId, req.FullName, req.ShortName,
            req.Nature, req.ProvinceCode, req.CityCode, req.DistrictCode, req.CoverRegion, req.RegisterAddress,
            req.MainContactName, req.MainContactPhone, req.WechatStatus, req.Remark, req.IsKeyAccount, req.IsHidden, industryIds);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
