using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Contact;

/// <summary>
/// 更新联系人命令
/// </summary>
public record UpdateContactCommand(
    ContactId Id,
    string Name,
    string? Phone,
    string? Email,
    string? Company,
    ContactGroupId? GroupId) : ICommand<bool>;

/// <summary>
/// 更新联系人命令验证器
/// </summary>
public class UpdateContactCommandValidator : AbstractValidator<UpdateContactCommand>
{
    /// <inheritdoc />
    public UpdateContactCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Phone).MaximumLength(50);
        RuleFor(c => c.Email).MaximumLength(200);
        RuleFor(c => c.Company).MaximumLength(200);
    }
}

/// <summary>
/// 更新联系人命令处理器
/// </summary>
public class UpdateContactCommandHandler(IContactRepository repository)
    : ICommandHandler<UpdateContactCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到联系人", ErrorCodes.ContactNotFound);
        contact.Update(request.Name, request.Phone, request.Email, request.Company, request.GroupId);
        return true;
    }
}
