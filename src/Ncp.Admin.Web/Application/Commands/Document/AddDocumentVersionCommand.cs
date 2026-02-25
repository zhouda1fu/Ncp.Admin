using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Document;

/// <summary>
/// 添加文档新版本命令
/// </summary>
public record AddDocumentVersionCommand(DocumentId DocumentId, string FileStorageKey, string FileName, long FileSize)
    : ICommand<bool>;

/// <summary>
/// 添加文档版本命令验证器
/// </summary>
public class AddDocumentVersionCommandValidator : AbstractValidator<AddDocumentVersionCommand>
{
    /// <inheritdoc />
    public AddDocumentVersionCommandValidator()
    {
        RuleFor(c => c.DocumentId).NotNull();
        RuleFor(c => c.FileStorageKey).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FileName).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FileSize).GreaterThanOrEqualTo(0);
    }
}

/// <summary>
/// 添加文档版本命令处理器
/// </summary>
public class AddDocumentVersionCommandHandler(IDocumentRepository repository)
    : ICommandHandler<AddDocumentVersionCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(AddDocumentVersionCommand request, CancellationToken cancellationToken)
    {
        var document = await repository.GetAsync(request.DocumentId, cancellationToken)
            ?? throw new KnownException("未找到文档", ErrorCodes.DocumentNotFound);
        document.AddVersion(request.FileStorageKey, request.FileName, request.FileSize);
        return true;
    }
}
