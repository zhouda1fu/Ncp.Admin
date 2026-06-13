import { PermissionCodes } from './permission-codes';

export type ModuleCategoryId = 'system';

export interface ModuleCategoryDefinition {
  id: ModuleCategoryId;
  icon: string;
  order: number;
  permissionCode: string;
  title: string;
}

export const MODULE_CATEGORY_DEFINITIONS: ModuleCategoryDefinition[] = [
  {
    id: 'system',
    title: '系统模块',
    icon: 'mdi:cog-outline',
    order: 1,
    permissionCode: PermissionCodes.SystemModule,
  },
];

export const MODULE_MENU_DEFAULT_EXPANDED_PATHS: string[] =
  MODULE_CATEGORY_DEFINITIONS.map((item) => `/${item.permissionCode}`);

export const MODULE_TOP_LEVEL_PERMISSION_ROOTS = [] as const;

/** 有权限定义但无侧栏菜单入口，不参与模块分组展示判定 */
export const MODULE_CATEGORY_NON_MENU_ROOT_PERMISSIONS = [
  PermissionCodes.NotificationManagement,
  PermissionCodes.HomeDashboard,
] as const;

const MODULE_CATEGORY_NON_MENU_ROOT_PERMISSION_SET = new Set<string>(
  MODULE_CATEGORY_NON_MENU_ROOT_PERMISSIONS,
);

export function isModuleCategoryNonMenuRootPermission(code: string): boolean {
  return MODULE_CATEGORY_NON_MENU_ROOT_PERMISSION_SET.has(code);
}

export const SYSTEM_GROUP_VALUE_PREFIX = '__system_group__:';

export const PERMISSION_ROOT_CATEGORY_MAP: Record<string, ModuleCategoryId> = {
  [PermissionCodes.RoleManagement]: 'system',
  [PermissionCodes.UserManagement]: 'system',
  [PermissionCodes.DeptManagement]: 'system',
  [PermissionCodes.PositionManagement]: 'system',
  [PermissionCodes.WorkflowManagement]: 'system',
  [PermissionCodes.NotificationManagement]: 'system',
  [PermissionCodes.OperationLogManagement]: 'system',
  [PermissionCodes.SystemLogManagement]: 'system',
  [PermissionCodes.BackgroundJobManagement]: 'system',
  [PermissionCodes.HomeDashboard]: 'system',
};

export function isSyntheticPermissionTreeKey(value: string): boolean {
  return value.startsWith(SYSTEM_GROUP_VALUE_PREFIX);
}

export function stripSyntheticPermissionTreeKeys(codes: string[]): string[] {
  return codes.filter((code) => !isSyntheticPermissionTreeKey(code));
}
