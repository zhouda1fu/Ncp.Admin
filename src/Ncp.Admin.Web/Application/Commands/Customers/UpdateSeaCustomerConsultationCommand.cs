using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 公海未领用客户仅更新咨询内容。
/// </summary>
public record UpdateSeaCustomerConsultationCommand(CustomerId Id, string ConsultationContent) : ICommand<bool>;

public class UpdateSeaCustomerConsultationCommandValidator : AbstractValidator<UpdateSeaCustomerConsultationCommand>
{
    public UpdateSeaCustomerConsultationCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.ConsultationContent).NotEmpty().MaximumLength(4000);
    }
}

public class UpdateSeaCustomerConsultationCommandHandler(ICustomerRepository repository)
    : ICommandHandler<UpdateSeaCustomerConsultationCommand, bool>
{
    public async Task<bool> Handle(UpdateSeaCustomerConsultationCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.UpdateConsultationWhenInSea(request.ConsultationContent);
        return true;
    }
}
