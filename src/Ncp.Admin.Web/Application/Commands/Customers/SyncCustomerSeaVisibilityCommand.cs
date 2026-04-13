using FluentValidation;
using MediatR;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaVisibilityAggregate;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Commands.Notifications;
using Ncp.Admin.Web.Application.Services;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 按客户当前区域与片区分配，同步公海可见性授权并发送站内通知。
/// </summary>
public record SyncCustomerSeaVisibilityCommand(CustomerId CustomerId) : ICommand;

public class SyncCustomerSeaVisibilityCommandValidator : AbstractValidator<SyncCustomerSeaVisibilityCommand>
{
    public SyncCustomerSeaVisibilityCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}

public class SyncCustomerSeaVisibilityCommandHandler(
    ICustomerRepository customerRepository,
    ICustomerSeaVisibilityBoardRepository boardRepository,
    ICustomerSeaVisibilityTargetResolver targetResolver,
    IMediator mediator) : ICommandHandler<SyncCustomerSeaVisibilityCommand>
{
    public async Task Handle(SyncCustomerSeaVisibilityCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);

        var targetUserIds = await targetResolver.ResolveUserIdsAsync(customer, cancellationToken);

        var board = await boardRepository.GetWithEntriesAsync(customer.Id, cancellationToken);
        if (board is null)
        {
            board = new CustomerSeaVisibilityBoard(customer.Id);
            await boardRepository.AddAsync(board, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var sync = board.SyncToDesiredUsers(targetUserIds, now);

        var businessId = customer.Id.ToString();
        const string businessType = "CustomerSea";

        foreach (var uid in sync.RevokedUserIds)
        {
            await mediator.Send(new SendNotificationCommand(
                "公海客户可见性已撤回",
                "您不再对某公海客户拥有片区可见授权（区域已变更或不再匹配）。",
                NotificationType.System,
                NotificationLevel.Info,
                null,
                "系统",
                uid.Id,
                businessId,
                businessType), cancellationToken);
        }

        foreach (var uid in sync.GrantedUserIds)
        {
            await mediator.Send(new SendNotificationCommand(
                "公海客户片区通知",
                "有新的公海客户与您的负责片区匹配，请在公海列表中查看。",
                NotificationType.System,
                NotificationLevel.Info,
                null,
                "系统",
                uid.Id,
                businessId,
                businessType), cancellationToken);
        }
    }
}
