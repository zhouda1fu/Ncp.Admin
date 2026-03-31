using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.ProductCategoryModule;

/// <summary>
/// 更新产品分类命令
/// </summary>
public record UpdateProductCategoryCommand(
    ProductCategoryId Id,
    string Name,
    string Remark,
    ProductCategoryId ParentId,
    int SortOrder,
    bool Visible,
    bool IsDiscount)
    : ICommand<bool>;

public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Remark).NotNull().MaximumLength(500);
    }
}

public class UpdateProductCategoryCommandHandler(IProductCategoryRepository repository)
    : ICommandHandler<UpdateProductCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品分类", ErrorCodes.ProductCategoryNotFound);
        entity.Update(request.Name, request.Remark, request.ParentId, request.SortOrder, request.Visible, request.IsDiscount);
        return true;
    }
}
