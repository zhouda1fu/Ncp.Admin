using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.IncomeExpenseTypeOption;

public record UpdateIncomeExpenseTypeOptionCommand(IncomeExpenseTypeOptionId Id, string Name, int TypeValue, int SortOrder) : ICommand<bool>;

public class UpdateIncomeExpenseTypeOptionCommandValidator : AbstractValidator<UpdateIncomeExpenseTypeOptionCommand>
{
    public UpdateIncomeExpenseTypeOptionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateIncomeExpenseTypeOptionCommandHandler(IIncomeExpenseTypeOptionRepository repository)
    : ICommandHandler<UpdateIncomeExpenseTypeOptionCommand, bool>
{
    public async Task<bool> Handle(UpdateIncomeExpenseTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到收支类型选项", ErrorCodes.IncomeExpenseTypeOptionNotFound);
        entity.Update(request.Name, request.TypeValue, request.SortOrder);
        return true;
    }
}
