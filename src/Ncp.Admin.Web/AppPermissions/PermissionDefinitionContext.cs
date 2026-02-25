using System.Collections.Immutable;

namespace Ncp.Admin.Web.AppPermissions;

/// <summary>
/// 管理权限定义的上下文类，负责初始化和提供权限组及其权限项。
/// </summary>
public static class PermissionDefinitionContext
{
    // 存储权限组的字典，键为权限组名称，值为权限组对象
    private static Dictionary<string, AppPermissionGroup> Groups { get; } = new();

    // 静态构造函数，在类初始化时创建默认的权限组和权限项
    static PermissionDefinitionContext()
    {
        var systemAccess = AddGroup("SystemAccess");
        
        // 用户管理权限
        var adminUserManagement = systemAccess.AddPermission(PermissionCodes.UserManagement, "用户管理");
        adminUserManagement.AddChild(PermissionCodes.UserCreate, "创建用户");
        adminUserManagement.AddChild(PermissionCodes.UserEdit, "编辑用户");
        adminUserManagement.AddChild(PermissionCodes.UserDelete, "删除用户");
        adminUserManagement.AddChild(PermissionCodes.UserView, "查看用户");
        adminUserManagement.AddChild(PermissionCodes.UserRoleAssign, "分配用户角色");
        adminUserManagement.AddChild(PermissionCodes.UserResetPassword, "重置用户密码");
        
        // 角色管理权限
        var roleManagement = systemAccess.AddPermission(PermissionCodes.RoleManagement, "角色管理");
        roleManagement.AddChild(PermissionCodes.RoleCreate, "创建角色");
        roleManagement.AddChild(PermissionCodes.RoleEdit, "编辑角色");
        roleManagement.AddChild(PermissionCodes.RoleDelete, "删除角色");
        roleManagement.AddChild(PermissionCodes.RoleView, "查看角色");
        roleManagement.AddChild(PermissionCodes.RoleUpdatePermissions, "更新角色权限");

        // 部门管理权限
        var deptManagement = systemAccess.AddPermission(PermissionCodes.DeptManagement, "部门管理");
        deptManagement.AddChild(PermissionCodes.DeptCreate, "创建部门");
        deptManagement.AddChild(PermissionCodes.DeptEdit, "编辑部门");
        deptManagement.AddChild(PermissionCodes.DeptDelete, "删除部门");
        deptManagement.AddChild(PermissionCodes.DeptView, "查看部门");

        // 工作流管理权限
        var workflowManagement = systemAccess.AddPermission(PermissionCodes.WorkflowManagement, "工作流管理");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionView, "查看流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionCreate, "创建流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionEdit, "编辑流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionDelete, "删除流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowDefinitionPublish, "发布流程定义");
        workflowManagement.AddChild(PermissionCodes.WorkflowStart, "发起流程");
        workflowManagement.AddChild(PermissionCodes.WorkflowCancel, "撤销流程");
        workflowManagement.AddChild(PermissionCodes.WorkflowTaskApprove, "审批任务");
        workflowManagement.AddChild(PermissionCodes.WorkflowInstanceView, "查看流程实例");
        workflowManagement.AddChild(PermissionCodes.WorkflowMonitor, "流程监控");

        // 岗位管理权限
        var positionManagement = systemAccess.AddPermission(PermissionCodes.PositionManagement, "岗位管理");
        positionManagement.AddChild(PermissionCodes.PositionCreate, "创建岗位");
        positionManagement.AddChild(PermissionCodes.PositionEdit, "编辑岗位");
        positionManagement.AddChild(PermissionCodes.PositionDelete, "删除岗位");
        positionManagement.AddChild(PermissionCodes.PositionView, "查看岗位");

        // 通知管理权限
        var notificationManagement = systemAccess.AddPermission(PermissionCodes.NotificationManagement, "通知管理");
        notificationManagement.AddChild(PermissionCodes.NotificationView, "查看通知");
        notificationManagement.AddChild(PermissionCodes.NotificationSend, "发送通知");

        // 公告管理权限
        var announcementManagement = systemAccess.AddPermission(PermissionCodes.AnnouncementManagement, "公告管理");
        announcementManagement.AddChild(PermissionCodes.AnnouncementView, "查看公告");
        announcementManagement.AddChild(PermissionCodes.AnnouncementCreate, "创建公告");
        announcementManagement.AddChild(PermissionCodes.AnnouncementEdit, "编辑公告");
        announcementManagement.AddChild(PermissionCodes.AnnouncementPublish, "发布公告");

        // 会议/预订管理权限
        var meetingManagement = systemAccess.AddPermission(PermissionCodes.MeetingManagement, "会议管理");
        meetingManagement.AddChild(PermissionCodes.MeetingRoomView, "查看会议室");
        meetingManagement.AddChild(PermissionCodes.MeetingRoomEdit, "管理会议室");
        meetingManagement.AddChild(PermissionCodes.MeetingBookingView, "查看预订");
        meetingManagement.AddChild(PermissionCodes.MeetingBookingCreate, "预订会议室");

        // 报销管理权限
        var expenseManagement = systemAccess.AddPermission(PermissionCodes.ExpenseManagement, "报销管理");
        expenseManagement.AddChild(PermissionCodes.ExpenseClaimView, "查看报销单");
        expenseManagement.AddChild(PermissionCodes.ExpenseClaimCreate, "创建报销单");
        expenseManagement.AddChild(PermissionCodes.ExpenseClaimSubmit, "提交报销单");

        // 考勤管理权限
        var attendanceManagement = systemAccess.AddPermission(PermissionCodes.AttendanceManagement, "考勤管理");
        attendanceManagement.AddChild(PermissionCodes.AttendanceRecordView, "查看考勤记录");
        attendanceManagement.AddChild(PermissionCodes.AttendanceCheckIn, "打卡/签退");
        attendanceManagement.AddChild(PermissionCodes.ScheduleView, "查看排班");
        attendanceManagement.AddChild(PermissionCodes.ScheduleEdit, "编辑排班");

        // 请假管理权限
        var leaveManagement = systemAccess.AddPermission(PermissionCodes.LeaveManagement, "请假管理");
        leaveManagement.AddChild(PermissionCodes.LeaveRequestView, "查看请假申请");
        leaveManagement.AddChild(PermissionCodes.LeaveRequestCreate, "创建请假申请");
        leaveManagement.AddChild(PermissionCodes.LeaveRequestEdit, "编辑请假申请");
        leaveManagement.AddChild(PermissionCodes.LeaveRequestSubmit, "提交请假申请");
        leaveManagement.AddChild(PermissionCodes.LeaveRequestCancel, "撤销请假申请");
        leaveManagement.AddChild(PermissionCodes.LeaveBalanceView, "查看请假余额");
        leaveManagement.AddChild(PermissionCodes.LeaveBalanceEdit, "设置请假余额");

        // 任务/项目管理权限
        var taskManagement = systemAccess.AddPermission(PermissionCodes.TaskManagement, "任务管理");
        taskManagement.AddChild(PermissionCodes.ProjectView, "查看项目");
        taskManagement.AddChild(PermissionCodes.ProjectCreate, "创建项目");
        taskManagement.AddChild(PermissionCodes.ProjectEdit, "编辑项目");
        taskManagement.AddChild(PermissionCodes.TaskView, "查看任务");
        taskManagement.AddChild(PermissionCodes.TaskCreate, "创建任务");
        taskManagement.AddChild(PermissionCodes.TaskEdit, "编辑任务");

        // 文档管理权限
        var documentManagement = systemAccess.AddPermission(PermissionCodes.DocumentManagement, "文档管理");
        documentManagement.AddChild(PermissionCodes.DocumentView, "查看文档");
        documentManagement.AddChild(PermissionCodes.DocumentCreate, "上传文档");
        documentManagement.AddChild(PermissionCodes.DocumentEdit, "编辑文档/添加版本");
        documentManagement.AddChild(PermissionCodes.DocumentShare, "创建共享链接");

        // 即时通讯权限
        var chatManagement = systemAccess.AddPermission(PermissionCodes.ChatManagement, "即时通讯");
        chatManagement.AddChild(PermissionCodes.ChatView, "查看会话与消息");
        chatManagement.AddChild(PermissionCodes.ChatCreate, "创建会话");

        // 通讯录管理权限
        var contactManagement = systemAccess.AddPermission(PermissionCodes.ContactManagement, "通讯录管理");
        contactManagement.AddChild(PermissionCodes.ContactGroupView, "查看联系组");
        contactManagement.AddChild(PermissionCodes.ContactGroupCreate, "创建联系组");
        contactManagement.AddChild(PermissionCodes.ContactGroupEdit, "编辑联系组");
        contactManagement.AddChild(PermissionCodes.ContactView, "查看联系人");
        contactManagement.AddChild(PermissionCodes.ContactCreate, "创建联系人");
        contactManagement.AddChild(PermissionCodes.ContactEdit, "编辑联系人");

        // 所有接口访问权限
        var allApiAccess = systemAccess.AddPermission(PermissionCodes.AllApiAccess, "所有接口访问权限");
    }

    /// <summary>
    /// 添加一个新的权限组，如果权限组名称已存在则抛出异常。
    /// </summary>
    /// <param name="name">权限组名称</param>
    /// <returns>返回创建的权限组</returns>
    /// <exception cref="ArgumentException">如果权限组名称已经存在，则抛出异常</exception>
    private static AppPermissionGroup AddGroup(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (Groups.ContainsKey(name))
        {
            throw new ArgumentException($"There is already an existing permission group with name: {name}");
        }

        return Groups[name] = new AppPermissionGroup(name);
    }

    /// <summary>
    /// 获取所有的权限组。
    /// </summary>
    public static IReadOnlyList<AppPermissionGroup> PermissionGroups => Groups.Values.ToImmutableList();
}

