using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record RemoveCustomerContactCommand(CustomerId CustomerId, CustomerContactId ContactId) : ICommand<bool>;

public class RemoveCustomerContactCommandValidator : AbstractValidator<RemoveCustomerContactCommand>
{
    public RemoveCustomerContactCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.ContactId).NotEmpty();
    }
}

public class RemoveCustomerContactCommandHandler(ICustomerRepository repository) : ICommandHandler<RemoveCustomerContactCommand, bool>
{
    public async Task<bool> Handle(RemoveCustomerContactCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.RemoveContact(request.ContactId);
        return true;
    }
}
