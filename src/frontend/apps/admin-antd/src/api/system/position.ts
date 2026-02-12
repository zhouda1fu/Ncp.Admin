import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace SystemPositionApi {
  export interface PositionListItem {
    id: string;
    name: string;
    code: string;
    description: string;
    deptId: string;
    deptName?: string;
    sortOrder: number;
    status: number;
    createdAt: string;
  }

  export interface Position {
    id: string;
    name: string;
    code: string;
    description: string;
    deptId: string;
    deptName?: string;
    sortOrder: number;
    status: number;
    createdAt: string;
  }
}

/**
 * 获取岗位列表（分页）
 */
async function getPositionList(params: Recordable<any>) {
  const result = await requestClient.get<{
    items: SystemPositionApi.PositionListItem[];
    total: number;
  }>('/position', { params });
  return result;
}

/**
 * 获取单个岗位
 */
async function getPosition(id: string) {
  return requestClient.get<SystemPositionApi.Position>(`/position/${id}`);
}

/**
 * 创建岗位
 */
async function createPosition(data: {
  name: string;
  code: string;
  description: string;
  deptId: string;
  sortOrder: number;
  status: number;
}) {
  return requestClient.post<{ id: string; name: string; code: string }>(
    '/position',
    data,
  );
}

/**
 * 更新岗位
 */
async function updatePosition(data: {
  id: string;
  name: string;
  code: string;
  description: string;
  deptId: string;
  sortOrder: number;
  status: number;
}) {
  return requestClient.put('/position', data);
}

/**
 * 删除岗位
 */
async function deletePosition(id: string) {
  return requestClient.delete(`/position/${id}`);
}

export {
  createPosition,
  deletePosition,
  getPosition,
  getPositionList,
  updatePosition,
};
