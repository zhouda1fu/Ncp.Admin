using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.IncomeExpenseTypeOptions;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.IncomeExpenseTypeOptions;

public record CreateIncomeExpenseTypeOptionRequest(string Name, int TypeValue, int SortOrder = 0);

public record CreateIncomeExpenseTypeOptionResponse(IncomeExpenseTypeOptionId Id);

public class CreateIncomeExpenseTypeOptionEndpoint(IMediator mediator)
    : Endpoint<CreateIncomeExpenseTypeOptionRequest, ResponseData<CreateIncomeExpenseTypeOptionResponse>>
{
    public override void Configure()
    {
        Tags("IncomeExpenseTypeOption");
        Description(b => b.AutoTagOverride("IncomeExpenseTypeOption").WithSummary("创建收支类型选项"));
        Post("/api/admin/income-expense-type-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.IncomeExpenseTypeCreate);
    }

    public override async Task HandleAsync(CreateIncomeExpenseTypeOptionRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateIncomeExpenseTypeOptionCommand(req.Name, req.TypeValue, req.SortOrder), ct);
        await Send.OkAsync(new CreateIncomeExpenseTypeOptionResponse(id).AsResponseData(), cancellation: ct);
    }
}
