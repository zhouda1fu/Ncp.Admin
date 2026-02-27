using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 公海客户更新请求
/// </summary>
/// <param name="Id">客户 ID</param>
/// <param name="CustomerSourceId">客户来源 ID</param>
/// <param name="CustomerSourceName">客户来源名称（由前端传入）</param>
/// <param name="FullName">客户全称</param>
/// <param name="ShortName">客户简称</param>
/// <param name="Nature">公司性质</param>
/// <param name="ProvinceCode">省区域码</param>
/// <param name="CityCode">市区域码</param>
/// <param name="DistrictCode">区/县区域码</param>
/// <param name="ProvinceName">省名称</param>
/// <param name="CityName">市名称</param>
/// <param name="DistrictName">区/县名称</param>
/// <param name="PhoneProvinceCode">电话省区域码</param>
/// <param name="PhoneCityCode">电话市区域码</param>
/// <param name="PhoneDistrictCode">电话区/县区域码</param>
/// <param name="PhoneProvinceName">电话省名称</param>
/// <param name="PhoneCityName">电话市名称</param>
/// <param name="PhoneDistrictName">电话区/县名称</param>
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
public record UpdateSeaCustomerRequest(
    CustomerId Id,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    string FullName,
    string ShortName,
    string Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string ProvinceName,
    string CityName,
    string DistrictName,
    string PhoneProvinceCode,
    string PhoneCityCode,
    string PhoneDistrictCode,
    string PhoneProvinceName,
    string PhoneCityName,
    string PhoneDistrictName,
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
/// 公海客户更新
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateSeaCustomerEndpoint(IMediator mediator) : Endpoint<UpdateSeaCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customers/{id}/sea");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateSeaCustomerRequest req, CancellationToken ct)
    {
        var cmd = new UpdateSeaCustomerCommand(
            req.Id, req.CustomerSourceId, req.CustomerSourceName , req.FullName, req.ShortName,
            req.Nature, req.ProvinceCode, req.CityCode, req.DistrictCode,
            req.ProvinceName, req.CityName, req.DistrictName,
            req.PhoneProvinceCode, req.PhoneCityCode, req.PhoneDistrictCode,
            req.PhoneProvinceName, req.PhoneCityName, req.PhoneDistrictName,
            req.ConsultationContent, req.CoverRegion, req.RegisterAddress, req.MainContactName, req.MainContactPhone,
            req.ContactQq, req.ContactWechat, req.WechatStatus, req.Remark, req.IsKeyAccount, req.IndustryIds);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
