import { requestClient } from '#/api/request';

export namespace SystemDeptApi {
  export interface DeptResponsibleUser {
    userId: string;
    name: string;
    isDefault: boolean;
    sortOrder: number;
  }

  export interface SystemDept {
    [key: string]: any;
    children?: SystemDept[];
    id: string;
    name: string;
    remark?: string;
    parentId?: string;
    status: 0 | 1;
    sortOrder: number;
    responsibleUsers?: DeptResponsibleUser[];
    createdAt: string;
  }
}

/**
 * 获取部门列表数据（后端无 GET /dept，使用 /dept/tree 返回树，调用方需自行 flatten 或直接使用树）
 */
async function getDeptList() {
  return requestClient.get<Array<SystemDeptApi.SystemDept>>('/dept/tree');
}

/**
 * 获取部门树数据
 * @param params.includeInactive 是否包含已停用部门（默认仅返回启用部门）
 */
async function getDeptTree(params?: { includeInactive?: boolean }) {
  return requestClient.get<Array<SystemDeptApi.SystemDept>>('/dept/tree', {
    params: params?.includeInactive ? { includeInactive: true } : undefined,
  });
}

/**
 * 获取单个部门信息
 * @param id 部门 ID
 */
async function getDept(id: string) {
  return requestClient.get<SystemDeptApi.SystemDept>(`/dept/${id}`);
}

/**
 * 创建部门
 * @param data 部门数据
 */
async function createDept(data: {
  name: string;
  remark?: string;
  parentId?: string;
  status: 0 | 1;
  sortOrder?: number;
  responsibleUserIds?: string[];
  defaultResponsibleUserId?: string;
}) {
  return requestClient.post('/dept', data);
}

/**
 * 更新部门
 *
 * @param id 部门 ID
 * @param data 部门数据
 */
async function updateDept(
  id: string,
  data: {
    name: string;
    remark?: string;
    parentId?: string;
    status: 0 | 1;
    sortOrder?: number;
    responsibleUserIds?: string[];
    defaultResponsibleUserId?: string;
  },
) {
  return requestClient.put('/dept', {
    id,
    ...data,
  });
}

/**
 * 删除部门
 * @param id 部门 ID
 */
async function deleteDept(id: string) {
  return requestClient.delete(`/dept/${id}`);
}

/**
 * 重排同级部门排序
 * @param parentId 父级部门 ID；空表示顶级部门
 * @param orderedIds 同级部门按新顺序排列的 ID 列表
 */
async function reorderDeptSort(parentId: string | undefined, orderedIds: string[]) {
  return requestClient.post<boolean>('/dept/reorder', {
    parentId: parentId ?? null,
    orderedIds,
  });
}

export {
  createDept,
  deleteDept,
  getDept,
  getDeptList,
  getDeptTree,
  reorderDeptSort,
  updateDept,
};
