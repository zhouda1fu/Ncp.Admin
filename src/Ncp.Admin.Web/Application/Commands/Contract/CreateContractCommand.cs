using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using ContractEntity = Ncp.Admin.Domain.AggregatesModel.ContractAggregate.Contract;

namespace Ncp.Admin.Web.Application.Commands.Contract;

public record CreateContractCommand(
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    UserId CreatorId,
    string FileStorageKey,
    OrderId OrderId,
    CustomerId CustomerId,
    int ContractType,
    int IncomeExpenseType,
    DateTimeOffset SignDate,
    string Note,
    string Description,
    Guid DepartmentId,
    string BusinessManager,
    string ResponsibleProject,
    string InputCustomer,
    bool NextPaymentReminder,
    bool ContractExpiryReminder,
    int SingleDoubleSeal,
    string InvoicingInformation,
    int PaymentStatus,
    string WarrantyPeriod,
    bool IsInstallmentPayment,
    decimal AccumulatedAmount) : ICommand<ContractId>;

public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
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

public class CreateContractCommandHandler(
    IContractRepository repository,
    ContractTypeOptionQuery contractTypeOptionQuery,
    IncomeExpenseTypeOptionQuery incomeExpenseTypeOptionQuery) : ICommandHandler<CreateContractCommand, ContractId>
{
    public async Task<ContractId> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var ctList = await contractTypeOptionQuery.GetListAsync(cancellationToken);
        var ieList = await incomeExpenseTypeOptionQuery.GetListAsync(cancellationToken);
        var contractTypeName = ctList.FirstOrDefault(x => x.TypeValue == request.ContractType)?.Name ?? string.Empty;
        var incomeExpenseTypeName = ieList.FirstOrDefault(x => x.TypeValue == request.IncomeExpenseType)?.Name ?? string.Empty;

        var contract = new ContractEntity(
            request.Code, request.Title, request.PartyA, request.PartyB,
            request.Amount, request.StartDate, request.EndDate, request.CreatorId, request.FileStorageKey,
            request.OrderId, request.CustomerId, request.ContractType, contractTypeName,
            request.IncomeExpenseType, incomeExpenseTypeName,
            request.SignDate, request.Note, request.Description,
            request.DepartmentId, request.BusinessManager, request.ResponsibleProject, request.InputCustomer,
            request.NextPaymentReminder, request.ContractExpiryReminder, request.SingleDoubleSeal,
            request.InvoicingInformation, request.PaymentStatus, request.WarrantyPeriod,
            request.IsInstallmentPayment, request.AccumulatedAmount);
        await repository.AddAsync(contract, cancellationToken);
        return contract.Id;
    }
}
