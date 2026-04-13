using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.ProductCategories;

/// <summary>
/// 删除产品分类命令
/// </summary>
public record DeleteProductCategoryCommand(ProductCategoryId Id) : ICommand<bool>;

public class DeleteProductCategoryCommandValidator : AbstractValidator<DeleteProductCategoryCommand>
{
    public DeleteProductCategoryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteProductCategoryCommandHandler(IProductCategoryRepository repository)
    : ICommandHandler<DeleteProductCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        _ = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品分类", ErrorCodes.ProductCategoryNotFound);
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
