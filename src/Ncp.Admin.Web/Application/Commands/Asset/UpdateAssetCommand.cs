using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Asset;

public record UpdateAssetCommand(
    AssetId Id,
    string Name,
    string Category,
    string Code,
    DateTimeOffset PurchaseDate,
    decimal Value,
    string? Remark = null) : ICommand<bool>;

public class UpdateAssetCommandValidator : AbstractValidator<UpdateAssetCommand>
{
    public UpdateAssetCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Category).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
    }
}

public class UpdateAssetCommandHandler(IAssetRepository repository) : ICommandHandler<UpdateAssetCommand, bool>
{
    public async Task<bool> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到资产", ErrorCodes.AssetNotFound);
        asset.Update(request.Name, request.Category, request.Code, request.PurchaseDate, request.Value, request.Remark);
        return true;
    }
}
