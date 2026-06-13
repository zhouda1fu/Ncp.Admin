import type { SystemDeptApi } from '#/api/system/dept';

/** 与后端 DataScope 枚举一致 */
export type DataScope = 0 | 1 | 2 | 3 | 4;

export interface JwtDataPermissionContext {
  authorizedDeptIds: string[];
  deptId: string;
  scope: DataScope;
}

/** 与后端 Ncp.Admin.Domain.AggregatesModel.RoleAggregate.DataScope.Self 一致 */
export const DATA_SCOPE_SELF = 3;

export function parseJwtPayload(token: null | string): Record<string, unknown> | null {
  if (!token) return null;
  const parts = token.split('.');
  if (parts.length < 2) return null;
  try {
    const segment = parts[1]!.replace(/-/g, '+').replace(/_/g, '/');
    const padded = segment + '='.repeat((4 - (segment.length % 4)) % 4);
    const json = atob(padded);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export function getDataScopeFromAccessToken(token: null | string): number | null {
  const payload = parseJwtPayload(token);
  if (!payload) return null;
  const v = payload.data_scope;
  if (v === undefined || v === null) return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
}

export function getDataPermissionContextFromAccessToken(
  token: null | string,
): JwtDataPermissionContext {
  const payload = parseJwtPayload(token);
  const scopeValue = Number(payload?.data_scope ?? 0);
  const scope = (Number.isNaN(scopeValue) ? 0 : scopeValue) as DataScope;
  const deptId = String(payload?.dept_id ?? '').trim();
  const authorizedRaw = String(payload?.authorized_dept_ids ?? '').trim();
  const authorizedDeptIds = authorizedRaw
    ? authorizedRaw.split(',').map((item) => item.trim()).filter(Boolean)
    : [];
  return { scope, deptId, authorizedDeptIds };
}

/** 角色数据权限允许的部门 ID；scope 为全部数据时返回 null 表示不限制 */
export function getAllowedDeptIdSet(context: JwtDataPermissionContext): Set<string> | null {
  if (context.scope === 0) return null;
  const ids =
    context.authorizedDeptIds.length > 0
      ? context.authorizedDeptIds
      : context.deptId
        ? [context.deptId]
        : [];
  return new Set(ids.map(String));
}

export function filterDeptTreeByAllowed(
  nodes: SystemDeptApi.SystemDept[],
  allowed: Set<string> | null,
): SystemDeptApi.SystemDept[] {
  if (!allowed) return nodes;

  const result: SystemDeptApi.SystemDept[] = [];
  for (const node of nodes) {
    const children = node.children?.length ? filterDeptTreeByAllowed(node.children, allowed) : [];
    const selfIncluded = allowed.has(String(node.id));
    if (selfIncluded || children.length > 0) {
      result.push({ ...node, children });
    }
  }
  return result;
}

export function flattenDeptTreeToOptions(
  nodes: SystemDeptApi.SystemDept[],
  pathLabel = '',
): Array<{ label: string; value: string }> {
  const options: Array<{ label: string; value: string }> = [];
  for (const node of nodes) {
    const label = pathLabel ? `${pathLabel} / ${node.name}` : node.name;
    options.push({ value: String(node.id), label });
    if (node.children?.length) {
      options.push(...flattenDeptTreeToOptions(node.children, label));
    }
  }
  return options;
}
