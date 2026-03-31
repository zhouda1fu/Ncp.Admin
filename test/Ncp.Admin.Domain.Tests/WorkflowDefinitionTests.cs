using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

namespace Ncp.Admin.Domain.Tests;

/// <summary>
/// 工作流定义聚合根单元测试：草稿 -> 发布 -> 归档生命周期。
/// </summary>
public class WorkflowDefinitionTests
{
    private static readonly UserId CreatorId = new(1);

    private static WorkflowDefinition CreateDraft()
    {
        return new WorkflowDefinition("请假审批", "描述", "请假", "{}", CreatorId);
    }

    [Fact]
    public void Constructor_ShouldSetDraftStatus()
    {
        var def = CreateDraft();

        Assert.Equal(WorkflowDefinitionStatus.Draft, def.Status);
        Assert.Equal("请假审批", def.Name);
        Assert.Equal(1, def.Version);
    }

    [Fact]
    public void UpdateInfo_WhenDraft_ShouldUpdateFields()
    {
        var def = CreateDraft();

        def.UpdateInfo("新名称", "新描述", "新分类", "{\"node\":1}");

        Assert.Equal("新名称", def.Name);
        Assert.Equal("新描述", def.Description);
        Assert.Equal("新分类", def.Category);
        Assert.Equal("{\"node\":1}", def.DefinitionJson);
    }

    [Fact]
    public void UpdateInfo_WhenPublished_ShouldThrow()
    {
        var def = CreateDraft();
        def.Publish();

        var ex = Assert.Throws<KnownException>(() => def.UpdateInfo("x", "x", "x", "{}"));

        Assert.Equal(ErrorCodes.WorkflowDefinitionAlreadyPublished, ex.ErrorCode);
    }

    [Fact]
    public void Publish_WhenDraft_ShouldSetPublished()
    {
        var def = CreateDraft();

        def.Publish();

        Assert.Equal(WorkflowDefinitionStatus.Published, def.Status);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldThrow()
    {
        var def = CreateDraft();
        def.Publish();

        var ex = Assert.Throws<KnownException>(() => def.Publish());

        Assert.Equal(ErrorCodes.WorkflowDefinitionAlreadyPublished, ex.ErrorCode);
    }

    [Fact]
    public void Archive_WhenPublished_ShouldSetArchived()
    {
        var def = CreateDraft();
        def.Publish();

        def.Archive();

        Assert.Equal(WorkflowDefinitionStatus.Archived, def.Status);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldThrow()
    {
        var def = CreateDraft();
        def.Publish();
        def.Archive();

        var ex = Assert.Throws<KnownException>(() => def.Archive());

        Assert.Equal(ErrorCodes.WorkflowDefinitionAlreadyArchived, ex.ErrorCode);
    }

    [Fact]
    public void Publish_WhenArchived_ShouldThrow()
    {
        var def = CreateDraft();
        def.Publish();
        def.Archive();

        var ex = Assert.Throws<KnownException>(() => def.Publish());

        Assert.Equal(ErrorCodes.WorkflowDefinitionAlreadyArchived, ex.ErrorCode);
    }

    [Fact]
    public void CreateNewVersion_ShouldIncrementVersion()
    {
        var def = CreateDraft();

        var newDef = def.CreateNewVersion();

        Assert.Equal(2, newDef.Version);
        Assert.Equal(def.Name, newDef.Name);
    }

    [Fact]
    public void SoftDelete_WhenNotDeleted_ShouldSetIsDeleted()
    {
        var def = CreateDraft();

        def.SoftDelete();

        Assert.True(def.IsDeleted);
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldThrow()
    {
        var def = CreateDraft();
        def.SoftDelete();

        var ex = Assert.Throws<KnownException>(() => def.SoftDelete());

        Assert.Equal(ErrorCodes.WorkflowDefinitionAlreadyDeleted, ex.ErrorCode);
    }
}
