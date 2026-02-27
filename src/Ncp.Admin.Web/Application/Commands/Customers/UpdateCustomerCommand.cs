using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record UpdateCustomerCommand(
    CustomerId Id,
    UserId? OwnerId,
    DeptId? DeptId,
    CustomerSourceId CustomerSourceId,
    string FullName,
    string? ShortName,
    string? Nature,
    string? ProvinceCode,
    string? CityCode,
    string? DistrictCode,
    string? PhoneProvinceCode,
    string? PhoneCityCode,
    string? PhoneDistrictCode,
    string? ConsultationContent,
    string? CoverRegion,
    string? RegisterAddress,
    string? MainContactName,
    string? MainContactPhone,
    string? ContactQq,
    string? ContactWechat,
    string? WechatStatus,
    string? Remark,
    bool IsKeyAccount,
    bool IsHidden,
    IReadOnlyList<IndustryId>? IndustryIds = null) : ICommand<bool>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ShortName).MaximumLength(100);
    }
}

public class UpdateCustomerCommandHandler(
    ICustomerRepository repository,
    ICustomerSourceRepository customerSourceRepository,
    IRegionRepository regionRepository) : ICommandHandler<UpdateCustomerCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        var source = await customerSourceRepository.GetAsync(request.CustomerSourceId, cancellationToken)
            ?? throw new KnownException("未找到客户来源", ErrorCodes.CustomerSourceNotFound);
        var (provinceName, cityName, districtName) = await ResolveRegionNamesAsync(
            request.ProvinceCode, request.CityCode, request.DistrictCode, cancellationToken);
        var (phoneProvinceName, phoneCityName, phoneDistrictName) = await ResolveRegionNamesAsync(
            request.PhoneProvinceCode, request.PhoneCityCode, request.PhoneDistrictCode, cancellationToken);
        customer.Update(
            request.OwnerId, request.DeptId, request.CustomerSourceId, source.Name, request.FullName,
            request.ShortName ?? string.Empty, request.Nature ?? string.Empty, request.ProvinceCode ?? string.Empty,
            request.CityCode ?? string.Empty, request.DistrictCode ?? string.Empty,
            provinceName, cityName, districtName,
            request.PhoneProvinceCode ?? string.Empty, request.PhoneCityCode ?? string.Empty, request.PhoneDistrictCode ?? string.Empty,
            phoneProvinceName, phoneCityName, phoneDistrictName,
            request.ConsultationContent ?? string.Empty,
            request.CoverRegion ?? string.Empty, request.RegisterAddress ?? string.Empty, request.MainContactName ?? string.Empty, request.MainContactPhone ?? string.Empty,
            request.ContactQq ?? string.Empty, request.ContactWechat ?? string.Empty,
            request.WechatStatus ?? string.Empty, request.Remark ?? string.Empty, request.IsKeyAccount, request.IsHidden,
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
