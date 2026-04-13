using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsCompanyAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanies;

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

public class CreateOrderLogisticsCompanyCommandHandler(IOrderLogisticsCompanyRepository repository)
    : ICommandHandler<CreateOrderLogisticsCompanyCommand, OrderLogisticsCompanyId>
{
    public async Task<OrderLogisticsCompanyId> Handle(CreateOrderLogisticsCompanyCommand request, CancellationToken cancellationToken)
    {
        var entity = new OrderLogisticsCompany(request.Name, request.TypeValue, request.Sort);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
