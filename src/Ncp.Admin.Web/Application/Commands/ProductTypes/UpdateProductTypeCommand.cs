using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.ProductTypes;

/// <summary>
/// 更新产品类型命令
/// </summary>
public record UpdateProductTypeCommand(ProductTypeId Id, string Name, int SortOrder, bool Visible)
    : ICommand<bool>;

public class UpdateProductTypeCommandValidator : AbstractValidator<UpdateProductTypeCommand>
{
    public UpdateProductTypeCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProductTypeCommandHandler(IProductTypeRepository repository)
    : ICommandHandler<UpdateProductTypeCommand, bool>
{
    public async Task<bool> Handle(UpdateProductTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品类型", ErrorCodes.ProductTypeNotFound);
        entity.Update(request.Name, request.SortOrder, request.Visible);
        return true;
    }
}
