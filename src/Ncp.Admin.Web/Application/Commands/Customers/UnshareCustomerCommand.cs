using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record UnshareCustomerCommand(CustomerId CustomerId, IReadOnlyList<UserId> SharedToUserIds) : ICommand<bool>;

public class UnshareCustomerCommandValidator : AbstractValidator<UnshareCustomerCommand>
{
    public UnshareCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.SharedToUserIds).NotNull();
    }
}

public class UnshareCustomerCommandHandler(ICustomerRepository repository)
    : ICommandHandler<UnshareCustomerCommand, bool>
{
    public async Task<bool> Handle(UnshareCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetWithSharesAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.UnshareUsers(request.SharedToUserIds);
        return true;
    }
}

