using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Contract;

public record UpdateContractCommand(
    ContractId Id,
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? FileStorageKey = null,
    OrderId? OrderId = null,
    CustomerId? CustomerId = null,
    int? ContractType = null,
    int? IncomeExpenseType = null,
    DateTimeOffset? SignDate = null,
    string? Note = null,
    string? Description = null,
    Guid? DepartmentId = null,
    string? BusinessManager = null,
    string? ResponsibleProject = null,
    string? InputCustomer = null,
    bool? NextPaymentReminder = null,
    bool? ContractExpiryReminder = null,
    int? SingleDoubleProfit = null,
    string? InvoicingInformation = null,
    int? PaymentStatus = null,
    string? WarrantyPeriod = null,
    bool? IsInstallmentPayment = null,
    decimal? AccumulatedAmount = null) : ICommand<bool>;

public class UpdateContractCommandValidator : AbstractValidator<UpdateContractCommand>
{
    public UpdateContractCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PartyA).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PartyB).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Amount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.EndDate).GreaterThanOrEqualTo(c => c.StartDate);
        RuleFor(c => c.Note).MaximumLength(2000);
        RuleFor(c => c.Description).MaximumLength(8000);
    }
}

public class UpdateContractCommandHandler(
    IContractRepository repository,
    ContractTypeOptionQuery contractTypeOptionQuery,
    IncomeExpenseTypeOptionQuery incomeExpenseTypeOptionQuery) : ICommandHandler<UpdateContractCommand, bool>
{
    public async Task<bool> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);

        var contractType = request.ContractType ?? contract.ContractType;
        var incomeExpenseType = request.IncomeExpenseType ?? contract.IncomeExpenseType;
        var ctList = await contractTypeOptionQuery.GetListAsync(cancellationToken);
        var ieList = await incomeExpenseTypeOptionQuery.GetListAsync(cancellationToken);
        var contractTypeName = ctList.FirstOrDefault(x => x.TypeValue == contractType)?.Name ?? string.Empty;
        var incomeExpenseTypeName = ieList.FirstOrDefault(x => x.TypeValue == incomeExpenseType)?.Name ?? string.Empty;

        contract.Update(
            request.Code, request.Title, request.PartyA, request.PartyB,
            request.Amount, request.StartDate, request.EndDate,
            request.FileStorageKey ?? contract.FileStorageKey,
            request.OrderId ?? contract.OrderId,
            request.CustomerId ?? contract.CustomerId,
            contractType, contractTypeName, incomeExpenseType, incomeExpenseTypeName,
            request.SignDate ?? contract.SignDate,
            request.Note ?? contract.Note, request.Description ?? contract.Description,
            request.DepartmentId ?? contract.DepartmentId,
            request.BusinessManager ?? contract.BusinessManager,
            request.ResponsibleProject ?? contract.ResponsibleProject,
            request.InputCustomer ?? contract.InputCustomer,
            request.NextPaymentReminder ?? contract.NextPaymentReminder,
            request.ContractExpiryReminder ?? contract.ContractExpiryReminder,
            request.SingleDoubleProfit ?? contract.SingleDoubleProfit,
            request.InvoicingInformation ?? contract.InvoicingInformation,
            request.PaymentStatus ?? contract.PaymentStatus,
            request.WarrantyPeriod ?? contract.WarrantyPeriod,
            request.IsInstallmentPayment ?? contract.IsInstallmentPayment,
            request.AccumulatedAmount ?? contract.AccumulatedAmount);
        return true;
    }
}
