using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Product;

public record CreateProductCommand(string Name, string Code, string Model, string Unit)
    : ICommand<ProductId>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Model).NotNull().MaximumLength(100);
        RuleFor(c => c.Unit).NotEmpty().MaximumLength(20);
    }
}

public class CreateProductCommandHandler(IProductRepository repository)
    : ICommandHandler<CreateProductCommand, ProductId>
{
    public async Task<ProductId> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Domain.AggregatesModel.ProductAggregate.Product(
            request.Name ?? string.Empty,
            request.Code ?? string.Empty,
            request.Model ?? string.Empty,
            request.Unit ?? string.Empty);
        await repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
