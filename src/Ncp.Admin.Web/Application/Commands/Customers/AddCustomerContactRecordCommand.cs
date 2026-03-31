using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record AddCustomerContactRecordCommand(
    CustomerId CustomerId,
    DateTimeOffset RecordAt,
    string RecordType,
    string Content,
    UserId? RecorderId,
    string RecorderName) : ICommand<CustomerContactRecordId>;

public class AddCustomerContactRecordCommandValidator : AbstractValidator<AddCustomerContactRecordCommand>
{
    public AddCustomerContactRecordCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.RecordType).NotEmpty().MaximumLength(50);
    }
}

public class AddCustomerContactRecordCommandHandler(ICustomerRepository repository) : ICommandHandler<AddCustomerContactRecordCommand, CustomerContactRecordId>
{
    public async Task<CustomerContactRecordId> Handle(AddCustomerContactRecordCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        var id = customer.AddContactRecord(
            request.RecordAt,
            request.RecordType,
            request.Content ?? string.Empty,
            request.RecorderId ?? new UserId(0),
            request.RecorderName ?? string.Empty);
        return id;
    }
}
