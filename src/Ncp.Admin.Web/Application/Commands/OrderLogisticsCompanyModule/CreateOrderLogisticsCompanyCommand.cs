using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanyModule;

public record CreateOrderLogisticsCompanyCommand(
    string Name,
    int TypeValue,
    int Sort) : ICommand<OrderLogisticsCompanyId>;

public class CreateOrderLogisticsCompanyCommandValidator : AbstractValidator<CreateOrderLogisticsCompanyCommand>
{
    public CreateOrderLogisticsCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateOrderLogisticsCompanyCommandHandler(
    IOrderLogisticsCompanyRepository repository,
    ApplicationDbContext dbContext)
    : ICommandHandler<CreateOrderLogisticsCompanyCommand, OrderLogisticsCompanyId>
{
    public async Task<OrderLogisticsCompanyId> Handle(CreateOrderLogisticsCompanyCommand request, CancellationToken cancellationToken)
    {
        var entity = OrderLogisticsCompany.Create(request.Name, request.TypeValue, request.Sort);
        await repository.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
