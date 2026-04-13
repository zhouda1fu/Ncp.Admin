using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record UpdateCustomerContactCommand(
    CustomerId CustomerId,
    CustomerContactId ContactId,
    string Name,
    string ContactType,
    int Gender,
    DateTimeOffset Birthday,
    string Position,
    string Mobile,
    string Phone,
    string Email,
    string Qq,
    string Wechat,
    bool IsWechatAdded,
    bool IsPrimary) : ICommand<bool>;

public class UpdateCustomerContactCommandValidator : AbstractValidator<UpdateCustomerContactCommand>
{
    public UpdateCustomerContactCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.ContactId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
        RuleFor(c => c).Must(c => CustomerContactCommandValidation.HasAtLeastOneContactChannel(
                c.Mobile, c.Phone, c.Qq, c.Wechat))
            .WithMessage(CustomerContactCommandValidation.AtLeastOneChannelMessage);
        RuleFor(c => c.Mobile).Must(CustomerContactCommandValidation.IsValidMobileIfPresent)
            .WithMessage(CustomerContactCommandValidation.MobileFormatMessage);
        RuleFor(c => c.Qq).Must(CustomerContactCommandValidation.IsValidQqIfPresent)
            .WithMessage(CustomerContactCommandValidation.QqFormatMessage);
    }
}

public class UpdateCustomerContactCommandHandler(ICustomerRepository repository) : ICommandHandler<UpdateCustomerContactCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomerContactCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetWithContactsAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.UpdateContact(
            request.ContactId, request.Name, request.ContactType , request.Gender, request.Birthday,
            request.Position , request.Mobile , request.Phone , request.Email,
            request.Qq, request.Wechat, request.IsWechatAdded,
            request.IsPrimary);
        return true;
    }
}
