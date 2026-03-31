using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderInvoiceTypeOptionModule;

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

public class CreateOrderInvoiceTypeOptionCommandHandler(
    IOrderInvoiceTypeOptionRepository repository,
    ApplicationDbContext dbContext)
    : ICommandHandler<CreateOrderInvoiceTypeOptionCommand, OrderInvoiceTypeOptionId>
{
    public async Task<OrderInvoiceTypeOptionId> Handle(CreateOrderInvoiceTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = new OrderInvoiceTypeOption(request.Name, request.TypeValue, request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
