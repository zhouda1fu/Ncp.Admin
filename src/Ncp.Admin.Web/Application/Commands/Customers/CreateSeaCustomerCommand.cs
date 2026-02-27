using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

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
        RuleFor(c => c.MainContactPhone).NotEmpty();
        RuleFor(c => c.ContactQq).NotEmpty();
        RuleFor(c => c.ContactWechat).NotEmpty();
        RuleFor(c => c.PhoneProvinceCode).NotEmpty();
        //RuleFor(c => c.PhoneCityCode).NotEmpty();
        //RuleFor(c => c.PhoneDistrictCode).NotEmpty();
        RuleFor(c => c.PhoneProvinceName).NotEmpty();
        //RuleFor(c => c.PhoneCityName).NotEmpty();
        //RuleFor(c => c.PhoneDistrictName).NotEmpty();
        RuleFor(c => c.ProvinceCode).NotEmpty();
        //RuleFor(c => c.CityCode).NotEmpty();
        //RuleFor(c => c.DistrictCode).NotEmpty();
        RuleFor(c => c.ProvinceName).NotEmpty();
        //RuleFor(c => c.CityName).NotEmpty();
        //RuleFor(c => c.DistrictName).NotEmpty();
        RuleFor(c => c.ConsultationContent).NotEmpty();
    }
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
