using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 公海客户更新命令。简称、主联系人、微信状态、是否重点客户不从请求接收，由 Handler 使用当前客户实体值，防止篡改。
/// </summary>
public record UpdateSeaCustomerCommand(
    CustomerId Id,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    CustomerStatus? Status,
    CompanyNature? Nature,
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
    string CoverRegion,
    string RegisterAddress,
    int EmployeeCount,
    string BusinessLicense,
    string ContactQq,
    string ContactWechat,
    string Remark,
    IReadOnlyList<IndustryId> IndustryIds) : ICommand<bool>;

public class UpdateSeaCustomerCommandValidator : AbstractValidator<UpdateSeaCustomerCommand>
{
    public UpdateSeaCustomerCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c).Must(HavePhoneOrProjectRegion).WithMessage("电话区域与项目区域请至少填写一侧（省/市/区任一级有效编码）");
    }

    private static bool HavePhoneOrProjectRegion(UpdateSeaCustomerCommand c) =>
        SeaCustomerRegionValidation.HasAnyRegionCode(
            c.PhoneProvinceCode, c.PhoneCityCode, c.PhoneDistrictCode,
            c.ProvinceCode, c.CityCode, c.DistrictCode);
}

public class UpdateSeaCustomerCommandHandler(ICustomerRepository repository)
    : ICommandHandler<UpdateSeaCustomerCommand, bool>
{
    public async Task<bool> Handle(UpdateSeaCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetWithIndustriesAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        if (string.IsNullOrWhiteSpace(customer.MainContactPhone)
            && string.IsNullOrWhiteSpace(request.ContactQq)
            && string.IsNullOrWhiteSpace(request.ContactWechat))
            throw new KnownException("请至少保留一种联系方式（电话、QQ 或微信）");
        customer.UpdateWhenInSea(
            request.CustomerSourceId, request.CustomerSourceName,
            customer.ShortName, request.Status, request.Nature, request.ProvinceCode,
            request.CityCode, request.DistrictCode,
            request.ProvinceName, request.CityName, request.DistrictName,
            request.PhoneProvinceCode, request.PhoneCityCode, request.PhoneDistrictCode,
            request.PhoneProvinceName, request.PhoneCityName, request.PhoneDistrictName,
            customer.ConsultationContent,
            request.CoverRegion, request.RegisterAddress, request.EmployeeCount, request.BusinessLicense ?? string.Empty,
            customer.MainContactName, customer.MainContactPhone,
            request.ContactQq, request.ContactWechat,
            customer.WechatStatus, request.Remark, customer.IsKeyAccount,
            request.IndustryIds);
        return true;
    }
}
