using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.IncomeExpenseTypeOptions;

public record CreateIncomeExpenseTypeOptionCommand(string Name, int TypeValue, int SortOrder = 0) : ICommand<IncomeExpenseTypeOptionId>;

public class CreateIncomeExpenseTypeOptionCommandValidator : AbstractValidator<CreateIncomeExpenseTypeOptionCommand>
{
    public CreateIncomeExpenseTypeOptionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateIncomeExpenseTypeOptionCommandHandler(IIncomeExpenseTypeOptionRepository repository)
    : ICommandHandler<CreateIncomeExpenseTypeOptionCommand, IncomeExpenseTypeOptionId>
{
    public async Task<IncomeExpenseTypeOptionId> Handle(CreateIncomeExpenseTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = new IncomeExpenseTypeOption(
            request.Name, request.TypeValue, request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
