using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record AddCustomerContactRecordCommand(
    CustomerId CustomerId,
    DateTimeOffset RecordAt,
    CustomerContactRecordType RecordType,
    string Title,
    string Content,
    DateTimeOffset? NextVisitAt,
    CustomerContactRecordStatus Status,
    IReadOnlyList<CustomerContactId>? CustomerContactIds,
    UserId OwnerId,
    UserId CreatorId,
    string Remark,
    int ReminderIntervalDays,
    int ReminderCount,
    string FilePath,
    string CustomerAddress,
    string VisitAddress) : ICommand<CustomerContactRecordId>;

public class AddCustomerContactRecordCommandValidator : AbstractValidator<AddCustomerContactRecordCommand>
{
    public AddCustomerContactRecordCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.OwnerId).NotEmpty();
        RuleFor(c => c.CreatorId).NotEmpty();
        RuleFor(c => c.Title).MaximumLength(200);
        RuleFor(c => c.Remark).MaximumLength(2000);
        RuleFor(c => c.FilePath).MaximumLength(1000);
        RuleFor(c => c.CustomerAddress).MaximumLength(500);
        RuleFor(c => c.VisitAddress).MaximumLength(500);
        RuleFor(c => c.ReminderIntervalDays)
            .Must(CustomerContactRecordReminderInterval.IsValid)
            .WithMessage("提醒间隔仅支持 1、2、3、10、15、20、30、50、80、100 天");
        RuleFor(c => c.ReminderCount).GreaterThanOrEqualTo(1);
    }
}

public class AddCustomerContactRecordCommandHandler(
    ICustomerRepository customerRepository,
    ICustomerContactRecordRepository recordRepository,
    UserQuery userQuery)
    : ICommandHandler<AddCustomerContactRecordCommand, CustomerContactRecordId>
{
    public async Task<CustomerContactRecordId> Handle(AddCustomerContactRecordCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetWithContactsAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.EnsureCanMaintainContactRecords();
        customer.EnsureContactIdsBelongToThisCustomer(request.CustomerContactIds);
        var owner = await userQuery.GetUserByIdAsync(request.OwnerId, cancellationToken);
        var record = new CustomerContactRecord(
            request.CustomerId,
            request.RecordAt,
            request.RecordType,
            request.Title ?? string.Empty,
            request.Content ?? string.Empty,
            request.NextVisitAt,
            request.Status,
            request.OwnerId,
            owner.RealName,
            owner.DeptId,
            owner.DeptName,
            request.CreatorId,
            request.Remark ?? string.Empty,
            request.ReminderIntervalDays,
            request.ReminderCount,
            request.FilePath ?? string.Empty,
            request.CustomerAddress ?? string.Empty,
            request.VisitAddress ?? string.Empty,
            request.CustomerContactIds);
        await recordRepository.AddAsync(record, cancellationToken);
        return record.Id;
    }
}
