using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.CustomerSource;

public record UpdateCustomerSourceCommand(CustomerSourceId Id, string Name, int SortOrder) : ICommand<bool>;

public class UpdateCustomerSourceCommandValidator : AbstractValidator<UpdateCustomerSourceCommand>
{
    public UpdateCustomerSourceCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateCustomerSourceCommandHandler(ICustomerSourceRepository repository) : ICommandHandler<UpdateCustomerSourceCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomerSourceCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户来源", ErrorCodes.CustomerSourceNotFound);
        entity.Update(request.Name, request.SortOrder);
        return true;
    }
}
