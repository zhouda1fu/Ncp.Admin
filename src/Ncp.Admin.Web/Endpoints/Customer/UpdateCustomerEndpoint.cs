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
/// <param name="CustomerSourceId">客户来源 ID</param>
/// <param name="CustomerSourceName">客户来源名称（前端传入）</param>
/// <param name="FullName">客户全称</param>
/// <param name="Status">客户状态（枚举值）</param>
/// <param name="Nature">公司性质（枚举值）</param>
/// <param name="ProvinceCode">省区域码</param>
/// <param name="CityCode">市区域码</param>
/// <param name="DistrictCode">区/县区域码</param>
/// <param name="PhoneProvinceCode">电话省区域码</param>
/// <param name="PhoneCityCode">电话市区域码</param>
/// <param name="PhoneDistrictCode">电话区/县区域码</param>
/// <param name="ConsultationContent">咨询内容</param>
/// <param name="CoverRegion">覆盖区域</param>
/// <param name="RegisterAddress">注册地址</param>
/// <param name="EmployeeCount">员工数量</param>
/// <param name="BusinessLicense">营业执照（路径或 URL）</param>
/// <param name="ContactQq">QQ</param>
/// <param name="ContactWechat">微信</param>
/// <param name="Remark">备注</param>
/// <param name="IsHidden">是否隐藏</param>
/// <param name="IndustryIds">所属行业 ID 列表</param>
/// <remarks>简称、负责人、主联系人、微信状态、是否重点客户不接受请求入参，由后端沿用当前客户数据，防止篡改。</remarks>
public record UpdateCustomerRequest(
    CustomerId Id,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    string FullName,
    CustomerStatus? Status,
    CompanyNature? Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string PhoneProvinceCode,
    string PhoneCityCode,
    string PhoneDistrictCode,
    string ConsultationContent,
    string CoverRegion,
    string RegisterAddress,
    int EmployeeCount,
    string? BusinessLicense,
    string ContactQq,
    string ContactWechat,
    string Remark,
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
            req.Id, req.CustomerSourceId, req.CustomerSourceName, req.FullName,
            req.Status, req.Nature, req.ProvinceCode, req.CityCode, req.DistrictCode, req.PhoneProvinceCode, req.PhoneCityCode, req.PhoneDistrictCode,
            req.ConsultationContent, req.CoverRegion, req.RegisterAddress, req.EmployeeCount, req.BusinessLicense ?? string.Empty,
            req.ContactQq, req.ContactWechat, req.Remark, req.IsHidden, req.IndustryIds);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
