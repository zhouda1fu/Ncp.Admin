using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Queries;
using Serilog;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例完成领域事件处理器：新增用户审批通过时创建用户
/// </summary>
public class WorkflowInstanceCompletedDomainEventHandlerForCreateUser(
    IMediator mediator,
    RoleQuery roleQuery)
    : IDomainEventHandler<WorkflowInstanceCompletedDomainEvent>
{
    public async Task Handle(WorkflowInstanceCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        if (instance.Status != WorkflowInstanceStatus.Completed || instance.BusinessType != WorkflowBusinessTypes.CreateUser)
            return;

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
                !string.IsNullOrEmpty(userData.DeptId) ? new Domain.AggregatesModel.DeptAggregate.DeptId(long.Parse(userData.DeptId)) : null,
                userData.DeptName,
                userData.IsDeptManager,
                userData.PositionId != null ? new Domain.AggregatesModel.PositionAggregate.PositionId(long.Parse(userData.PositionId)) : null,
                userData.PositionName,
                rolesToBeAssigned,
                instance.InitiatorId,
                userData.IdCardNumber,
                userData.Address,
                userData.Education,
                userData.GraduateSchool,
                userData.AvatarUrl,
                userData.NotOrderMeal,
                userData.WechatGuid,
                userData.IsResigned,
                userData.ResignedTime);

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
/// 创建用户的工作流变量结构（前端序列化后存入 workflow variables）
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
    public bool IsDeptManager { get; set; }
    public string? PositionId { get; set; }
    public string? PositionName { get; set; }
    public string[] RoleIds { get; set; } = [];
    public string IdCardNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string GraduateSchool { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool NotOrderMeal { get; set; }
    public string WechatGuid { get; set; } = string.Empty;
    public bool IsResigned { get; set; }
    public DateTimeOffset ResignedTime { get; set; }
}
