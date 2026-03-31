using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContractModule;

/// <summary>
/// 更新合同发票命令
/// </summary>
public record UpdateContractInvoiceCommand(
    ContractId ContractId,
    ContractInvoiceId InvoiceId,
    InvoiceType Type,
    string InvoiceNumber,
    decimal TaxRate,
    decimal AmountExclTax,
    string Source,
    bool Status,
    string Title,
    decimal TaxAmount,
    decimal InvoicedAmount,
    string Handler,
    DateTimeOffset BillingDate,
    string Remarks,
    string AttachmentStorageKey) : ICommand<bool>;

public class UpdateContractInvoiceCommandValidator : AbstractValidator<UpdateContractInvoiceCommand>
{
    public UpdateContractInvoiceCommandValidator()
    {
        RuleFor(c => c.InvoiceNumber).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Source).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.AmountExclTax).GreaterThanOrEqualTo(0);
        RuleFor(c => c.TaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.InvoicedAmount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Handler).MaximumLength(100);
        RuleFor(c => c.Remarks).MaximumLength(2000);
        RuleFor(c => c.AttachmentStorageKey).MaximumLength(500);
    }
}

public class UpdateContractInvoiceCommandHandler(IContractRepository repository)
    : ICommandHandler<UpdateContractInvoiceCommand, bool>
{
    public async Task<bool> Handle(UpdateContractInvoiceCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.ContractId, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);
        contract.UpdateInvoice(
            request.InvoiceId, request.Type, request.InvoiceNumber, request.TaxRate, request.AmountExclTax, request.Source,
            request.Status, request.Title, request.TaxAmount, request.InvoicedAmount, request.Handler,
            request.BillingDate, request.Remarks, request.AttachmentStorageKey);
        await repository.UpdateAsync(contract, cancellationToken);
        return true;
    }
}
