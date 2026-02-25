using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Document;

/// <summary>
/// 创建文档命令（带首版文件）
/// </summary>
public record CreateDocumentCommand(
    UserId CreatorId,
    string Title,
    string FileStorageKey,
    string FileName,
    long FileSize) : ICommand<DocumentId>;

/// <summary>
/// 创建文档命令验证器
/// </summary>
public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    /// <inheritdoc />
    public CreateDocumentCommandValidator()
    {
        RuleFor(c => c.CreatorId).NotNull();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FileStorageKey).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FileName).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FileSize).GreaterThanOrEqualTo(0);
    }
}

/// <summary>
/// 创建文档命令处理器
/// </summary>
public class CreateDocumentCommandHandler(IDocumentRepository repository)
    : ICommandHandler<CreateDocumentCommand, DocumentId>
{
    /// <inheritdoc />
    public async Task<DocumentId> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = new Ncp.Admin.Domain.AggregatesModel.DocumentAggregate.Document(
            request.Title, request.CreatorId, request.FileStorageKey, request.FileName, request.FileSize);
        await repository.AddAsync(document, cancellationToken);
        return document.Id;
    }
}
