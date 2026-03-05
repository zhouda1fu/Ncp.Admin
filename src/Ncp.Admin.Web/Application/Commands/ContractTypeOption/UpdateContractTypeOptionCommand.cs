using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContractTypeOption;

public record UpdateContractTypeOptionCommand(
    ContractTypeOptionId Id,
    string Name,
    int TypeValue,
    bool OrderSigningCompanyOptionDisplay,
    int SortOrder) : ICommand<bool>;

public class UpdateContractTypeOptionCommandValidator : AbstractValidator<UpdateContractTypeOptionCommand>
{
    public UpdateContractTypeOptionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateContractTypeOptionCommandHandler(IContractTypeOptionRepository repository)
    : ICommandHandler<UpdateContractTypeOptionCommand, bool>
{
    public async Task<bool> Handle(UpdateContractTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到合同类型选项", ErrorCodes.ContractTypeOptionNotFound);
        entity.Update(request.Name, request.TypeValue, request.OrderSigningCompanyOptionDisplay, request.SortOrder);
        return true;
    }
}
