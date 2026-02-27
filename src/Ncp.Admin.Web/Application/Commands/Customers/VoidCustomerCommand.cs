using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record VoidCustomerCommand(CustomerId Id) : ICommand<bool>;

public class VoidCustomerCommandValidator : AbstractValidator<VoidCustomerCommand>
{
    public VoidCustomerCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class VoidCustomerCommandHandler(ICustomerRepository repository) : ICommandHandler<VoidCustomerCommand, bool>
{
    public async Task<bool> Handle(VoidCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.Void();
        return true;
    }
}
