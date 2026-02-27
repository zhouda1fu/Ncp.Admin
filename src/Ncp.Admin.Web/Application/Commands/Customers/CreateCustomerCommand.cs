using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record CreateCustomerCommand(
    UserId? OwnerId,
    DeptId? DeptId,
    CustomerSourceId CustomerSourceId,
    int StatusId,
    string FullName,
    string? ShortName,
    string? Nature,
    string? ProvinceCode,
    string? CityCode,
    string? DistrictCode,
    string? CoverRegion,
    string? RegisterAddress,
    string? MainContactName,
    string? MainContactPhone,
    string? WechatStatus,
    string? Remark,
    bool IsKeyAccount,
    UserId CreatorId,
    IReadOnlyList<IndustryId>? IndustryIds = null) : ICommand<CustomerId>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ShortName).MaximumLength(100);
    }
}

public class CreateCustomerCommandHandler(ICustomerRepository repository, ICustomerSourceRepository customerSourceRepository) : ICommandHandler<CreateCustomerCommand, CustomerId>
{
    public async Task<CustomerId> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var source = await customerSourceRepository.GetAsync(request.CustomerSourceId, cancellationToken)
            ?? throw new KnownException("未找到客户来源", ErrorCodes.CustomerSourceNotFound);
        var customer = new Customer(
            request.OwnerId, request.DeptId, request.CustomerSourceId, source.Name, request.StatusId, request.FullName,
            request.ShortName ?? string.Empty, request.Nature ?? string.Empty, request.ProvinceCode ?? string.Empty,
            request.CityCode ?? string.Empty, request.DistrictCode ?? string.Empty, request.CoverRegion ?? string.Empty,
            request.RegisterAddress ?? string.Empty, request.MainContactName ?? string.Empty, request.MainContactPhone ?? string.Empty,
            request.WechatStatus ?? string.Empty, request.Remark ?? string.Empty, request.IsKeyAccount, request.CreatorId);
        await repository.AddAsync(customer, cancellationToken);
        if (request.IndustryIds is { Count: > 0 })
            customer.SetIndustries(request.IndustryIds);
        return customer.Id;
    }
}
