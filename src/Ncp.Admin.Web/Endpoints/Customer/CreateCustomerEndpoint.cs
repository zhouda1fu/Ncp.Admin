using System.Security.Claims;
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

/// <summary>
/// 创建客户请求
/// </summary>
/// <param name="OwnerId">负责人用户 ID，null 表示公海客户</param>
/// <param name="DeptId">所属部门 ID</param>
/// <param name="CustomerSourceId">客户来源 ID</param>
/// <param name="FullName">客户全称</param>
/// <param name="ShortName">客户简称</param>
/// <param name="Nature">公司性质</param>
/// <param name="ProvinceCode">省区域码</param>
/// <param name="CityCode">市区域码</param>
/// <param name="DistrictCode">区/县区域码</param>
/// <param name="PhoneProvinceCode">电话省区域码</param>
/// <param name="PhoneCityCode">电话市区域码</param>
/// <param name="PhoneDistrictCode">电话区/县区域码</param>
/// <param name="ConsultationContent">咨询内容</param>
/// <param name="CoverRegion">覆盖区域</param>
/// <param name="RegisterAddress">注册地址</param>
/// <param name="MainContactName">主联系人姓名</param>
/// <param name="MainContactPhone">主联系人电话</param>
/// <param name="ContactQq">QQ</param>
/// <param name="ContactWechat">微信</param>
/// <param name="WechatStatus">微信状态</param>
/// <param name="Remark">备注</param>
/// <param name="IsKeyAccount">是否重点客户</param>
/// <param name="IndustryIds">所属行业 ID 列表</param>
public record CreateCustomerRequest(
    UserId OwnerId,
    DeptId DeptId,
    CustomerSourceId CustomerSourceId,
    string FullName,
    string ShortName,
    string Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string PhoneProvinceCode,
    string PhoneCityCode,
    string PhoneDistrictCode,
    string ConsultationContent,
    string CoverRegion,
    string RegisterAddress,
    string MainContactName,
    string MainContactPhone,
    string ContactQq,
    string ContactWechat,
    string WechatStatus,
    string Remark,
    bool IsKeyAccount,
    IReadOnlyList<IndustryId> IndustryIds);

/// <summary>
/// 创建客户响应
/// </summary>
/// <param name="Id">新创建的客户 ID</param>
public record CreateCustomerResponse(CustomerId Id);

/// <summary>
/// 创建客户
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class CreateCustomerEndpoint(IMediator mediator) : Endpoint<CreateCustomerRequest, ResponseData<CreateCustomerResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerCreate);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateCustomerCommand(
            req.OwnerId, req.DeptId, req.CustomerSourceId, req.FullName, req.ShortName,
            req.Nature, req.ProvinceCode, req.CityCode, req.DistrictCode, req.PhoneProvinceCode, req.PhoneCityCode, req.PhoneDistrictCode,
            req.ConsultationContent, req.CoverRegion, req.RegisterAddress, req.MainContactName, req.MainContactPhone,
            req.ContactQq, req.ContactWechat, req.WechatStatus, req.Remark, req.IsKeyAccount, new UserId(uid), req.IndustryIds);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateCustomerResponse(id).AsResponseData(), cancellation: ct);
    }
}
