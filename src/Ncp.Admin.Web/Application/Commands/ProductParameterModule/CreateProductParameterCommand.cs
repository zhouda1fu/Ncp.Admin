using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProductParameterModule;

/// <summary>
/// 创建产品参数命令
/// </summary>
/// <param name="ProductId">产品 ID</param>
/// <param name="Year">参数年份（如 "2024"）</param>
/// <param name="Description">参数内容/描述</param>
public record CreateProductParameterCommand(ProductId ProductId, string Year, string Description)
    : ICommand<ProductParameterId>;

public class CreateProductParameterCommandValidator : AbstractValidator<CreateProductParameterCommand>
{
    public CreateProductParameterCommandValidator()
    {
        RuleFor(c => c.ProductId).NotEmpty();
        RuleFor(c => c.Year).NotNull().MaximumLength(20);
        RuleFor(c => c.Description).NotNull().MaximumLength(4000);
    }
}

public class CreateProductParameterCommandHandler(IProductParameterRepository repository)
    : ICommandHandler<CreateProductParameterCommand, ProductParameterId>
{
    public async Task<ProductParameterId> Handle(CreateProductParameterCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProductParameter(request.ProductId, request.Year, request.Description);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
