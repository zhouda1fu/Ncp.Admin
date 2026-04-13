using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProductTypes;

/// <summary>
/// 创建产品类型命令
/// </summary>
public record CreateProductTypeCommand(string Name, int SortOrder = 0, bool Visible = true)
    : ICommand<ProductTypeId>;

public class CreateProductTypeCommandValidator : AbstractValidator<CreateProductTypeCommand>
{
    public CreateProductTypeCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateProductTypeCommandHandler(IProductTypeRepository repository)
    : ICommandHandler<CreateProductTypeCommand, ProductTypeId>
{
    public async Task<ProductTypeId> Handle(CreateProductTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.AggregatesModel.ProductTypeAggregate.ProductType(
            request.Name,
            request.SortOrder,
            request.Visible);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
