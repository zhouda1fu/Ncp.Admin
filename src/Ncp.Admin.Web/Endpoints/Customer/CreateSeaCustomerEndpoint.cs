using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 公海创建客户请求（仅包含公海录入表单字段，均为必填）
/// </summary>
/// <param name="CustomerSourceId">客户来源 ID</param>
/// <param name="CustomerSourceName">客户来源名称（由前端传入）</param>
/// <param name="MainContactName">主联系人姓名</param>
/// <param name="MainContactPhone">主联系人电话</param>
/// <param name="ContactQq">QQ</param>
/// <param name="ContactWechat">微信</param>
/// <param name="PhoneProvinceCode">电话省区域码</param>
/// <param name="PhoneCityCode">电话市区域码</param>
/// <param name="PhoneDistrictCode">电话区/县区域码</param>
/// <param name="PhoneProvinceName">电话省名称</param>
/// <param name="PhoneCityName">电话市名称</param>
/// <param name="PhoneDistrictName">电话区/县名称</param>
/// <param name="ProvinceCode">省区域码</param>
/// <param name="CityCode">市区域码</param>
/// <param name="DistrictCode">区/县区域码</param>
/// <param name="ProvinceName">省名称</param>
/// <param name="CityName">市名称</param>
/// <param name="DistrictName">区/县名称</param>
/// <param name="ConsultationContent">咨询内容</param>
public record CreateSeaCustomerRequest(
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    string MainContactName,
    string MainContactPhone,
    string ContactQq,
    string ContactWechat,
    string PhoneProvinceCode,
    string PhoneCityCode,
    string PhoneDistrictCode,
    string PhoneProvinceName,
    string PhoneCityName,
    string PhoneDistrictName,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string ProvinceName,
    string CityName,
    string DistrictName,
    string ConsultationContent);

/// <summary>
/// 公海创建客户响应
/// </summary>
/// <param name="Id">新创建的客户 ID</param>
public record CreateSeaCustomerResponse(CustomerId Id);

/// <summary>
/// 公海创建客户（公海录入专用接口）
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class CreateSeaCustomerEndpoint(IMediator mediator) : Endpoint<CreateSeaCustomerRequest, ResponseData<CreateSeaCustomerResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/sea");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerCreate);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateSeaCustomerRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var creatorName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var cmd = new CreateSeaCustomerCommand(
            req.CustomerSourceId,
            req.CustomerSourceName,
            req.MainContactName,
            req.MainContactPhone,
            req.ContactQq,
            req.ContactWechat,
            req.PhoneProvinceCode,
            req.PhoneCityCode,
            req.PhoneDistrictCode,
            req.PhoneProvinceName,
            req.PhoneCityName,
            req.PhoneDistrictName,
            req.ProvinceCode,
            req.CityCode,
            req.DistrictCode,
            req.ProvinceName,
            req.CityName,
            req.DistrictName,
            req.ConsultationContent,
            new UserId(uid),
            creatorName);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateSeaCustomerResponse(id).AsResponseData(), cancellation: ct);
    }
}
