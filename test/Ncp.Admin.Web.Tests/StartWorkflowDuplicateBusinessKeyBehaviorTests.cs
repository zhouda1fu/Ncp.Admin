using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using NetCorePal.Extensions.Primitives;
using Npgsql;

namespace Ncp.Admin.Web.Tests;

public class StartWorkflowDuplicateBusinessKeyBehaviorTests
{
    [Fact]
    public async Task Handle_ActiveBusinessUniqueViolation_ThrowsKnownException()
    {
        var behavior = new StartWorkflowDuplicateBusinessKeyBehavior();
        var postgresException = new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            "23505",
            constraintName: "ix_workflow_instance_active_business");

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            behavior.Handle(CreateCommand(), _ => throw new DbUpdateException("duplicate", postgresException), CancellationToken.None));

        Assert.Equal(ErrorCodes.WorkflowDuplicateBusinessKey, ex.ErrorCode);
    }

    [Fact]
    public async Task Handle_OtherUniqueViolation_RethrowsOriginalException()
    {
        var behavior = new StartWorkflowDuplicateBusinessKeyBehavior();
        var postgresException = new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            "23505",
            constraintName: "other_unique_index");

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            behavior.Handle(CreateCommand(), _ => throw new DbUpdateException("duplicate", postgresException), CancellationToken.None));
    }

    private static StartWorkflowCommand CreateCommand() =>
        new(
            WorkflowDefinitionId.Unassigned,
            "biz-1",
            "Test",
            "测试流程",
            new UserId(1),
            "发起人",
            "{}",
            string.Empty);
}
