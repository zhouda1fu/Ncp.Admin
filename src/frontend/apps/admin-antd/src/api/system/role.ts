import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 数据权限范围：0=全部 1=本部门 2=本部门及下级 3=仅本人 */
export type DataScope = 0 | 1 | 2 | 3;

export namespace SystemRoleApi {
  export interface SystemRole {
    [key: string]: any;
    roleId: string;
    name: string;
    description: string;
    dataScope?: DataScope;
    isActive: boolean;
    createdAt: string;
    permissionCodes: string[];
  }
}

/**
 * 获取角色列表数据
 */
async function getRoleList(params: Recordable<any>) {
  const result = await requestClient.get<{
    items: Array<{
      roleId: string;
      name: string;
      description: string;
      dataScope?: DataScope;
      isActive: boolean;
      createdAt: string;
      permissionCodes: string[];
    }>;
    total: number;
    page: number;
    pageSize: number;
  }>('/roles', { params });
  return result;
}

/**
 * 获取单个角色信息
 * @param id 角色 ID
 */
async function getRole(id: string) {
  return requestClient.get<SystemRoleApi.SystemRole>(`/roles/${id}`);
}

/**
 * 创建角色
 * @param data 角色数据
 */
async function createRole(data: {
  name: string;
  description: string;
  dataScope?: DataScope;
  permissionCodes: string[];
}) {
  return requestClient.post('/roles', data);
}

/**
 * 更新角色
 *
 * @param id 角色 ID
 * @param data 角色数据
 */
async function updateRole(
  id: string,
  data: {
    name: string;
    description: string;
    dataScope?: DataScope;
    permissionCodes: string[];
  },
) {
  return requestClient.put('/roles/update', {
    roleId: id,
    ...data,
  });
}

/**
 * 删除角色
 * @param id 角色 ID
 */
async function deleteRole(id: string) {
  return requestClient.delete(`/roles/${id}`);
}

/**
 * 激活角色
 * @param id 角色 ID
 */
async function activateRole(id: string) {
  return requestClient.put('/roles/activate', {
    roleId: id,
  });
}

/**
 * 停用角色
 * @param id 角色 ID
 */
async function deactivateRole(id: string) {
  return requestClient.put('/roles/deactivate', {
    roleId: id,
  });
}

export {
  activateRole,
  createRole,
  deactivateRole,
  deleteRole,
  getRole,
  getRoleList,
  updateRole,
};
