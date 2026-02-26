/**
 * 权限码常量定义
 * 与后端 PermissionCodes.cs 保持一致
 */
export const PermissionCodes = {
  // 角色管理权限
  RoleManagement: 'RoleManagement',
  RoleCreate: 'RoleCreate',
  RoleEdit: 'RoleEdit',
  RoleDelete: 'RoleDelete',
  RoleView: 'RoleView',
  RoleUpdatePermissions: 'RoleUpdatePermissions',

  // 用户管理权限
  UserManagement: 'UserManagement',
  UserCreate: 'UserCreate',
  UserEdit: 'UserEdit',
  UserDelete: 'UserDelete',
  UserView: 'UserView',
  UserRoleAssign: 'UserRoleAssign',
  UserResetPassword: 'UserResetPassword',

  // 部门管理权限
  DeptManagement: 'DeptManagement',
  DeptCreate: 'DeptCreate',
  DeptEdit: 'DeptEdit',
  DeptDelete: 'DeptDelete',
  DeptView: 'DeptView',

  // 岗位管理权限
  PositionManagement: 'PositionManagement',
  PositionCreate: 'PositionCreate',
  PositionEdit: 'PositionEdit',
  PositionDelete: 'PositionDelete',
  PositionView: 'PositionView',

  // 工作流管理权限
  WorkflowManagement: 'WorkflowManagement',
  WorkflowDefinitionView: 'WorkflowDefinitionView',
  WorkflowDefinitionCreate: 'WorkflowDefinitionCreate',
  WorkflowDefinitionEdit: 'WorkflowDefinitionEdit',
  WorkflowDefinitionDelete: 'WorkflowDefinitionDelete',
  WorkflowDefinitionPublish: 'WorkflowDefinitionPublish',
  WorkflowStart: 'WorkflowStart',
  WorkflowCancel: 'WorkflowCancel',
  WorkflowTaskApprove: 'WorkflowTaskApprove',
  WorkflowInstanceView: 'WorkflowInstanceView',
  WorkflowMonitor: 'WorkflowMonitor',

  // 通知管理权限
  NotificationManagement: 'NotificationManagement',
  NotificationView: 'NotificationView',
  NotificationSend: 'NotificationSend',

  // 报销管理权限
  ExpenseManagement: 'ExpenseManagement',
  ExpenseClaimView: 'ExpenseClaimView',
  ExpenseClaimCreate: 'ExpenseClaimCreate',
  ExpenseClaimSubmit: 'ExpenseClaimSubmit',

  // 会议管理权限
  MeetingManagement: 'MeetingManagement',
  MeetingRoomView: 'MeetingRoomView',
  MeetingRoomEdit: 'MeetingRoomEdit',
  MeetingBookingView: 'MeetingBookingView',
  MeetingBookingCreate: 'MeetingBookingCreate',

  // 资产管理权限
  AssetManagement: 'AssetManagement',
  AssetView: 'AssetView',
  AssetCreate: 'AssetCreate',
  AssetEdit: 'AssetEdit',
  AssetAllocate: 'AssetAllocate',
  AssetReturn: 'AssetReturn',
  AssetScrap: 'AssetScrap',
  AssetAllocationView: 'AssetAllocationView',

  // 车辆管理权限
  VehicleManagement: 'VehicleManagement',
  VehicleView: 'VehicleView',
  VehicleEdit: 'VehicleEdit',
  VehicleBookingView: 'VehicleBookingView',
  VehicleBookingCreate: 'VehicleBookingCreate',
  VehicleBookingCancel: 'VehicleBookingCancel',
  VehicleBookingComplete: 'VehicleBookingComplete',

  // 合同管理权限
  ContractManagement: 'ContractManagement',
  ContractView: 'ContractView',
  ContractCreate: 'ContractCreate',
  ContractEdit: 'ContractEdit',
  ContractSubmit: 'ContractSubmit',
  ContractApprove: 'ContractApprove',
  ContractArchive: 'ContractArchive',

  // 考勤管理权限
  AttendanceManagement: 'AttendanceManagement',
  AttendanceRecordView: 'AttendanceRecordView',
  AttendanceCheckIn: 'AttendanceCheckIn',
  ScheduleView: 'ScheduleView',
  ScheduleEdit: 'ScheduleEdit',

  // 公告管理权限
  AnnouncementManagement: 'AnnouncementManagement',
  AnnouncementView: 'AnnouncementView',
  AnnouncementCreate: 'AnnouncementCreate',
  AnnouncementEdit: 'AnnouncementEdit',
  AnnouncementPublish: 'AnnouncementPublish',

  // 任务/项目管理权限
  TaskManagement: 'TaskManagement',
  ProjectView: 'ProjectView',
  ProjectCreate: 'ProjectCreate',
  ProjectEdit: 'ProjectEdit',
  TaskView: 'TaskView',
  TaskCreate: 'TaskCreate',
  TaskEdit: 'TaskEdit',

  // 即时通讯权限
  ChatManagement: 'ChatManagement',
  ChatView: 'ChatView',
  ChatCreate: 'ChatCreate',

  // 文档管理权限
  DocumentManagement: 'DocumentManagement',
  DocumentView: 'DocumentView',
  DocumentCreate: 'DocumentCreate',
  DocumentEdit: 'DocumentEdit',
  DocumentShare: 'DocumentShare',

  // 通讯录管理权限
  ContactManagement: 'ContactManagement',
  ContactGroupView: 'ContactGroupView',
  ContactGroupCreate: 'ContactGroupCreate',
  ContactGroupEdit: 'ContactGroupEdit',
  ContactView: 'ContactView',
  ContactCreate: 'ContactCreate',
  ContactEdit: 'ContactEdit',

  // 请假管理权限
  LeaveManagement: 'LeaveManagement',
  LeaveRequestView: 'LeaveRequestView',
  LeaveRequestCreate: 'LeaveRequestCreate',
  LeaveRequestEdit: 'LeaveRequestEdit',
  LeaveRequestSubmit: 'LeaveRequestSubmit',
  LeaveRequestCancel: 'LeaveRequestCancel',
  LeaveBalanceView: 'LeaveBalanceView',
  LeaveBalanceEdit: 'LeaveBalanceEdit',

  // 所有接口访问权限
  AllApiAccess: 'AllApiAccess',
} as const;
