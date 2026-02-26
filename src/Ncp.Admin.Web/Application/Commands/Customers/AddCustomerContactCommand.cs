using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record AddCustomerContactCommand(
    CustomerId CustomerId,
    string Name,
    string? ContactType,
    int? Gender,
    DateTime? Birthday,
    string? Position,
    string? Mobile,
    string? Phone,
    string? Email,
    bool IsPrimary) : ICommand<CustomerContactId>;

public class AddCustomerContactCommandValidator : AbstractValidator<AddCustomerContactCommand>
{
    public AddCustomerContactCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
    }
}

public class AddCustomerContactCommandHandler(ICustomerRepository repository) : ICommandHandler<AddCustomerContactCommand, CustomerContactId>
{
    public async Task<CustomerContactId> Handle(AddCustomerContactCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        var id = customer.AddContact(
            request.Name, request.ContactType ?? string.Empty, request.Gender, request.Birthday,
            request.Position ?? string.Empty, request.Mobile ?? string.Empty, request.Phone ?? string.Empty, request.Email ?? string.Empty, request.IsPrimary);
        return id;
    }
}
