using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.Suppliers;

/// <summary>
/// 更新供应商命令
/// </summary>
public record UpdateSupplierCommand(
    SupplierId Id,
    string FullName,
    string ShortName,
    string Contact,
    string Phone,
    string Email,
    string Address,
    string Remark)
    : ICommand<bool>;

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ShortName).NotNull().MaximumLength(100);
        RuleFor(c => c.Contact).NotNull().MaximumLength(100);
        RuleFor(c => c.Phone).NotNull().MaximumLength(50);
        RuleFor(c => c.Email).NotNull().MaximumLength(100);
        RuleFor(c => c.Address).NotNull().MaximumLength(500);
        RuleFor(c => c.Remark).NotNull().MaximumLength(500);
    }
}

public class UpdateSupplierCommandHandler(ISupplierRepository repository)
    : ICommandHandler<UpdateSupplierCommand, bool>
{
    public async Task<bool> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到供应商", ErrorCodes.SupplierNotFound);
        entity.Update(
            request.FullName,
            request.ShortName,
            request.Contact,
            request.Phone,
            request.Email,
            request.Address,
            request.Remark);
        return true;
    }
}
