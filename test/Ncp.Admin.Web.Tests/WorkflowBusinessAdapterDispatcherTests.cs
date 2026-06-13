using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Tests;

public class WorkflowBusinessAdapterDispatcherTests
{
    [Fact]
    public void GetIntegrations_ReturnsRegisteredBusinessContracts()
    {
        var dispatcher = new WorkflowBusinessAdapterDispatcher(
        [
            new DefaultAdapter("Order"),
            new PayloadAdapter(),
        ]);

        var integrations = dispatcher.GetIntegrations();

        Assert.Equal(["Order", "PersonnelBenefit"], integrations.Select(i => i.BusinessType).ToArray());
        var payload = integrations.Single(i => i.BusinessType == "PersonnelBenefit");
        Assert.Contains(WorkflowBusinessCallbackNames.BeforeTaskApproved, payload.CallbackNames);
        var schema = Assert.Single(payload.ActionPayloadSchemas);
        Assert.Equal("personnelBenefit.purchaserUserId", schema.FieldPath);
    }

    private sealed class DefaultAdapter(string businessType) : IWorkflowBusinessAdapter
    {
        public string BusinessType { get; } = businessType;
    }

    private sealed class PayloadAdapter : IWorkflowBusinessAdapter
    {
        public string BusinessType => "PersonnelBenefit";

        public WorkflowBusinessIntegrationDescriptor Integration => WorkflowBusinessIntegrationDescriptor.Create(
            BusinessType,
            [],
            [
                WorkflowBusinessCallbackNames.BeforeTaskApproved,
                WorkflowBusinessCallbackNames.Completed,
            ],
            [
                new WorkflowActionPayloadSchemaDto(
                    "personnelBenefit",
                    "personnelBenefit.purchaserUserId",
                    "number|string",
                    false,
                    "购买执行人用户 ID"),
            ]);

        public Task OnCompletedAsync(WorkflowInstance instance, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
