using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderInvoiceTypeOptions;

public record CreateOrderInvoiceTypeOptionCommand(
    string Name,
    int TypeValue,
    int SortOrder) : ICommand<OrderInvoiceTypeOptionId>;

public class CreateOrderInvoiceTypeOptionCommandValidator : AbstractValidator<CreateOrderInvoiceTypeOptionCommand>
{
    public CreateOrderInvoiceTypeOptionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateOrderInvoiceTypeOptionCommandHandler(IOrderInvoiceTypeOptionRepository repository)
    : ICommandHandler<CreateOrderInvoiceTypeOptionCommand, OrderInvoiceTypeOptionId>
{
    public async Task<OrderInvoiceTypeOptionId> Handle(CreateOrderInvoiceTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = new OrderInvoiceTypeOption(request.Name, request.TypeValue, request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
