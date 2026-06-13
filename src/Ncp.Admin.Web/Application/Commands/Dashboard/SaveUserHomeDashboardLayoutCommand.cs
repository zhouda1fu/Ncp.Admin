using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Dashboard;

namespace Ncp.Admin.Web.Application.Commands.Dashboard;

/// <summary>
/// 保存当前用户可拖拽首页卡片的排序（不含置顶区；仅保存当前有权展示的卡片 key）。
/// </summary>
/// <param name="UserId">用户标识。</param>
/// <param name="CardOrder">客户端提交的卡片 key 顺序。</param>
/// <param name="GrantedPermissionCodes">当前用户 JWT 中的权限码集合。</param>
public record SaveUserHomeDashboardLayoutCommand(
    UserId UserId,
    IReadOnlyList<string> CardOrder,
    IReadOnlySet<string> GrantedPermissionCodes) : ICommand;

public class SaveUserHomeDashboardLayoutCommandValidator : AbstractValidator<SaveUserHomeDashboardLayoutCommand>
{
    public SaveUserHomeDashboardLayoutCommandValidator()
    {
        RuleFor(x => x.CardOrder).NotNull();
        RuleFor(x => x.GrantedPermissionCodes).NotNull();
        RuleForEach(x => x.CardOrder).Must(HomeDashboardCardKeys.IsKnownCardKey);
    }
}

public class SaveUserHomeDashboardLayoutCommandHandler(IUserHomeDashboardPreferenceRepository preferenceRepository)
    : ICommandHandler<SaveUserHomeDashboardLayoutCommand>
{
    public async Task Handle(SaveUserHomeDashboardLayoutCommand request, CancellationToken cancellationToken)
    {
        var sortableOnly = HomeDashboardCardAccess
            .FilterSortableCardKeysForSave(request.CardOrder, request.GrantedPermissionCodes)
            .ToList();

        var sortableDefaults = HomeDashboardCardKeys.DefaultCardOrder
            .Where(k => !HomeDashboardCardKeys.IsPinnedCard(k)
                        && HomeDashboardCardAccess.CanViewCard(k, request.GrantedPermissionCodes));
        sortableOnly.AddRange(sortableDefaults.Where(k => !sortableOnly.Contains(k, StringComparer.Ordinal)));

        var json = JsonSerializer.Serialize(sortableOnly);
        var existing = await preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (existing is null)
        {
            await preferenceRepository.AddAsync(
                new UserHomeDashboardPreference(request.UserId, json),
                cancellationToken);
        }
        else
        {
            existing.UpdateCardOrder(json);
        }
    }
}
