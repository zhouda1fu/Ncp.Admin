using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record VoidCustomerCommand(CustomerId Id, UserId OperatorUserId) : ICommand<bool>;

public class VoidCustomerCommandValidator : AbstractValidator<VoidCustomerCommand>
{
    public VoidCustomerCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.OperatorUserId).NotEmpty();
    }
}

public class VoidCustomerCommandHandler(ICustomerRepository repository) : ICommandHandler<VoidCustomerCommand, bool>
{
    public async Task<bool> Handle(VoidCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        if (customer.IsInSea)
            customer.VoidUnclaimedSeaCustomer();
        else
            customer.VoidAfterSeaClaimByOwner(request.OperatorUserId);
        return true;
    }
}
