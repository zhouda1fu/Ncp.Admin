/**
 * 权限码常量定义
 * 与后端 PermissionCodes.cs 保持一致（平台脚手架）
 */
export const PermissionCodes = {
  AllApiAccess: 'AllApiAccess',

  SystemModule: 'SystemModule',

  CommonDataAccess: 'CommonDataAccess',
  RoleOptionView: 'RoleOptionView',
  UserOptionView: 'UserOptionView',
  DeptOptionView: 'DeptOptionView',
  PositionOptionView: 'PositionOptionView',
  FileAccess: 'FileAccess',

  RoleManagement: 'RoleManagement',
  RoleCreate: 'RoleCreate',
  RoleEdit: 'RoleEdit',
  RoleDelete: 'RoleDelete',
  RoleView: 'RoleView',
  RoleUpdatePermissions: 'RoleUpdatePermissions',

  UserManagement: 'UserManagement',
  UserCreate: 'UserCreate',
  UserEdit: 'UserEdit',
  UserDelete: 'UserDelete',
  UserView: 'UserView',
  UserRoleAssign: 'UserRoleAssign',
  UserResetPassword: 'UserResetPassword',
  UserExport: 'UserExport',
  UserImport: 'UserImport',
  UserChangeHistoryView: 'UserChangeHistoryView',

  DeptManagement: 'DeptManagement',
  DeptCreate: 'DeptCreate',
  DeptEdit: 'DeptEdit',
  DeptDelete: 'DeptDelete',
  DeptView: 'DeptView',

  PositionManagement: 'PositionManagement',
  PositionCreate: 'PositionCreate',
  PositionEdit: 'PositionEdit',
  PositionDelete: 'PositionDelete',
  PositionView: 'PositionView',

  WorkflowManagement: 'WorkflowManagement',
  WorkflowDefinitionView: 'WorkflowDefinitionView',
  WorkflowDefinitionCreate: 'WorkflowDefinitionCreate',
  WorkflowDefinitionEdit: 'WorkflowDefinitionEdit',
  WorkflowDefinitionDelete: 'WorkflowDefinitionDelete',
  WorkflowDefinitionDeletePublished: 'WorkflowDefinitionDeletePublished',
  WorkflowDefinitionPublish: 'WorkflowDefinitionPublish',
  WorkflowStart: 'WorkflowStart',
  WorkflowCancel: 'WorkflowCancel',
  WorkflowTaskApprove: 'WorkflowTaskApprove',
  WorkflowInstanceView: 'WorkflowInstanceView',
  WorkflowMonitor: 'WorkflowMonitor',

  NotificationManagement: 'NotificationManagement',
  NotificationView: 'NotificationView',
  NotificationSend: 'NotificationSend',

  OperationLogManagement: 'OperationLogManagement',
  OperationLogView: 'OperationLogView',

  SystemLogManagement: 'SystemLogManagement',
  SystemLogView: 'SystemLogView',

  BackgroundJobManagement: 'BackgroundJobManagement',
  BackgroundJobView: 'BackgroundJobView',
  BackgroundJobTrigger: 'BackgroundJobTrigger',

  HomeDashboard: 'HomeDashboard',
} as const;
