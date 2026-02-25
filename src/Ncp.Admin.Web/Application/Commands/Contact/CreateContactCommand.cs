using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Contact;

/// <summary>
/// 创建联系人命令
/// </summary>
public record CreateContactCommand(
    UserId CreatorId,
    string Name,
    string? Phone,
    string? Email,
    string? Company,
    ContactGroupId? GroupId) : ICommand<ContactId>;

/// <summary>
/// 创建联系人命令验证器
/// </summary>
public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    /// <inheritdoc />
    public CreateContactCommandValidator()
    {
        RuleFor(c => c.CreatorId).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Phone).MaximumLength(50);
        RuleFor(c => c.Email).MaximumLength(200);
        RuleFor(c => c.Company).MaximumLength(200);
    }
}

/// <summary>
/// 创建联系人命令处理器
/// </summary>
public class CreateContactCommandHandler(IContactRepository repository)
    : ICommandHandler<CreateContactCommand, ContactId>
{
    /// <inheritdoc />
    public async Task<ContactId> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = new Ncp.Admin.Domain.AggregatesModel.ContactAggregate.Contact(
            request.Name, request.CreatorId, request.Phone, request.Email, request.Company, request.GroupId);
        await repository.AddAsync(contact, cancellationToken);
        return contact.Id;
    }
}
