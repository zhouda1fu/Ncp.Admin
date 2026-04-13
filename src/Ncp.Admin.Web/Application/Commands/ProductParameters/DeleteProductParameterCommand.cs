using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProductParameters;

/// <summary>
/// 删除产品参数命令
/// </summary>
/// <param name="Id">参数 ID</param>
public record DeleteProductParameterCommand(ProductParameterId Id) : ICommand<bool>;

public class DeleteProductParameterCommandValidator : AbstractValidator<DeleteProductParameterCommand>
{
    public DeleteProductParameterCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteProductParameterCommandHandler(IProductParameterRepository repository)
    : ICommandHandler<DeleteProductParameterCommand, bool>
{
    public async Task<bool> Handle(DeleteProductParameterCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
