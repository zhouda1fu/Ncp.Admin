using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ShareLink;

/// <summary>
/// 创建共享链接命令
/// </summary>
public record CreateShareLinkCommand(DocumentId DocumentId, UserId CreatorId, DateTimeOffset? ExpiresAt)
    : ICommand<(ShareLinkId Id, string Token)>;

/// <summary>
/// 创建共享链接命令验证器
/// </summary>
public class CreateShareLinkCommandValidator : AbstractValidator<CreateShareLinkCommand>
{
    /// <inheritdoc />
    public CreateShareLinkCommandValidator()
    {
        RuleFor(c => c.DocumentId).NotNull();
        RuleFor(c => c.CreatorId).NotNull();
    }
}

/// <summary>
/// 创建共享链接命令处理器
/// </summary>
public class CreateShareLinkCommandHandler(
    IShareLinkRepository shareLinkRepository,
    IDocumentRepository documentRepository)
    : ICommandHandler<CreateShareLinkCommand, (ShareLinkId Id, string Token)>
{
    /// <inheritdoc />
    public async Task<(ShareLinkId Id, string Token)> Handle(
        CreateShareLinkCommand request,
        CancellationToken cancellationToken)
    {
        _ = await documentRepository.GetAsync(request.DocumentId, cancellationToken)
            ?? throw new KnownException("未找到文档", ErrorCodes.DocumentNotFound);
        var token = Guid.NewGuid().ToString("N");
        var shareLink = new Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate.ShareLink(
            request.DocumentId, token, request.CreatorId, request.ExpiresAt);
        await shareLinkRepository.AddAsync(shareLink, cancellationToken);
        return (shareLink.Id, token);
    }
}
