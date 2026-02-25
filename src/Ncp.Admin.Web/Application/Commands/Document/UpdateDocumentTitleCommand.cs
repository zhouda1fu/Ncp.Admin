using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Document;

/// <summary>
/// 更新文档标题命令
/// </summary>
public record UpdateDocumentTitleCommand(DocumentId Id, string Title) : ICommand<bool>;

/// <summary>
/// 更新文档标题命令验证器
/// </summary>
public class UpdateDocumentTitleCommandValidator : AbstractValidator<UpdateDocumentTitleCommand>
{
    /// <inheritdoc />
    public UpdateDocumentTitleCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(500);
    }
}

/// <summary>
/// 更新文档标题命令处理器
/// </summary>
public class UpdateDocumentTitleCommandHandler(IDocumentRepository repository)
    : ICommandHandler<UpdateDocumentTitleCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(UpdateDocumentTitleCommand request, CancellationToken cancellationToken)
    {
        var document = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到文档", ErrorCodes.DocumentNotFound);
        document.UpdateTitle(request.Title);
        return true;
    }
}
