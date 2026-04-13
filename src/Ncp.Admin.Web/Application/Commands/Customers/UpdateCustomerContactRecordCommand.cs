using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record UpdateCustomerContactRecordCommand(
    CustomerId CustomerId,
    CustomerContactRecordId RecordId,
    DateTimeOffset RecordAt,
    CustomerContactRecordType RecordType,
    string Title,
    string Content,
    DateTimeOffset? NextVisitAt,
    CustomerContactRecordStatus Status,
    IReadOnlyList<CustomerContactId>? CustomerContactIds,
    UserId OwnerId,
    UserId ModifierId,
    string Remark,
    int ReminderIntervalDays,
    int ReminderCount,
    string FilePath,
    string CustomerAddress,
    string VisitAddress) : ICommand<bool>;

public class UpdateCustomerContactRecordCommandValidator : AbstractValidator<UpdateCustomerContactRecordCommand>
{
    public UpdateCustomerContactRecordCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.RecordId).NotEmpty();
        RuleFor(c => c.OwnerId).NotEmpty();
        RuleFor(c => c.ModifierId).NotEmpty();
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

public class UpdateCustomerContactRecordCommandHandler(
    ICustomerRepository customerRepository,
    ICustomerContactRecordRepository recordRepository,
    UserQuery userQuery)
    : ICommandHandler<UpdateCustomerContactRecordCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomerContactRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await recordRepository.GetWithContactLinksAsync(request.RecordId, cancellationToken)
            ?? throw new KnownException("未找到客户联系记录", ErrorCodes.CustomerContactRecordNotFound);
        if (record.CustomerId != request.CustomerId)
            throw new KnownException("联系记录与所属客户不匹配", ErrorCodes.CustomerContactRecordNotFound);
        var customer = await customerRepository.GetWithContactsAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.EnsureCanMaintainContactRecords();
        customer.EnsureContactIdsBelongToThisCustomer(request.CustomerContactIds);
        var owner = await userQuery.GetUserByIdAsync(request.OwnerId, cancellationToken);
        record.Update(
            request.RecordAt,
            request.RecordType,
            request.Title ?? string.Empty,
            request.Content ?? string.Empty,
            request.NextVisitAt,
            request.Status,
            request.CustomerContactIds,
            request.OwnerId,
            owner.RealName,
            owner.DeptId,
            owner.DeptName,
            request.ModifierId,
            request.Remark ?? string.Empty,
            request.ReminderIntervalDays,
            request.ReminderCount,
            request.FilePath ?? string.Empty,
            request.CustomerAddress ?? string.Empty,
            request.VisitAddress ?? string.Empty);
        return true;
    }
}
