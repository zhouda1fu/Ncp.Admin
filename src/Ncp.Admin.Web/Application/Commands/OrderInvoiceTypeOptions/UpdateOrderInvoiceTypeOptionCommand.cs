using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderInvoiceTypeOptions;

public record UpdateOrderInvoiceTypeOptionCommand(
    OrderInvoiceTypeOptionId Id,
    string Name,
    int TypeValue,
    int SortOrder) : ICommand<bool>;

public class UpdateOrderInvoiceTypeOptionCommandValidator : AbstractValidator<UpdateOrderInvoiceTypeOptionCommand>
{
    public UpdateOrderInvoiceTypeOptionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateOrderInvoiceTypeOptionCommandHandler(IOrderInvoiceTypeOptionRepository repository)
    : ICommandHandler<UpdateOrderInvoiceTypeOptionCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderInvoiceTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到订单发票类型选项", ErrorCodes.OrderInvoiceTypeOptionNotFound);
        entity.Update(request.Name, request.TypeValue, request.SortOrder);
        return true;
    }
}
