import { requestClient } from '#/api/request';

export namespace SystemDeptApi {
  export interface SystemDept {
    [key: string]: any;
    children?: SystemDept[];
    id: string;
    name: string;
    description: string;
    parentId: string;
    sortOrder: number;
    isActive: boolean;
    createdAt: string;
  }
}

/**
 * 获取部门列表数据
 */
async function getDeptList() {
  return requestClient.get<Array<SystemDeptApi.SystemDept>>(
    '/organization-units',
  );
}

/**
 * 获取部门树数据
 */
async function getDeptTree() {
  return requestClient.get<Array<SystemDeptApi.SystemDept>>(
    '/organization-units/tree',
  );
}

/**
 * 获取单个部门信息
 * @param id 部门 ID
 */
async function getDept(id: string) {
  return requestClient.get<SystemDeptApi.SystemDept>(
    `/organization-units/${id}`,
  );
}

/**
 * 创建部门
 * @param data 部门数据
 */
async function createDept(data: {
  name: string;
  description: string;
  parentId?: string;
  sortOrder: number;
}) {
  return requestClient.post('/organization-units', data);
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
    description: string;
    parentId?: string;
    sortOrder: number;
  },
) {
  return requestClient.put('/organization-units', {
    id,
    ...data,
  });
}

/**
 * 删除部门
 * @param id 部门 ID
 */
async function deleteDept(id: string) {
  return requestClient.delete(`/organization-units/${id}`);
}

export {
  createDept,
  deleteDept,
  getDept,
  getDeptList,
  getDeptTree,
  updateDept,
};
