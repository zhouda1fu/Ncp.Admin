using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Tasks;

public record DeleteTaskCommand(ProjectTaskId Id) : ICommand<bool>;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
    }
}

public class DeleteTaskCommandHandler(IProjectTaskRepository repository)
    : ICommandHandler<DeleteTaskCommand, bool>
{
    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        _ = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到任务", ErrorCodes.TaskNotFound);
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
