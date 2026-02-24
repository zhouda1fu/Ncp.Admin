using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Expense;

/// <summary>
/// 报销明细输入项（用于创建报销单）
/// </summary>
public record ExpenseItemInput(ExpenseType Type, decimal Amount, string Description, string? InvoiceUrl);

/// <summary>
/// 创建报销单命令（草稿，含多条明细）
/// </summary>
public record CreateExpenseClaimCommand(UserId ApplicantId, string ApplicantName, List<ExpenseItemInput> Items) : ICommand<ExpenseClaimId>;

/// <summary>
/// 创建报销单命令验证器
/// </summary>
public class CreateExpenseClaimCommandValidator : AbstractValidator<CreateExpenseClaimCommand>
{
    /// <inheritdoc />
    public CreateExpenseClaimCommandValidator()
    {
        RuleFor(c => c.ApplicantId).NotNull();
        RuleFor(c => c.ApplicantName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Items).NotEmpty().WithMessage("请至少添加一条报销明细");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Amount).GreaterThan(0);
            item.RuleFor(x => x.Description).MaximumLength(500);
        });
    }
}

/// <summary>
/// 创建报销单命令处理器
/// </summary>
public class CreateExpenseClaimCommandHandler(IExpenseClaimRepository repository) : ICommandHandler<CreateExpenseClaimCommand, ExpenseClaimId>
{
    /// <inheritdoc />
    public async Task<ExpenseClaimId> Handle(CreateExpenseClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = new ExpenseClaim(request.ApplicantId, request.ApplicantName);
        foreach (var item in request.Items)
            claim.AddItem(item.Type, item.Amount, item.Description, item.InvoiceUrl);
        await repository.AddAsync(claim, cancellationToken);
        return claim.Id;
    }
}
