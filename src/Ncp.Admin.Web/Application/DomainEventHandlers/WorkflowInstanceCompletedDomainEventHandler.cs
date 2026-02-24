using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Application.Commands.Notification;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;
using Serilog;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例完成领域事件处理器
/// 根据 BusinessType 自动执行对应的业务操作
/// </summary>
public class WorkflowInstanceCompletedDomainEventHandler(
    IMediator mediator,
    RoleQuery roleQuery,
    ILeaveRequestRepository leaveRequestRepository,
    ILeaveBalanceRepository leaveBalanceRepository,
    ApplicationDbContext dbContext) : IDomainEventHandler<WorkflowInstanceCompletedDomainEvent>
{
    public async Task Handle(WorkflowInstanceCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        // 只处理审批通过（Completed）的流程
        if (instance.Status != WorkflowInstanceStatus.Completed)
        {
            return;
        }

        Log.Information("工作流实例审批通过，开始执行业务逻辑: InstanceId={InstanceId}, BusinessType={BusinessType}",
            instance.Id, instance.BusinessType);

        // 向发起人发送审批通过通知
        await mediator.Send(
            new SendNotificationCommand(
                "您的流程已审批通过",
                $"流程「{instance.Title}」已审批通过，系统将自动执行后续操作。",
                NotificationType.Workflow,
                NotificationLevel.Success,
                null,
                string.Empty,
                instance.InitiatorId.Id,
                instance.Id.ToString(),
                "WorkflowInstance"),
            cancellationToken);

        switch (instance.BusinessType)
        {
            case "CreateUser":
                await HandleCreateUser(instance, cancellationToken);
                break;
            case "LeaveRequest":
                await HandleLeaveRequest(instance, cancellationToken);
                break;
            default:
                Log.Warning("未知的工作流业务类型: {BusinessType}", instance.BusinessType);
                break;
        }
    }

    /// <summary>
    /// 处理「请假申请」审批通过：更新请假单状态并扣减余额
    /// </summary>
    private async Task HandleLeaveRequest(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(instance.BusinessKey, out var leaveIdGuid))
        {
            Log.Error("请假申请ID无效: BusinessKey={BusinessKey}", instance.BusinessKey);
            return;
        }

        var leave = await leaveRequestRepository.GetAsync(new LeaveRequestId(leaveIdGuid), cancellationToken);
        if (leave == null)
        {
            Log.Error("未找到请假申请: LeaveRequestId={Id}", leaveIdGuid);
            return;
        }

        leave.Approve();

        var year = leave.StartDate.Year;
        var balance = await leaveBalanceRepository.GetByUserYearTypeAsync(leave.ApplicantId, year, leave.LeaveType, cancellationToken);
        if (balance != null)
        {
            balance.Deduct(leave.Days);
            Log.Information("请假审批通过，已扣减余额: LeaveRequestId={Id}, UserId={UserId}, LeaveType={Type}, Days={Days}",
                leave.Id, leave.ApplicantId, leave.LeaveType, leave.Days);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 处理「新增用户」审批通过 —— 反序列化 Variables 并调用 CreateUserCommand
    /// </summary>
    private async Task HandleCreateUser(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        try
        {
            var userData = JsonSerializer.Deserialize<CreateUserVariables>(instance.Variables, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (userData == null)
            {
                Log.Error("工作流变量反序列化失败: InstanceId={InstanceId}", instance.Id);
                return;
            }

            // 查询角色信息（与 CreateUserEndpoint 模式一致）
            var rolesToBeAssigned = userData.RoleIds.Any()
                ? await roleQuery.GetAdminRolesForAssignmentAsync(
                    userData.RoleIds.Select(id => new Domain.AggregatesModel.RoleAggregate.RoleId(Guid.Parse(id))),
                    cancellationToken)
                : [];

            var cmd = new CreateUserCommand(
                userData.Name,
                userData.Email,
                userData.Password,
                userData.Phone ?? string.Empty,
                userData.RealName,
                userData.Status,
                userData.Gender ?? string.Empty,
                userData.BirthDate,
                !string.IsNullOrEmpty(userData.DeptId) ? new DeptId(long.Parse(userData.DeptId)) : null,
                userData.DeptName,
                rolesToBeAssigned);

            var userId = await mediator.Send(cmd, cancellationToken);

            Log.Information("工作流审批通过，用户创建成功: InstanceId={InstanceId}, UserId={UserId}, UserName={UserName}",
                instance.Id, userId, userData.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "工作流审批通过后创建用户失败: InstanceId={InstanceId}", instance.Id);
            await mediator.Send(new MarkWorkflowInstanceFaultedCommand(instance.Id, ex.Message), cancellationToken);
        }
    }
}

/// <summary>
/// 创建用户的工作流变量结构
/// 前端序列化后存入 workflow variables
/// </summary>
public class CreateUserVariables
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string RealName { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
    public string? Gender { get; set; }
    public DateTimeOffset BirthDate { get; set; }
    public string? DeptId { get; set; }
    public string? DeptName { get; set; }
    public string[] RoleIds { get; set; } = [];
}
