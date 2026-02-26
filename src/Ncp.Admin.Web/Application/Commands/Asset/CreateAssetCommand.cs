using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using AssetEntity = Ncp.Admin.Domain.AggregatesModel.AssetAggregate.Asset;

namespace Ncp.Admin.Web.Application.Commands.Asset;

public record CreateAssetCommand(
    string Name,
    string Category,
    string Code,
    DateTimeOffset PurchaseDate,
    decimal Value,
    UserId CreatorId,
    string? Remark = null) : ICommand<AssetId>;

public class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Category).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
    }
}

public class CreateAssetCommandHandler(IAssetRepository repository) : ICommandHandler<CreateAssetCommand, AssetId>
{
    public async Task<AssetId> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = new AssetEntity(
            request.Name, request.Category, request.Code,
            request.PurchaseDate, request.Value, request.CreatorId, request.Remark);
        await repository.AddAsync(asset, cancellationToken);
        return asset.Id;
    }
}
