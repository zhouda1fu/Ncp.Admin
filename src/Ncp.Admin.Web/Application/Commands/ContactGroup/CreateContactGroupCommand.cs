using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContactGroup;

/// <summary>
/// 创建联系组命令
/// </summary>
public record CreateContactGroupCommand(UserId CreatorId, string Name, int SortOrder = 0) : ICommand<ContactGroupId>;

/// <summary>
/// 创建联系组命令验证器
/// </summary>
public class CreateContactGroupCommandValidator : AbstractValidator<CreateContactGroupCommand>
{
    /// <inheritdoc />
    public CreateContactGroupCommandValidator()
    {
        RuleFor(c => c.CreatorId).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

/// <summary>
/// 创建联系组命令处理器
/// </summary>
public class CreateContactGroupCommandHandler(IContactGroupRepository repository)
    : ICommandHandler<CreateContactGroupCommand, ContactGroupId>
{
    /// <inheritdoc />
    public async Task<ContactGroupId> Handle(CreateContactGroupCommand request, CancellationToken cancellationToken)
    {
        var group = new Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate.ContactGroup(
            request.Name, request.CreatorId, request.SortOrder);
        await repository.AddAsync(group, cancellationToken);
        return group.Id;
    }
}
