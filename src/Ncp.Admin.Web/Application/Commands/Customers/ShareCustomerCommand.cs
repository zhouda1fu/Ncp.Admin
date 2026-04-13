using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record ShareCustomerCommand(CustomerId CustomerId, UserId SharedByUserId, IReadOnlyList<UserId> SharedToUserIds)
    : ICommand<bool>;

public class ShareCustomerCommandValidator : AbstractValidator<ShareCustomerCommand>
{
    public ShareCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.SharedByUserId).NotEmpty();
        RuleFor(x => x.SharedToUserIds).NotNull();
    }
}

public class ShareCustomerCommandHandler(ICustomerRepository repository)
    : ICommandHandler<ShareCustomerCommand, bool>
{
    public async Task<bool> Handle(ShareCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetWithSharesAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.ShareToUsers(request.SharedByUserId, request.SharedToUserIds);
        return true;
    }
}

