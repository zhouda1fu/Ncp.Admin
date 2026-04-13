using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Suppliers;

/// <summary>
/// 创建供应商命令
/// </summary>
public record CreateSupplierCommand(
    string FullName,
    string ShortName,
    string Contact,
    string Phone,
    string Email,
    string Address,
    string Remark)
    : ICommand<SupplierId>;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ShortName).NotNull().MaximumLength(100);
        RuleFor(c => c.Contact).NotNull().MaximumLength(100);
        RuleFor(c => c.Phone).NotNull().MaximumLength(50);
        RuleFor(c => c.Email).NotNull().MaximumLength(100);
        RuleFor(c => c.Address).NotNull().MaximumLength(500);
        RuleFor(c => c.Remark).NotNull().MaximumLength(500);
    }
}

public class CreateSupplierCommandHandler(ISupplierRepository repository)
    : ICommandHandler<CreateSupplierCommand, SupplierId>
{
    public async Task<SupplierId> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var entity = new Supplier(
            request.FullName,
            request.ShortName,
            request.Contact,
            request.Phone,
            request.Email,
            request.Address,
            request.Remark);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
