import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace SystemUserApi {
  export interface SystemUser {
    [key: string]: any;
    userId: string;
    name: string;
    email: string;
    phone: string;
    realName: string;
    status: 0 | 1;
    gender: string;
    age: number;
    birthDate: string;
    deptId?: string;
    deptName?: string;
    isDeptManager?: boolean;
    roles: string[];
    createdAt: string;
    idCardNumber?: string;
    address?: string;
    education?: string;
    graduateSchool?: string;
    avatarUrl?: string;
    notOrderMeal?: boolean;
    orderMealSort?: number | null;
    wechatGuid?: string;
    isResigned?: boolean;
    resignedTime?: string | null;
    creatorId?: string;
    modifierId?: string | null;
    deleterId?: string | null;
    lastLoginTime?: string | null;
    lastLoginIp?: string | null;
  }
}

/**
 * 用户列表查询参数
 */
export interface GetUserListParams extends Recordable<any> {
  pageIndex?: number;
  pageSize?: number;
  countTotal?: boolean;
  keyword?: string;
  status?: number;
  isResigned?: boolean;
  /** 按部门筛选（与 positionId 二选一，后端优先 positionId） */
  deptId?: string;
  /** 按岗位筛选（与 deptId 二选一） */
  positionId?: string;
}

/**
 * 获取用户列表数据
 */
async function getUserList(params: GetUserListParams) {
  const result = await requestClient.get<{
    items: Array<{
      userId: string;
      name: string;
      email: string;
      phone: string;
      realName: string;
      status: 0 | 1;
      gender: string;
      age: number;
      birthDate: string;
      deptId?: string;
      deptName?: string;
      isDeptManager?: boolean;
      roles: string[];
      createdAt: string;
      idCardNumber?: string;
      address?: string;
      education?: string;
      graduateSchool?: string;
      avatarUrl?: string;
      notOrderMeal?: boolean;
      orderMealSort?: number | null;
      wechatGuid?: string;
      isResigned?: boolean;
      resignedTime?: string | null;
      creatorId?: string;
      modifierId?: string | null;
      deleterId?: string | null;
      lastLoginTime?: string | null;
      lastLoginIp?: string | null;
    }>;
    total: number;
    page: number;
    pageSize: number;
  }>('/users', { params });
  return result;
}

/**
 * 获取单个用户信息
 * @param id 用户 ID
 */
async function getUser(id: string) {
  return requestClient.get<SystemUserApi.SystemUser>(`/users/${id}`);
}

/**
 * 创建用户
 * @param data 用户数据
 */
async function createUser(data: {
  name: string;
  email: string;
  password: string;
  phone: string;
  realName: string;
  status: 0 | 1;
  gender: string;
  birthDate: string;
  deptId?: string;
  deptName?: string;
  isDeptManager?: boolean;
  roleIds: string[];
  idCardNumber?: string;
  address?: string;
  education?: string;
  graduateSchool?: string;
  avatarUrl?: string;
  notOrderMeal?: boolean;
  wechatGuid?: string;
  isResigned?: boolean;
  resignedTime?: string;
}) {
  return requestClient.post('/users', data);
}

/**
 * 更新用户
 *
 * @param id 用户 ID
 * @param data 用户数据
 */
async function updateUser(
  id: string,
  data: {
    name: string;
    email: string;
    phone: string;
    realName: string;
    status: 0 | 1;
    gender: string;
    age: number;
    birthDate: string;
    deptId: string;
    deptName: string;
    isDeptManager?: boolean;
    password?: string;
    idCardNumber?: string;
    address?: string;
    education?: string;
    graduateSchool?: string;
    avatarUrl?: string;
    notOrderMeal?: boolean;
    wechatGuid?: string;
    isResigned?: boolean;
    resignedTime?: string;
  },
) {
  return requestClient.put('/user/update', {
    userId: id,
    ...data,
  });
}

/**
 * 删除用户
 * @param id 用户 ID
 */
async function deleteUser(id: string) {
  return requestClient.delete(`/users/${id}`);
}

/**
 * 更新用户角色
 * @param userId 用户 ID
 * @param roleIds 角色ID列表
 */
async function updateUserRoles(userId: string, roleIds: string[]) {
  return requestClient.put('/users/update-roles', {
    userId,
    roleIds,
  });
}

export interface UserImportResult {
  successCount: number;
  errors: Array<{ rowNumber: number; message: string }>;
}

function triggerBlobDownload(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  a.click();
  URL.revokeObjectURL(url);
}

/**
 * 按当前列表筛选条件导出用户 Excel（不含密码）
 */
async function exportUsersExcel(params: Omit<GetUserListParams, 'pageIndex' | 'pageSize' | 'countTotal'>) {
  const blob = (await requestClient.get<Blob>('/users/excel/export', {
    params,
    responseType: 'blob',
    responseReturn: 'body',
  })) as Blob;
  const stamp = new Date().toISOString().slice(0, 19).replace(/:/g, '').replace('T', '-');
  triggerBlobDownload(blob, `users-${stamp}.xlsx`);
}

/** 下载用户导入模板 */
async function downloadUserImportTemplate() {
  const blob = (await requestClient.get<Blob>('/users/excel/import-template', {
    responseType: 'blob',
    responseReturn: 'body',
  })) as Blob;
  triggerBlobDownload(blob, 'user-import-template.xlsx');
}

/** 上传 Excel 批量创建用户 */
async function importUsersExcel(file: File) {
  const formData = new FormData();
  formData.append('file', file);
  return requestClient.post<UserImportResult>('/users/excel/import', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

export {
  createUser,
  deleteUser,
  downloadUserImportTemplate,
  exportUsersExcel,
  getUser,
  getUserList,
  importUsersExcel,
  updateUser,
  updateUserRoles,
};
