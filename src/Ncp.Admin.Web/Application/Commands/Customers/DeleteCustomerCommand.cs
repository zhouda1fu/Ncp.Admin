using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record DeleteCustomerCommand(CustomerId Id) : ICommand<bool>;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteCustomerCommandHandler(ICustomerRepository repository) : ICommandHandler<DeleteCustomerCommand, bool>
{
    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        if (!customer.IsInSea)
            throw new KnownException("仅公海客户可删除", ErrorCodes.CustomerNotInSea);
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
