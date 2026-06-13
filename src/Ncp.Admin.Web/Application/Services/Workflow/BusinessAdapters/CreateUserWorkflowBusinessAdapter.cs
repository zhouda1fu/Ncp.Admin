using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Queries;
using Serilog;

namespace Ncp.Admin.Web.Application.Services.Workflow.BusinessAdapters;

/// <summary>
/// 创建用户审批工作流适配器：审批通过后根据流程变量创建系统用户。
/// </summary>
public class CreateUserWorkflowBusinessAdapter(
    IMediator mediator,
    RoleQuery roleQuery)
    : IWorkflowBusinessAdapter
{
    /// <inheritdoc />
    public string BusinessType => WorkflowBusinessTypes.CreateUser;

    /// <summary>
    /// 创建用户流程可用于条件分支的变量字段，字段名需与 <see cref="CreateUserVariables"/> 保持一致。
    /// </summary>
    public IReadOnlyList<ConditionFieldDto> GetConditionFields()
    {
        return
        [
            new ConditionFieldDto("Name", "用户名", "string"),
            new ConditionFieldDto("Email", "邮箱", "string"),
            new ConditionFieldDto("RealName", "真实姓名", "string"),
            new ConditionFieldDto("Phone", "手机号", "string"),
            new ConditionFieldDto("Status", "状态", "number"),
            new ConditionFieldDto("Gender", "性别", "string"),
            new ConditionFieldDto("DeptId", "部门ID", "string"),
            new ConditionFieldDto("DeptName", "部门名称", "string"),
        ];
    }

    /// <summary>
    /// 审批通过后反序列化用户变量并发送创建用户命令；业务执行失败时标记工作流异常。
    /// </summary>
    public async Task OnCompletedAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        try
        {
            var userData = JsonSerializer.Deserialize<CreateUserVariables>(instance.Variables, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (userData == null)
            {
                throw new KnownException(
                    $"创建用户工作流变量为空，InstanceId={instance.Id}",
                    ErrorCodes.WorkflowBusinessCallbackFailed);
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
                userData.ResignedTime,
                userData.AttendanceRequired,
                userData.SetAsDeptResponsibleUser,
                userData.SetAsDefaultDeptResponsibleUser);

            await mediator.Send(cmd, cancellationToken);

        }
        catch (Exception ex)
        {
            await mediator.Send(new MarkWorkflowInstanceFaultedCommand(instance.Id, ex.Message), cancellationToken);
        }
    }
}

/// <summary>
/// 创建用户的工作流变量结构（前端序列化后存入 workflow variables）。
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
    public string? PositionId { get; set; }
    public string? PositionName { get; set; }
    public string[] RoleIds { get; set; } = [];
    public string IdCardNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string GraduateSchool { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool NotOrderMeal { get; set; }
    public bool AttendanceRequired { get; set; } = true;
    public string WechatGuid { get; set; } = string.Empty;
    public bool IsResigned { get; set; }
    public DateTimeOffset ResignedTime { get; set; }
    public bool SetAsDeptResponsibleUser { get; set; }
    public bool SetAsDefaultDeptResponsibleUser { get; set; }
}
