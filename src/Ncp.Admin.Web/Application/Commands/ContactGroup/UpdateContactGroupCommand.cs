using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContactGroup;

/// <summary>
/// 更新联系组命令
/// </summary>
public record UpdateContactGroupCommand(ContactGroupId Id, string Name, int SortOrder) : ICommand<bool>;

/// <summary>
/// 更新联系组命令验证器
/// </summary>
public class UpdateContactGroupCommandValidator : AbstractValidator<UpdateContactGroupCommand>
{
    /// <inheritdoc />
    public UpdateContactGroupCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

/// <summary>
/// 更新联系组命令处理器
/// </summary>
public class UpdateContactGroupCommandHandler(IContactGroupRepository repository)
    : ICommandHandler<UpdateContactGroupCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(UpdateContactGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到联系组", ErrorCodes.ContactGroupNotFound);
        group.Update(request.Name, request.SortOrder);
        return true;
    }
}
