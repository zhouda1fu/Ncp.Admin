using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Announcements;

/// <summary>
/// 更新公告命令（仅草稿可更新）
/// </summary>
public record UpdateAnnouncementCommand(AnnouncementId Id, string Title, string Content) : ICommand;

/// <summary>
/// 更新公告命令验证器
/// </summary>
public class UpdateAnnouncementCommandValidator : AbstractValidator<UpdateAnnouncementCommand>
{
    /// <inheritdoc />
    public UpdateAnnouncementCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Content).NotEmpty();
    }
}

/// <summary>
/// 更新公告命令处理器
/// </summary>
public class UpdateAnnouncementCommandHandler(IAnnouncementRepository repository)
    : ICommandHandler<UpdateAnnouncementCommand>
{
    /// <inheritdoc />
    public async Task Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到公告", ErrorCodes.AnnouncementNotFound);
        entity.UpdateDraft(request.Title, request.Content);
    }
}
