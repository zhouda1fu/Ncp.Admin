using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record ReleaseCustomerToSeaCommand(CustomerId Id) : ICommand<bool>;

public class ReleaseCustomerToSeaCommandValidator : AbstractValidator<ReleaseCustomerToSeaCommand>
{
    public ReleaseCustomerToSeaCommandValidator() => RuleFor(c => c.Id).NotEmpty();
}

public class ReleaseCustomerToSeaCommandHandler(ICustomerRepository repository) : ICommandHandler<ReleaseCustomerToSeaCommand, bool>
{
    public async Task<bool> Handle(ReleaseCustomerToSeaCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.ReleaseToSea();
        return true;
    }
}
