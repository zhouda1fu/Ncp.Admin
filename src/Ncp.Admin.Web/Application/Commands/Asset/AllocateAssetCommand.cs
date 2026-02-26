using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Asset;

public record AllocateAssetCommand(AssetId AssetId, UserId UserId, string? Note = null) : ICommand<AssetAllocationId>;

public class AllocateAssetCommandValidator : AbstractValidator<AllocateAssetCommand>
{
    public AllocateAssetCommandValidator()
    {
        RuleFor(c => c.AssetId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
    }
}

public class AllocateAssetCommandHandler(
    IAssetRepository assetRepository,
    IAssetAllocationRepository allocationRepository) : ICommandHandler<AllocateAssetCommand, AssetAllocationId>
{
    public async Task<AssetAllocationId> Handle(AllocateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetAsync(request.AssetId, cancellationToken)
            ?? throw new KnownException("未找到资产", ErrorCodes.AssetNotFound);
        asset.MarkAllocated();
        var allocation = new AssetAllocation(request.AssetId, request.UserId, request.Note);
        await allocationRepository.AddAsync(allocation, cancellationToken);
        return allocation.Id;
    }
}
