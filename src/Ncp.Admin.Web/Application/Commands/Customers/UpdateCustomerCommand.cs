using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 更新客户命令。简称、负责人、主联系人、微信状态、是否重点客户不从请求接收，由 Handler 使用当前客户实体值，防止篡改。
/// </summary>
public record UpdateCustomerCommand(
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
    string BusinessLicense,
    string ContactQq,
    string ContactWechat,
    string Remark,
    bool IsHidden,
    IReadOnlyList<IndustryId> IndustryIds) : ICommand<bool>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CustomerSourceName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCustomerCommandHandler(
    ICustomerRepository repository,
    IRegionRepository regionRepository) : ICommandHandler<UpdateCustomerCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        var (provinceName, cityName, districtName) = await ResolveRegionNamesAsync(
            request.ProvinceCode, request.CityCode, request.DistrictCode, cancellationToken);
        var (phoneProvinceName, phoneCityName, phoneDistrictName) = await ResolveRegionNamesAsync(
            request.PhoneProvinceCode, request.PhoneCityCode, request.PhoneDistrictCode, cancellationToken);
        customer.Update(
            customer.OwnerId, request.CustomerSourceId, request.CustomerSourceName, request.FullName,
            customer.ShortName, request.Status, request.Nature, request.ProvinceCode,
            request.CityCode, request.DistrictCode,
            provinceName, cityName, districtName,
            request.PhoneProvinceCode, request.PhoneCityCode, request.PhoneDistrictCode,
            phoneProvinceName, phoneCityName, phoneDistrictName,
            request.ConsultationContent,
            request.CoverRegion, request.RegisterAddress, request.EmployeeCount, request.BusinessLicense ?? string.Empty,
            customer.MainContactName, customer.MainContactPhone,
            request.ContactQq, request.ContactWechat,
            customer.WechatStatus, request.Remark, customer.IsKeyAccount, request.IsHidden,
            request.IndustryIds);
        return true;
    }

    private async Task<(string ProvinceName, string CityName, string DistrictName)> ResolveRegionNamesAsync(
        string? provinceCode, string? cityCode, string? districtCode, CancellationToken cancellationToken)
    {
        var p = await ResolveOneAsync(regionRepository, provinceCode, "省", cancellationToken);
        var c = await ResolveOneAsync(regionRepository, cityCode, "市", cancellationToken);
        var d = await ResolveOneAsync(regionRepository, districtCode, "区/县", cancellationToken);
        return (p, c, d);
    }

    private static async Task<string> ResolveOneAsync(IRegionRepository repo, string? code, string label, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;
        if (!long.TryParse(code.Trim(), out var id))
            throw new KnownException($"{label}区域码格式无效", ErrorCodes.RegionNotFound);
        var region = await repo.GetAsync(new RegionId(id), ct);
        if (region == null)
            throw new KnownException($"{label}区域码不存在", ErrorCodes.RegionNotFound);
        return region.Name;
    }
}
