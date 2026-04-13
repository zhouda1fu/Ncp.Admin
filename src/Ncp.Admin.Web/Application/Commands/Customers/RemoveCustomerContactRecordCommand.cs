using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record RemoveCustomerContactRecordCommand(
    CustomerId CustomerId,
    CustomerContactRecordId RecordId,
    UserId DeleterId) : ICommand<bool>;

public class RemoveCustomerContactRecordCommandValidator : AbstractValidator<RemoveCustomerContactRecordCommand>
{
    public RemoveCustomerContactRecordCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.RecordId).NotEmpty();
    }
}

public class RemoveCustomerContactRecordCommandHandler(
    ICustomerRepository customerRepository,
    ICustomerContactRecordRepository recordRepository)
    : ICommandHandler<RemoveCustomerContactRecordCommand, bool>
{
    public async Task<bool> Handle(RemoveCustomerContactRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await recordRepository.GetAsync(request.RecordId, cancellationToken)
            ?? throw new KnownException("未找到客户联系记录", ErrorCodes.CustomerContactRecordNotFound);
        if (record.CustomerId != request.CustomerId)
            throw new KnownException("联系记录与所属客户不匹配", ErrorCodes.CustomerContactRecordNotFound);
        var customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.EnsureCanMaintainContactRecords();
        record.SoftDelete(request.DeleterId);
        return true;
    }
}
