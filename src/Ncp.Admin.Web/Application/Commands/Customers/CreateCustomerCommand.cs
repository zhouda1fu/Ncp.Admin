using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record CreateCustomerCommand(
    UserId OwnerId,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    string FullName,
    string ShortName,
    CustomerStatus Status,
    CompanyNature Nature,
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
    int EmployeeCount,
    string BusinessLicense,
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
        RuleFor(c => c.Status).IsInEnum().WithMessage("客户状态必填且有效");
        RuleFor(c => c.Nature).IsInEnum().WithMessage("公司性质必填且有效");
    }
}

public class CreateCustomerCommandHandler(ICustomerRepository repository, UserQuery userQuery)
    : ICommandHandler<CreateCustomerCommand, CustomerId>
{
    public async Task<CustomerId> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // 冗余负责人部门信息：从用户侧读取 DeptId/DeptName，写入 Customer 聚合便于展示与数据权限过滤
        var user = await userQuery.GetUserByIdAsync(request.OwnerId, cancellationToken);
        var deptId = user.DeptId != new DeptId(0) ? user.DeptId : new DeptId(0);
        var deptName = user.DeptName;

        var customer = new Customer(
            request.OwnerId, deptId, deptName, request.CustomerSourceId, request.CustomerSourceName, request.FullName,
            request.ShortName, request.Status, request.Nature, request.ProvinceCode, request.CityCode, request.DistrictCode,
            request.ProvinceName, request.CityName, request.DistrictName,
            request.PhoneProvinceCode, request.PhoneCityCode, request.PhoneDistrictCode,
            request.PhoneProvinceName, request.PhoneCityName, request.PhoneDistrictName,
            request.ConsultationContent,
            request.CoverRegion, request.RegisterAddress, request.EmployeeCount, request.BusinessLicense ?? string.Empty, request.MainContactName, request.MainContactPhone,
            request.ContactQq, request.ContactWechat,
            request.WechatStatus, request.Remark, request.IsKeyAccount, request.CreatorId, request.CreatorName,
            request.IndustryIds);
        await repository.AddAsync(customer, cancellationToken);
        return customer.Id;
    }
}
