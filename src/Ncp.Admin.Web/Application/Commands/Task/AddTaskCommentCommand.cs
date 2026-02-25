using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectTask;

/// <summary>
/// 添加任务评论命令
/// </summary>
public record AddTaskCommentCommand(TaskId TaskId, string Content, UserId AuthorId) : ICommand<bool>;

/// <summary>
/// 添加任务评论命令验证器
/// </summary>
public class AddTaskCommentCommandValidator : AbstractValidator<AddTaskCommentCommand>
{
    /// <inheritdoc />
    public AddTaskCommentCommandValidator()
    {
        RuleFor(c => c.TaskId).NotNull();
        RuleFor(c => c.Content).NotEmpty().MaximumLength(2000);
        RuleFor(c => c.AuthorId).NotNull();
    }
}

/// <summary>
/// 添加任务评论命令处理器
/// </summary>
public class AddTaskCommentCommandHandler(ITaskRepository repository) : ICommandHandler<AddTaskCommentCommand, bool>
{
    /// <inheritdoc />
    public async System.Threading.Tasks.Task<bool> Handle(AddTaskCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await repository.GetAsync(request.TaskId, cancellationToken)
            ?? throw new KnownException("未找到任务", ErrorCodes.TaskNotFound);
        task.AddComment(request.Content, request.AuthorId);
        return true;
    }
}
