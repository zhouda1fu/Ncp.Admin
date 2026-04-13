using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.ProductParameters;

/// <summary>
/// 更新产品参数命令
/// </summary>
/// <param name="Id">参数 ID</param>
/// <param name="Year">参数年份</param>
/// <param name="Description">参数内容/描述</param>
public record UpdateProductParameterCommand(ProductParameterId Id, string Year, string Description)
    : ICommand<bool>;

public class UpdateProductParameterCommandValidator : AbstractValidator<UpdateProductParameterCommand>
{
    public UpdateProductParameterCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Year).NotNull().MaximumLength(20);
        RuleFor(c => c.Description).NotNull().MaximumLength(4000);
    }
}

public class UpdateProductParameterCommandHandler(IProductParameterRepository repository)
    : ICommandHandler<UpdateProductParameterCommand, bool>
{
    public async Task<bool> Handle(UpdateProductParameterCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品参数", ErrorCodes.ProductParameterNotFound);
        entity.Update(request.Year, request.Description);
        return true;
    }
}
