using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Workflows;

/// <summary>
/// 从导出 JSON 导入流程定义：按名称重映射用户/角色/部门 ID，并按流程名称+分类更新草稿或新建。
/// </summary>
public record ImportWorkflowDefinitionFromExportCommand(
    string Format,
    int Version,
    string Name,
    string Description,
    string Category,
    string DesignerSchemaJson,
    UserId CreatedBy,
    bool UpsertByName = true) : ICommand<ImportWorkflowDefinitionResult>;

public record ImportWorkflowDefinitionResult(
    WorkflowDefinitionId Id,
    string Name,
    ImportWorkflowDefinitionAction Action,
    WorkflowDefinitionIdentityRemapReport RemapReport,
    IReadOnlyList<string> Warnings);

public enum ImportWorkflowDefinitionAction
{
    Created = 0,
    Updated = 1,
}

public class ImportWorkflowDefinitionFromExportCommandValidator
    : AbstractValidator<ImportWorkflowDefinitionFromExportCommand>
{
    public const string ExpectedFormat = WorkflowDefinitionExportService.Format;
    public static readonly int[] SupportedVersions =
    [
        WorkflowDefinitionExportService.LegacyVersion,
        WorkflowDefinitionExportService.CurrentVersion,
    ];

    public ImportWorkflowDefinitionFromExportCommandValidator()
    {
        RuleFor(c => c.Format)
            .NotEmpty()
            .Equal(ExpectedFormat)
            .WithMessage("导入文件格式不正确（需为本系统导出的流程定义 JSON）");
        RuleFor(c => c.Version)
            .Must(v => SupportedVersions.Contains(v))
            .WithMessage("不支持的导出文件版本");
        RuleFor(c => c.Name).NotEmpty().WithMessage("流程名称不能为空")
            .MaximumLength(200).WithMessage("流程名称长度不能超过200个字符");
        RuleFor(c => c.Description).MaximumLength(500).WithMessage("流程描述长度不能超过500个字符");
        RuleFor(c => c.Category).MaximumLength(100).WithMessage("流程分类长度不能超过100个字符");
    }
}

public class ImportWorkflowDefinitionFromExportCommandHandler(
    IMediator mediator,
    IWorkflowDefinitionRepository repository,
    WorkflowDefinitionIdentityRemapper identityRemapper,
    WorkflowDefinitionAssigneeConfigValidator assigneeConfigValidator,
    WorkflowDefinitionCacheInvalidator cacheInvalidator)
    : ICommandHandler<ImportWorkflowDefinitionFromExportCommand, ImportWorkflowDefinitionResult>
{
    public async Task<ImportWorkflowDefinitionResult> Handle(
        ImportWorkflowDefinitionFromExportCommand request,
        CancellationToken cancellationToken)
    {
        var remap = await identityRemapper.RemapAsync(
            request.DesignerSchemaJson ?? string.Empty,
            request.Category,
            cancellationToken);

        await assigneeConfigValidator.ValidateAsync(remap.DesignerSchemaJson, request.Category, cancellationToken);

        if (request.UpsertByName)
        {
            var existing = await repository.GetByNameAndCategoryAsync(
                request.Name,
                request.Category ?? string.Empty,
                cancellationToken);

            if (existing != null)
            {
                if (existing.Status == WorkflowDefinitionStatus.Published)
                {
                    throw new KnownException(
                        $"已存在已发布的流程「{request.Name}」（分类：{request.Category}），请先归档或删除后再导入，或在设计器中手动调整",
                        ErrorCodes.WorkflowDefinitionAlreadyPublished);
                }

                if (existing.Status == WorkflowDefinitionStatus.Archived)
                {
                    throw new KnownException(
                        $"已存在已归档的流程「{request.Name}」（分类：{request.Category}），请删除后再导入",
                        ErrorCodes.WorkflowDefinitionAlreadyArchived);
                }

                existing.UpdateInfo(
                    request.Name,
                    request.Description ?? string.Empty,
                    request.Category ?? string.Empty,
                    remap.DesignerSchemaJson);
                existing.UpdateLatestDraftVersion(remap.DesignerSchemaJson);
                cacheInvalidator.InvalidateDefinitionWrite(existing.Id);

                return new ImportWorkflowDefinitionResult(
                    existing.Id,
                    existing.Name,
                    ImportWorkflowDefinitionAction.Updated,
                    remap.Report,
                    remap.Warnings);
            }
        }

        var id = await mediator.Send(
            new CreateWorkflowDefinitionCommand(
                request.Name,
                request.Description ?? string.Empty,
                request.Category ?? string.Empty,
                remap.DesignerSchemaJson,
                request.CreatedBy),
            cancellationToken);

        return new ImportWorkflowDefinitionResult(
            id,
            request.Name,
            ImportWorkflowDefinitionAction.Created,
            remap.Report,
            remap.Warnings);
    }
}
