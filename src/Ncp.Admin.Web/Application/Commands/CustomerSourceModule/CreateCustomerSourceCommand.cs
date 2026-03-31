using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.CustomerSourceModule;

public record CreateCustomerSourceCommand(string Name, int SortOrder = 0, CustomerSourceUsageScene UsageScene = CustomerSourceUsageScene.Both) : ICommand<CustomerSourceId>;

public class CreateCustomerSourceCommandValidator : AbstractValidator<CreateCustomerSourceCommand>
{
    public CreateCustomerSourceCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateCustomerSourceCommandHandler(ICustomerSourceRepository repository) : ICommandHandler<CreateCustomerSourceCommand, CustomerSourceId>
{
    public async Task<CustomerSourceId> Handle(CreateCustomerSourceCommand request, CancellationToken cancellationToken)
    {
        var entity = new CustomerSource(request.Name, request.SortOrder, request.UsageScene);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
