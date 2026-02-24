using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Expense;

/// <summary>
/// 提交报销单命令（仅草稿可提交）
/// </summary>
public record SubmitExpenseClaimCommand(ExpenseClaimId ExpenseClaimId) : ICommand;

/// <summary>
/// 提交报销单命令验证器
/// </summary>
public class SubmitExpenseClaimCommandValidator : AbstractValidator<SubmitExpenseClaimCommand>
{
    /// <inheritdoc />
    public SubmitExpenseClaimCommandValidator()
    {
        RuleFor(c => c.ExpenseClaimId).NotNull();
    }
}

/// <summary>
/// 提交报销单命令处理器
/// </summary>
public class SubmitExpenseClaimCommandHandler(IExpenseClaimRepository repository) : ICommandHandler<SubmitExpenseClaimCommand>
{
    /// <inheritdoc />
    public async Task Handle(SubmitExpenseClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await repository.GetAsync(request.ExpenseClaimId, cancellationToken)
            ?? throw new KnownException("未找到报销单", ErrorCodes.ExpenseClaimNotFound);
        claim.Submit();
    }
}
