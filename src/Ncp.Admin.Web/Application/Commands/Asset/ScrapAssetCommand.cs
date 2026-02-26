using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Asset;

public record ScrapAssetCommand(AssetId Id) : ICommand<bool>;

public class ScrapAssetCommandHandler(IAssetRepository repository) : ICommandHandler<ScrapAssetCommand, bool>
{
    public async Task<bool> Handle(ScrapAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到资产", ErrorCodes.AssetNotFound);
        asset.Scrap();
        return true;
    }
}
