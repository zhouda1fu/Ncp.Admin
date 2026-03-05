using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.IncomeExpenseTypeOption;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.IncomeExpenseTypeOption;

public record CreateIncomeExpenseTypeOptionRequest(string Name, int TypeValue, int SortOrder = 0);

public record CreateIncomeExpenseTypeOptionResponse(IncomeExpenseTypeOptionId Id);

public class CreateIncomeExpenseTypeOptionEndpoint(IMediator mediator)
    : Endpoint<CreateIncomeExpenseTypeOptionRequest, ResponseData<CreateIncomeExpenseTypeOptionResponse>>
{
    public override void Configure()
    {
        Tags("IncomeExpenseTypeOption");
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
