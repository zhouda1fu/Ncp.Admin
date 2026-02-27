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

public record CreateCustomerCommand(
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
    UserId CreatorId,
    IReadOnlyList<IndustryId> IndustryIds) : ICommand<CustomerId>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ShortName).MaximumLength(100);
    }
}

public class CreateCustomerCommandHandler(
    ICustomerRepository repository,
    ICustomerSourceRepository customerSourceRepository,
    IRegionRepository regionRepository) : ICommandHandler<CreateCustomerCommand, CustomerId>
{
    public async Task<CustomerId> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var source = await customerSourceRepository.GetAsync(request.CustomerSourceId, cancellationToken)
            ?? throw new KnownException("未找到客户来源", ErrorCodes.CustomerSourceNotFound);
        var (provinceName, cityName, districtName) = await ResolveRegionNamesAsync(
            request.ProvinceCode, request.CityCode, request.DistrictCode, cancellationToken);
        var (phoneProvinceName, phoneCityName, phoneDistrictName) = await ResolveRegionNamesAsync(
            request.PhoneProvinceCode, request.PhoneCityCode, request.PhoneDistrictCode, cancellationToken);
        var customer = new Customer(
            request.OwnerId, request.DeptId, request.CustomerSourceId, source.Name, request.FullName,
            request.ShortName , request.Nature , request.ProvinceCode ,
            request.CityCode , request.DistrictCode ,
            provinceName, cityName, districtName,
            request.PhoneProvinceCode , request.PhoneCityCode , request.PhoneDistrictCode ,
            phoneProvinceName, phoneCityName, phoneDistrictName,
            request.ConsultationContent ,
            request.CoverRegion , request.RegisterAddress , request.MainContactName , request.MainContactPhone ,
            request.ContactQq , request.ContactWechat ,
            request.WechatStatus , request.Remark , request.IsKeyAccount, request.CreatorId);
        await repository.AddAsync(customer, cancellationToken);
        if (request.IndustryIds is { Count: > 0 })
            customer.SetIndustries(request.IndustryIds);
        return customer.Id;
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
