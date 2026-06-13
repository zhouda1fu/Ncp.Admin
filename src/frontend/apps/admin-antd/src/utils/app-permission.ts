import { PermissionCodes } from '#/constants/permission-codes';

/**
 * 是否拥有指定应用权限码（须角色授权树中显式勾选该码）。
 * 不因父级分组码或 AllApiAccess 隐式放行。
 */
export function hasExplicitAppPermission(
  accessCodes: readonly string[] | undefined,
  code: string,
): boolean {
  if (!code || !accessCodes?.length) {
    return false;
  }
  return accessCodes.includes(code);
}

/**
 * 与后端 HasAnyAppPermission(AllApiAccess, code) 一致：拥有 AllApiAccess 或显式权限码即放行。
 */
export function hasAppPermission(
  accessCodes: readonly string[] | undefined,
  code: string,
): boolean {
  if (!code || !accessCodes?.length) {
    return false;
  }
  if (accessCodes.includes(PermissionCodes.AllApiAccess)) {
    return true;
  }
  return accessCodes.includes(code);
}