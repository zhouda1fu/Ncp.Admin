using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.Product;

public record UpdateProductCommand(ProductId Id, string Name, string Code, string Model, string Unit)
    : ICommand<bool>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Model).NotNull().MaximumLength(100);
        RuleFor(c => c.Unit).NotEmpty().MaximumLength(20);
    }
}

public class UpdateProductCommandHandler(IProductRepository repository)
    : ICommandHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品", ErrorCodes.ProductNotFound);
        product.Update(
            request.Name ?? string.Empty,
            request.Code ?? string.Empty,
            request.Model ?? string.Empty,
            request.Unit ?? string.Empty);
        return true;
    }
}
