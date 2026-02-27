using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 更新客户请求
/// </summary>
/// <param name="Id">客户 ID</param>
/// <param name="OwnerId">负责人用户 ID，null 表示公海</param>
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
/// <param name="IsHidden">是否隐藏</param>
/// <param name="IndustryIds">所属行业 ID 列表</param>
public record UpdateCustomerRequest(
    CustomerId Id,
    UserId OwnerId,
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
    bool IsHidden,
    IReadOnlyList<IndustryId> IndustryIds);

/// <summary>
/// 更新客户
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateCustomerEndpoint(IMediator mediator) : Endpoint<UpdateCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateCustomerRequest req, CancellationToken ct)
    {
        var cmd = new UpdateCustomerCommand(
            req.Id, req.OwnerId, req.CustomerSourceId, req.FullName, req.ShortName,
            req.Nature, req.ProvinceCode, req.CityCode, req.DistrictCode, req.PhoneProvinceCode, req.PhoneCityCode, req.PhoneDistrictCode,
            req.ConsultationContent, req.CoverRegion, req.RegisterAddress, req.MainContactName, req.MainContactPhone,
            req.ContactQq, req.ContactWechat, req.WechatStatus, req.Remark, req.IsKeyAccount, req.IsHidden, req.IndustryIds);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
