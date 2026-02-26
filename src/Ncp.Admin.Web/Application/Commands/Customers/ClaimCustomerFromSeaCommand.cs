using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record ClaimCustomerFromSeaCommand(CustomerId Id, UserId OwnerId, DeptId? DeptId) : ICommand<bool>;

public class ClaimCustomerFromSeaCommandValidator : AbstractValidator<ClaimCustomerFromSeaCommand>
{
    public ClaimCustomerFromSeaCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.OwnerId).NotEmpty();
    }
}

public class ClaimCustomerFromSeaCommandHandler(ICustomerRepository repository) : ICommandHandler<ClaimCustomerFromSeaCommand, bool>
{
    public async Task<bool> Handle(ClaimCustomerFromSeaCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.ClaimFromSea(request.OwnerId, request.DeptId);
        return true;
    }
}
