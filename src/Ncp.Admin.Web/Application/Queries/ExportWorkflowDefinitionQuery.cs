using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Queries;

public record ExportWorkflowDefinitionQuery(WorkflowDefinitionId Id) : IQuery<WorkflowDefinitionExportDocument?>;

public class ExportWorkflowDefinitionQueryHandler(
    WorkflowDefinitionQuery definitionQuery,
    WorkflowDefinitionExportService exportService)
    : IQueryHandler<ExportWorkflowDefinitionQuery, WorkflowDefinitionExportDocument?>
{
    public async Task<WorkflowDefinitionExportDocument?> Handle(
        ExportWorkflowDefinitionQuery request,
        CancellationToken cancellationToken)
    {
        var definition = await definitionQuery.GetDefinitionByIdAsync(request.Id, cancellationToken);
        if (definition == null)
        {
            return null;
        }

        return exportService.Build(
            definition.Name,
            definition.Description,
            definition.Category,
            definition.DesignerSchemaJson);
    }
}
