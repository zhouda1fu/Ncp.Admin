using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsMethodModule;

public record UpdateOrderLogisticsMethodCommand(
    OrderLogisticsMethodId Id,
    string Name,
    int TypeValue,
    int Sort) : ICommand<bool>;

public class UpdateOrderLogisticsMethodCommandValidator : AbstractValidator<UpdateOrderLogisticsMethodCommand>
{
    public UpdateOrderLogisticsMethodCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateOrderLogisticsMethodCommandHandler(IOrderLogisticsMethodRepository repository)
    : ICommandHandler<UpdateOrderLogisticsMethodCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderLogisticsMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到物流方式", ErrorCodes.OrderNotFound);
        entity.Update(request.Name, request.TypeValue, request.Sort);
        return true;
    }
}
