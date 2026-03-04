using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

public record DeleteOrderCommand(OrderId Id) : ICommand<bool>;

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteOrderCommandHandler(IOrderRepository repository) : ICommandHandler<DeleteOrderCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);
        if (order.IsDeleted)
            return true;
        order.MarkDeleted();
        return true;
    }
}
