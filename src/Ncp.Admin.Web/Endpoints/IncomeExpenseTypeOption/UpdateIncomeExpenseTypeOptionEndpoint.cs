using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.IncomeExpenseTypeOption;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.IncomeExpenseTypeOption;

public record UpdateIncomeExpenseTypeOptionRequest(string Name, int TypeValue, int SortOrder);

public class UpdateIncomeExpenseTypeOptionEndpoint(IMediator mediator)
    : Endpoint<UpdateIncomeExpenseTypeOptionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("IncomeExpenseTypeOption");
        Put("/api/admin/income-expense-type-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.IncomeExpenseTypeEdit);
    }

    public override async Task HandleAsync(UpdateIncomeExpenseTypeOptionRequest req, CancellationToken ct)
    {
        var id = new IncomeExpenseTypeOptionId(Route<Guid>("id"));
        await mediator.Send(new UpdateIncomeExpenseTypeOptionCommand(id, req.Name, req.TypeValue, req.SortOrder), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
