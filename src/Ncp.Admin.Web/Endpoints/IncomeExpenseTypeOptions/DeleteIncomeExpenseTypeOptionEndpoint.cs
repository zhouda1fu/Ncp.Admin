using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.IncomeExpenseTypeOptions;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.IncomeExpenseTypeOptions;

public class DeleteIncomeExpenseTypeOptionEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("IncomeExpenseTypeOption");
        Description(b => b.AutoTagOverride("IncomeExpenseTypeOption").WithSummary("删除收支类型选项"));
        Delete("/api/admin/income-expense-type-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.IncomeExpenseTypeDelete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await mediator.Send(new DeleteIncomeExpenseTypeOptionCommand(new IncomeExpenseTypeOptionId(id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
