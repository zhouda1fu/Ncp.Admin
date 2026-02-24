using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Announcements;

/// <summary>
/// 创建公告命令（初始为草稿）
/// </summary>
public record CreateAnnouncementCommand(UserId PublisherId, string PublisherName, string Title, string Content) : ICommand<AnnouncementId>;

/// <summary>
/// 创建公告命令验证器
/// </summary>
public class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    /// <inheritdoc />
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(c => c.PublisherId).NotNull();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Content).NotEmpty();
    }
}

/// <summary>
/// 创建公告命令处理器
/// </summary>
public class CreateAnnouncementCommandHandler(IAnnouncementRepository repository)
    : ICommandHandler<CreateAnnouncementCommand, AnnouncementId>
{
    /// <inheritdoc />
    public async Task<AnnouncementId> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var entity = new Announcement(request.PublisherId, request.PublisherName ?? "", request.Title, request.Content);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
