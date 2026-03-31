using Ncp.Admin.Domain.DomainEvents.PositionEvents;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 岗位信息变更领域事件处理器 - 用于更新用户岗位名称
/// </summary>
public class PositionInfoChangedDomainEventHandlerForUpdateUserPositionName(IUserRepository userRepository) : IDomainEventHandler<PositionInfoChangedDomainEvent>
{
    public async Task Handle(PositionInfoChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var position = domainEvent.Position;

        await userRepository.BulkUpdateUserPositionNamesAsync(position.Id, position.Name, cancellationToken);
    }
}
