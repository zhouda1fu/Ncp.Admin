using System.Text.Json;
using FluentValidation;
using MediatR;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Leave;

/// <summary>
/// 请假审批流程分类（与流程定义前端的 category 选项 value 一致：LeaveRequest）
/// </summary>
public static class LeaveWorkflowCategory
{
    public const string Category = "LeaveRequest";
}

/// <summary>
/// 提交请假申请（发起审批流程，流程按分类内置选择，无需前端传流程定义ID）
/// </summary>
public record SubmitLeaveRequestCommand(
    LeaveRequestId LeaveRequestId,
    string Remark = "") : ICommand;

public class SubmitLeaveRequestCommandValidator : AbstractValidator<SubmitLeaveRequestCommand>
{
    public SubmitLeaveRequestCommandValidator()
    {
        RuleFor(c => c.LeaveRequestId).NotNull().WithMessage("请假申请ID不能为空");
    }
}

public class SubmitLeaveRequestCommandHandler(
    ILeaveRequestRepository leaveRequestRepository,
    WorkflowDefinitionQuery workflowDefinitionQuery,
    IMediator mediator)
    : ICommandHandler<SubmitLeaveRequestCommand>
{
    public async Task Handle(SubmitLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await leaveRequestRepository.GetAsync(request.LeaveRequestId, cancellationToken)
            ?? throw new KnownException("未找到请假申请", ErrorCodes.LeaveRequestNotFound);

        var definitionDto = await workflowDefinitionQuery.GetFirstPublishedByCategoryAsync(
            LeaveWorkflowCategory.Category, cancellationToken)
            ?? throw new KnownException("未配置请假审批流程，请在流程定义中发布分类为「请假审批」的流程", ErrorCodes.LeaveWorkflowNotConfigured);

        var variables = new LeaveRequestVariables
        {
            LeaveRequestId = leave.Id.ToString(),
            LeaveType = (int)leave.LeaveType,
            StartDate = leave.StartDate.ToString("O"),
            EndDate = leave.EndDate.ToString("O"),
            Days = leave.Days,
            Reason = leave.Reason,
        };

        var title = $"请假申请-{leave.ApplicantName}-{leave.StartDate:yyyy-MM-dd}至{leave.EndDate:yyyy-MM-dd}";
        var startCmd = new StartWorkflowCommand(
            definitionDto.Id,
            leave.Id.ToString(),
            "LeaveRequest",
            title,
            leave.ApplicantId,
            leave.ApplicantName,
            JsonSerializer.Serialize(variables),
            request.Remark ?? "");

        var instanceId = await mediator.Send(startCmd, cancellationToken);
        leave.Submit(instanceId);
    }
}

/// <summary>
/// 请假申请工作流变量（存入 Variables JSON）
/// </summary>
public class LeaveRequestVariables
{
    public string LeaveRequestId { get; set; } = string.Empty;
    public int LeaveType { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public decimal Days { get; set; }
    public string Reason { get; set; } = string.Empty;
}
