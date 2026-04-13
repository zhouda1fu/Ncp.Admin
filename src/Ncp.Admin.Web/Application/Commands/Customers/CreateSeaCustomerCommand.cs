using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

internal static class SeaCustomerRegionValidation
{
    internal static bool HasAnyRegionCode(
        string phoneProvince, string phoneCity, string phoneDistrict,
        string projectProvince, string projectCity, string projectDistrict)
    {
        bool Any(string? code) => !string.IsNullOrWhiteSpace(code) && long.TryParse(code!.Trim(), out var v) && v != 0;

        return Any(phoneProvince) || Any(phoneCity) || Any(phoneDistrict)
               || Any(projectProvince) || Any(projectCity) || Any(projectDistrict);
    }
}

/// <summary>
/// 公海创建客户命令，仅包含公海录入表单实际提交的字段，均为必填。
/// </summary>
public record CreateSeaCustomerCommand(
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
    string ConsultationContent,
    UserId CreatorId,
    string CreatorName) : ICommand<CustomerId>;

public class CreateSeaCustomerCommandValidator : AbstractValidator<CreateSeaCustomerCommand>
{
    public CreateSeaCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerSourceId).NotEmpty();
        RuleFor(c => c.CustomerSourceName).NotEmpty();
        RuleFor(c => c.MainContactName).NotEmpty();
        RuleFor(c => c.ConsultationContent).NotEmpty();
        RuleFor(c => c).Must(HaveAtLeastOneContactMethod).WithMessage("请至少填写一种联系方式（电话、QQ 或微信）");
        RuleFor(c => c).Must(HavePhoneOrProjectRegion).WithMessage("电话区域与项目区域请至少填写一侧（省/市/区任一级有效编码）");
        RuleFor(c => c.PhoneProvinceName).NotEmpty().When(c => !string.IsNullOrWhiteSpace(c.PhoneProvinceCode));
        RuleFor(c => c.ProvinceName).NotEmpty().When(c => !string.IsNullOrWhiteSpace(c.ProvinceCode));
    }

    private static bool HaveAtLeastOneContactMethod(CreateSeaCustomerCommand c) =>
        !string.IsNullOrWhiteSpace(c.MainContactPhone)
        || !string.IsNullOrWhiteSpace(c.ContactQq)
        || !string.IsNullOrWhiteSpace(c.ContactWechat);

    private static bool HavePhoneOrProjectRegion(CreateSeaCustomerCommand c) =>
        SeaCustomerRegionValidation.HasAnyRegionCode(
            c.PhoneProvinceCode, c.PhoneCityCode, c.PhoneDistrictCode,
            c.ProvinceCode, c.CityCode, c.DistrictCode);
}

public class CreateSeaCustomerCommandHandler(ICustomerRepository repository)
    : ICommandHandler<CreateSeaCustomerCommand, CustomerId>
{
    public async Task<CustomerId> Handle(CreateSeaCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer(
            request.CustomerSourceId,
            request.CustomerSourceName,
            request.MainContactName,
            request.MainContactPhone,
            request.ContactQq,
            request.ContactWechat,
            request.PhoneProvinceCode,
            request.PhoneCityCode,
            request.PhoneDistrictCode,
            request.PhoneProvinceName,
            request.PhoneCityName,
            request.PhoneDistrictName,
            request.ProvinceCode,
            request.CityCode,
            request.DistrictCode,
            request.ProvinceName,
            request.CityName,
            request.DistrictName,
            request.ConsultationContent,
            request.CreatorId,
            request.CreatorName);
        await repository.AddAsync(customer, cancellationToken);
        return customer.Id;
    }
}
