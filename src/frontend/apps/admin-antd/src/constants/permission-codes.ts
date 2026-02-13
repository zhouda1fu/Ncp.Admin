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

  // 所有接口访问权限
  AllApiAccess: 'AllApiAccess',
} as const;
