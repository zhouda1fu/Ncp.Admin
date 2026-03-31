using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProductCategoryModule;

/// <summary>
/// 创建产品分类命令
/// </summary>
public record CreateProductCategoryCommand(
    string Name,
    string Remark,
    ProductCategoryId ParentId,
    int SortOrder = 0,
    bool Visible = true,
    bool IsDiscount = false)
    : ICommand<ProductCategoryId>;

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Remark).NotNull().MaximumLength(500);
    }
}

public class CreateProductCategoryCommandHandler(IProductCategoryRepository repository)
    : ICommandHandler<CreateProductCategoryCommand, ProductCategoryId>
{
    public async Task<ProductCategoryId> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProductCategory(
            request.Name,
            request.Remark,
            request.ParentId,
            request.SortOrder,
            request.Visible,
            request.IsDiscount);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
