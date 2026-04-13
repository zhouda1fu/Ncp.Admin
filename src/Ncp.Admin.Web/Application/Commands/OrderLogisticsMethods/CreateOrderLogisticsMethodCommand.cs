using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsMethodAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsMethods;

public record CreateOrderLogisticsMethodCommand(
    string Name,
    int TypeValue,
    int Sort) : ICommand<OrderLogisticsMethodId>;

public class CreateOrderLogisticsMethodCommandValidator : AbstractValidator<CreateOrderLogisticsMethodCommand>
{
    public CreateOrderLogisticsMethodCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateOrderLogisticsMethodCommandHandler(IOrderLogisticsMethodRepository repository)
    : ICommandHandler<CreateOrderLogisticsMethodCommand, OrderLogisticsMethodId>
{
    public async Task<OrderLogisticsMethodId> Handle(CreateOrderLogisticsMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = new OrderLogisticsMethod(request.Name, request.TypeValue, request.Sort);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
