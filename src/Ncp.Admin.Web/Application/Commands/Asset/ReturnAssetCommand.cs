using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Asset;

public record ReturnAssetCommand(AssetAllocationId AllocationId) : ICommand<bool>;

public class ReturnAssetCommandHandler(
    IAssetAllocationRepository allocationRepository,
    IAssetRepository assetRepository) : ICommandHandler<ReturnAssetCommand, bool>
{
    public async Task<bool> Handle(ReturnAssetCommand request, CancellationToken cancellationToken)
    {
        var allocation = await allocationRepository.GetAsync(request.AllocationId, cancellationToken)
            ?? throw new KnownException("未找到领用记录", ErrorCodes.AssetAllocationNotFound);
        allocation.Return();
        var asset = await assetRepository.GetAsync(allocation.AssetId, cancellationToken)
            ?? throw new KnownException("未找到资产", ErrorCodes.AssetNotFound);
        asset.MarkAvailable();
        return true;
    }
}
