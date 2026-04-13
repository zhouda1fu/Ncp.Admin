using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.Suppliers;

/// <summary>
/// 删除供应商命令
/// </summary>
public record DeleteSupplierCommand(SupplierId Id) : ICommand<bool>;

public class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteSupplierCommandHandler(ISupplierRepository repository)
    : ICommandHandler<DeleteSupplierCommand, bool>
{
    public async Task<bool> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        _ = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到供应商", ErrorCodes.SupplierNotFound);
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
