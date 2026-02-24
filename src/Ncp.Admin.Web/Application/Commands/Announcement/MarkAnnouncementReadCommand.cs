using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Announcements;

/// <summary>
/// 标记公告已读命令
/// </summary>
public record MarkAnnouncementReadCommand(AnnouncementId AnnouncementId, UserId UserId) : ICommand;

/// <summary>
/// 标记公告已读命令验证器
/// </summary>
public class MarkAnnouncementReadCommandValidator : AbstractValidator<MarkAnnouncementReadCommand>
{
    /// <inheritdoc />
    public MarkAnnouncementReadCommandValidator()
    {
        RuleFor(c => c.AnnouncementId).NotNull();
        RuleFor(c => c.UserId).NotNull();
    }
}

public class MarkAnnouncementReadCommandHandler(
    IAnnouncementRepository announcementRepository,
    IAnnouncementReadRecordRepository readRecordRepository)
    : ICommandHandler<MarkAnnouncementReadCommand>
{
    /// <inheritdoc />
    public async Task Handle(MarkAnnouncementReadCommand request, CancellationToken cancellationToken)
    {
        var announcement = await announcementRepository.GetAsync(request.AnnouncementId, cancellationToken);
        if (announcement == null || announcement.Status != AnnouncementStatus.Published) return;
        if (await readRecordRepository.ExistsAsync(request.AnnouncementId, request.UserId, cancellationToken))
            return;
        var record = new AnnouncementReadRecord(request.AnnouncementId, request.UserId);
        await readRecordRepository.AddAsync(record, cancellationToken);
    }
}
