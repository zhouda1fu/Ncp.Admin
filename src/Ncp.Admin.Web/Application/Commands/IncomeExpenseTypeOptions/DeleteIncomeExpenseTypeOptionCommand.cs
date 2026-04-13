using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.IncomeExpenseTypeOptions;

public record DeleteIncomeExpenseTypeOptionCommand(IncomeExpenseTypeOptionId Id) : ICommand<bool>;

public class DeleteIncomeExpenseTypeOptionCommandValidator : AbstractValidator<DeleteIncomeExpenseTypeOptionCommand>
{
    public DeleteIncomeExpenseTypeOptionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
    }
}

public class DeleteIncomeExpenseTypeOptionCommandHandler(IIncomeExpenseTypeOptionRepository repository)
    : ICommandHandler<DeleteIncomeExpenseTypeOptionCommand, bool>
{
    public async Task<bool> Handle(DeleteIncomeExpenseTypeOptionCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
