using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanyModule;

public record UpdateOrderLogisticsCompanyCommand(
    OrderLogisticsCompanyId Id,
    string Name,
    int TypeValue,
    int Sort) : ICommand<bool>;

public class UpdateOrderLogisticsCompanyCommandValidator : AbstractValidator<UpdateOrderLogisticsCompanyCommand>
{
    public UpdateOrderLogisticsCompanyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateOrderLogisticsCompanyCommandHandler(IOrderLogisticsCompanyRepository repository)
    : ICommandHandler<UpdateOrderLogisticsCompanyCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderLogisticsCompanyCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到物流公司", ErrorCodes.OrderNotFound);
        entity.Update(request.Name, request.TypeValue, request.Sort);
        return true;
    }
}
