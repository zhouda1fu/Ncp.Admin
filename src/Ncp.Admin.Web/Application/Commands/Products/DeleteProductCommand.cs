using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.Products;

public record DeleteProductCommand(ProductId Id) : ICommand<bool>;

public class DeleteProductCommandHandler(IProductRepository repository)
    : ICommandHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品", ErrorCodes.ProductNotFound);
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
