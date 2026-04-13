using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.ProductTypes;

/// <summary>
/// 删除产品类型命令
/// </summary>
public record DeleteProductTypeCommand(ProductTypeId Id) : ICommand<bool>;

public class DeleteProductTypeCommandValidator : AbstractValidator<DeleteProductTypeCommand>
{
    public DeleteProductTypeCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteProductTypeCommandHandler(IProductTypeRepository repository)
    : ICommandHandler<DeleteProductTypeCommand, bool>
{
    public async Task<bool> Handle(DeleteProductTypeCommand request, CancellationToken cancellationToken)
    {
        _ = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品类型", ErrorCodes.ProductTypeNotFound);
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
