using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record RemoveCustomerContactRecordCommand(
    CustomerId CustomerId,
    CustomerContactRecordId RecordId) : ICommand<bool>;

public class RemoveCustomerContactRecordCommandValidator : AbstractValidator<RemoveCustomerContactRecordCommand>
{
    public RemoveCustomerContactRecordCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.RecordId).NotEmpty();
    }
}

public class RemoveCustomerContactRecordCommandHandler(ICustomerRepository repository) : ICommandHandler<RemoveCustomerContactRecordCommand, bool>
{
    public async Task<bool> Handle(RemoveCustomerContactRecordCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.RemoveContactRecord(request.RecordId);
        return true;
    }
}
