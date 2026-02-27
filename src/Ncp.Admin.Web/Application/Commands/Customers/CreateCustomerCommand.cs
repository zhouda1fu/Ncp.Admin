using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record CreateCustomerCommand(
    UserId OwnerId,
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
    UserId CreatorId,
    string CreatorName,
    IReadOnlyList<IndustryId> IndustryIds) : ICommand<CustomerId>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ShortName).MaximumLength(100);
    }
}

public class CreateCustomerCommandHandler(ICustomerRepository repository) : ICommandHandler<CreateCustomerCommand, CustomerId>
{
    public async Task<CustomerId> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer(
            request.OwnerId, request.CustomerSourceId, request.CustomerSourceName, request.FullName,
            request.ShortName, request.Nature, request.ProvinceCode, request.CityCode, request.DistrictCode,
            request.ProvinceName, request.CityName, request.DistrictName,
            request.PhoneProvinceCode, request.PhoneCityCode, request.PhoneDistrictCode,
            request.PhoneProvinceName, request.PhoneCityName, request.PhoneDistrictName,
            request.ConsultationContent,
            request.CoverRegion, request.RegisterAddress, request.MainContactName, request.MainContactPhone,
            request.ContactQq, request.ContactWechat,
            request.WechatStatus, request.Remark, request.IsKeyAccount, request.CreatorId, request.CreatorName);
        await repository.AddAsync(customer, cancellationToken);
        if (request.IndustryIds is { Count: > 0 })
            customer.SetIndustries(request.IndustryIds);
        return customer.Id;
    }
}
