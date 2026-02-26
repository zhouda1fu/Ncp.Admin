namespace Ncp.Admin.Web.AppPermissions;

/// <summary>
/// 权限常量定义
/// </summary>
public static class PermissionCodes
{
    #region 角色管理权限
    public const string RoleManagement = nameof(RoleManagement);
    public const string RoleCreate = nameof(RoleCreate);
    public const string RoleEdit = nameof(RoleEdit);
    public const string RoleDelete = nameof(RoleDelete);
    public const string RoleView = nameof(RoleView);
    public const string RoleUpdatePermissions = nameof(RoleUpdatePermissions);
    #endregion

    #region 用户管理权限
    public const string UserManagement = nameof(UserManagement);
    public const string UserCreate = nameof(UserCreate);
    public const string UserEdit = nameof(UserEdit);
    public const string UserDelete = nameof(UserDelete);
    public const string UserView = nameof(UserView);
    public const string UserRoleAssign = nameof(UserRoleAssign);
    public const string UserResetPassword = nameof(UserResetPassword);
    #endregion

    #region 部门管理权限
    public const string DeptManagement = nameof(DeptManagement);
    public const string DeptCreate = nameof(DeptCreate);
    public const string DeptEdit = nameof(DeptEdit);
    public const string DeptDelete = nameof(DeptDelete);
    public const string DeptView = nameof(DeptView);
    #endregion

    #region 工作流管理权限
    public const string WorkflowManagement = nameof(WorkflowManagement);
    public const string WorkflowDefinitionView = nameof(WorkflowDefinitionView);
    public const string WorkflowDefinitionCreate = nameof(WorkflowDefinitionCreate);
    public const string WorkflowDefinitionEdit = nameof(WorkflowDefinitionEdit);
    public const string WorkflowDefinitionDelete = nameof(WorkflowDefinitionDelete);
    public const string WorkflowDefinitionPublish = nameof(WorkflowDefinitionPublish);
    public const string WorkflowStart = nameof(WorkflowStart);
    public const string WorkflowCancel = nameof(WorkflowCancel);
    public const string WorkflowTaskApprove = nameof(WorkflowTaskApprove);
    public const string WorkflowInstanceView = nameof(WorkflowInstanceView);
    public const string WorkflowMonitor = nameof(WorkflowMonitor);
    #endregion

    #region 岗位管理权限
    public const string PositionManagement = nameof(PositionManagement);
    public const string PositionCreate = nameof(PositionCreate);
    public const string PositionEdit = nameof(PositionEdit);
    public const string PositionDelete = nameof(PositionDelete);
    public const string PositionView = nameof(PositionView);
    #endregion

    #region 通知管理权限
    public const string NotificationManagement = nameof(NotificationManagement);
    public const string NotificationView = nameof(NotificationView);
    public const string NotificationSend = nameof(NotificationSend);
    #endregion

    #region 公告管理权限
    public const string AnnouncementManagement = nameof(AnnouncementManagement);
    public const string AnnouncementView = nameof(AnnouncementView);
    public const string AnnouncementCreate = nameof(AnnouncementCreate);
    public const string AnnouncementEdit = nameof(AnnouncementEdit);
    public const string AnnouncementPublish = nameof(AnnouncementPublish);
    #endregion

    #region 会议/预订管理权限
    public const string MeetingManagement = nameof(MeetingManagement);
    public const string MeetingRoomView = nameof(MeetingRoomView);
    public const string MeetingRoomEdit = nameof(MeetingRoomEdit);
    public const string MeetingBookingView = nameof(MeetingBookingView);
    public const string MeetingBookingCreate = nameof(MeetingBookingCreate);
    #endregion

    #region 报销管理权限
    public const string ExpenseManagement = nameof(ExpenseManagement);
    public const string ExpenseClaimView = nameof(ExpenseClaimView);
    public const string ExpenseClaimCreate = nameof(ExpenseClaimCreate);
    public const string ExpenseClaimSubmit = nameof(ExpenseClaimSubmit);
    #endregion

    #region 考勤管理权限
    public const string AttendanceManagement = nameof(AttendanceManagement);
    public const string AttendanceRecordView = nameof(AttendanceRecordView);
    public const string AttendanceCheckIn = nameof(AttendanceCheckIn);
    public const string ScheduleView = nameof(ScheduleView);
    public const string ScheduleEdit = nameof(ScheduleEdit);
    #endregion

    #region 任务/项目管理权限
    public const string TaskManagement = nameof(TaskManagement);
    public const string ProjectView = nameof(ProjectView);
    public const string ProjectCreate = nameof(ProjectCreate);
    public const string ProjectEdit = nameof(ProjectEdit);
    public const string TaskView = nameof(TaskView);
    public const string TaskCreate = nameof(TaskCreate);
    public const string TaskEdit = nameof(TaskEdit);
    #endregion

    #region 文档管理权限
    public const string DocumentManagement = nameof(DocumentManagement);
    public const string DocumentView = nameof(DocumentView);
    public const string DocumentCreate = nameof(DocumentCreate);
    public const string DocumentEdit = nameof(DocumentEdit);
    public const string DocumentShare = nameof(DocumentShare);
    #endregion

    #region 即时通讯权限
    public const string ChatManagement = nameof(ChatManagement);
    public const string ChatView = nameof(ChatView);
    public const string ChatCreate = nameof(ChatCreate);
    #endregion

    #region 通讯录管理权限
    public const string ContactManagement = nameof(ContactManagement);
    public const string ContactGroupView = nameof(ContactGroupView);
    public const string ContactGroupCreate = nameof(ContactGroupCreate);
    public const string ContactGroupEdit = nameof(ContactGroupEdit);
    public const string ContactView = nameof(ContactView);
    public const string ContactCreate = nameof(ContactCreate);
    public const string ContactEdit = nameof(ContactEdit);
    #endregion

    #region 资产管理权限
    public const string AssetManagement = nameof(AssetManagement);
    public const string AssetView = nameof(AssetView);
    public const string AssetCreate = nameof(AssetCreate);
    public const string AssetEdit = nameof(AssetEdit);
    public const string AssetAllocate = nameof(AssetAllocate);
    public const string AssetReturn = nameof(AssetReturn);
    public const string AssetScrap = nameof(AssetScrap);
    public const string AssetAllocationView = nameof(AssetAllocationView);
    #endregion

    #region 车辆管理权限
    public const string VehicleManagement = nameof(VehicleManagement);
    public const string VehicleView = nameof(VehicleView);
    public const string VehicleEdit = nameof(VehicleEdit);
    public const string VehicleBookingView = nameof(VehicleBookingView);
    public const string VehicleBookingCreate = nameof(VehicleBookingCreate);
    public const string VehicleBookingCancel = nameof(VehicleBookingCancel);
    public const string VehicleBookingComplete = nameof(VehicleBookingComplete);
    #endregion

    #region 合同管理权限
    public const string ContractManagement = nameof(ContractManagement);
    public const string ContractView = nameof(ContractView);
    public const string ContractCreate = nameof(ContractCreate);
    public const string ContractEdit = nameof(ContractEdit);
    public const string ContractSubmit = nameof(ContractSubmit);
    public const string ContractApprove = nameof(ContractApprove);
    public const string ContractArchive = nameof(ContractArchive);
    #endregion

    #region 客户管理权限
    public const string CustomerManagement = nameof(CustomerManagement);
    public const string CustomerView = nameof(CustomerView);
    public const string CustomerCreate = nameof(CustomerCreate);
    public const string CustomerEdit = nameof(CustomerEdit);
    public const string CustomerDelete = nameof(CustomerDelete);
    public const string CustomerContactEdit = nameof(CustomerContactEdit);
    public const string CustomerReleaseToSea = nameof(CustomerReleaseToSea);
    public const string CustomerClaimFromSea = nameof(CustomerClaimFromSea);
    public const string IndustryView = nameof(IndustryView);
    #endregion

    #region 请假管理权限
    public const string LeaveManagement = nameof(LeaveManagement);
    public const string LeaveRequestView = nameof(LeaveRequestView);
    public const string LeaveRequestCreate = nameof(LeaveRequestCreate);
    public const string LeaveRequestEdit = nameof(LeaveRequestEdit);
    public const string LeaveRequestSubmit = nameof(LeaveRequestSubmit);
    public const string LeaveRequestCancel = nameof(LeaveRequestCancel);
    public const string LeaveBalanceView = nameof(LeaveBalanceView);
    public const string LeaveBalanceEdit = nameof(LeaveBalanceEdit);
    #endregion

    #region 所有接口访问权限
    public const string AllApiAccess = nameof(AllApiAccess);
    #endregion
}

