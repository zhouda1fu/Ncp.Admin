using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Announcements;

/// <summary>
/// 发布公告命令（仅草稿可发布）
/// </summary>
public record PublishAnnouncementCommand(AnnouncementId Id) : ICommand;

/// <summary>
/// 发布公告命令验证器
/// </summary>
public class PublishAnnouncementCommandValidator : AbstractValidator<PublishAnnouncementCommand>
{
    /// <inheritdoc />
    public PublishAnnouncementCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
    }
}

/// <summary>
/// 发布公告命令处理器
/// </summary>
public class PublishAnnouncementCommandHandler(IAnnouncementRepository repository)
    : ICommandHandler<PublishAnnouncementCommand>
{
    /// <inheritdoc />
    public async Task Handle(PublishAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到公告", ErrorCodes.AnnouncementNotFound);
        entity.Publish();
    }
}
