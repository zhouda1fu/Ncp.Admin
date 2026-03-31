using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsMethodModule;

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

public class CreateOrderLogisticsMethodCommandHandler(
    IOrderLogisticsMethodRepository repository,
    ApplicationDbContext dbContext)
    : ICommandHandler<CreateOrderLogisticsMethodCommand, OrderLogisticsMethodId>
{
    public async Task<OrderLogisticsMethodId> Handle(CreateOrderLogisticsMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = OrderLogisticsMethod.Create(request.Name, request.TypeValue, request.Sort);
        await repository.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
