import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace SystemUserApi {
  export interface UserColumnFacet {
    value: string;
    count: number;
    displayLabel?: string | null;
  }

  /** 用户修改记录（与后端 UserFieldChangeRowDto 对齐） */
  export interface UserFieldChangeRow {
    fieldKey: string;
    oldDisplay: string;
    newDisplay: string;
    operatorUserName: string;
    changedAt: string;
  }

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
    roles: string[];
    createdAt: string;
    idCardNumber?: string;
    address?: string;
    education?: string;
    graduateSchool?: string;
    avatarUrl?: string;
    notOrderMeal?: boolean;
    orderMealSort?: number | null;
    attendanceRequired?: boolean;
    wechatGuid?: string;
    isResigned?: boolean;
    resignedTime?: string | null;
    setAsDeptResponsibleUser?: boolean;
    setAsDefaultDeptResponsibleUser?: boolean;
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
  /** 表头筛选：部门名称（精确多选） */
  filterDeptNames?: string[];
  /** 表头筛选：角色名称（精确多选；拥有任一角色即命中） */
  filterRoleNames?: string[];
  /**
   * 仅「营销中心」及其下级部门用户（与 deptId/positionId 互斥：后端在开启本项时忽略部门/岗位筛选）。
   * 用于客户协作转交/分享选人等场景。
   */
  onlyMarketingCenterDeptSubtree?: boolean;
  /**
   * 仅「技术部」及其下级部门用户（与 deptId/positionId 互斥：后端在开启本项时忽略部门/岗位筛选）。
   * 用于技术分配选人等场景。
   */
  onlyTechnologyDeptSubtree?: boolean;
  /**
   * 仅值日可选部门且参与考勤的用户：「产品研发中心」「网络推广组」及其下级（与 deptId/positionId 互斥）。
   * 用于值日安排选人等场景。
   */
  onlyProductResearchCenterDeptSubtree?: boolean;
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
      setAsDeptResponsibleUser?: boolean;
      setAsDefaultDeptResponsibleUser?: boolean;
      creatorId?: string;
      modifierId?: string | null;
      deleterId?: string | null;
      lastLoginTime?: string | null;
      lastLoginIp?: string | null;
    }>;
    total: number;
    page: number;
    pageSize: number;
  }>('/users', { params, paramsSerializer: 'repeat' });
  return result;
}

/** 用户列表表头分面（不含当前列自身的表头多选条件） */
async function getUserColumnFacets(params: Recordable<any>, facetColumn: string) {
  return requestClient.get<SystemUserApi.UserColumnFacet[]>('/users/column-facets', {
    params: { ...params, facetColumn },
    paramsSerializer: 'repeat',
  });
}

/**
 * 获取单个用户信息
 * @param id 用户 ID
 */
async function getUser(id: string, config?: Recordable<any>) {
  return requestClient.get<SystemUserApi.SystemUser>(`/users/${id}`, config);
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
  roleIds: string[];
  idCardNumber?: string;
  address?: string;
  education?: string;
  graduateSchool?: string;
  avatarUrl?: string;
  notOrderMeal?: boolean;
  attendanceRequired?: boolean;
  wechatGuid?: string;
  isResigned?: boolean;
  resignedTime?: string;
  setAsDeptResponsibleUser?: boolean;
  setAsDefaultDeptResponsibleUser?: boolean;
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
    password?: string;
    idCardNumber?: string;
    address?: string;
    education?: string;
    graduateSchool?: string;
    avatarUrl?: string;
    notOrderMeal?: boolean;
    attendanceRequired?: boolean;
    wechatGuid?: string;
    isResigned?: boolean;
    resignedTime?: string;
    setAsDeptResponsibleUser?: boolean;
    setAsDefaultDeptResponsibleUser?: boolean;
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
    paramsSerializer: 'repeat',
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

/** 当前用户工作流路由角色（作废审批等多角色弹窗） */
export interface WorkflowRoutingRoleItem {
  roleId: string;
  roleName: string;
}

async function getCurrentUserWorkflowRoutingRoles() {
  return requestClient.get<WorkflowRoutingRoleItem[]>('/user/current/workflow-routing-roles');
}

/** 上传 Excel 批量创建用户 */
async function importUsersExcel(file: File) {
  const formData = new FormData();
  formData.append('file', file);
  return requestClient.post<UserImportResult>('/users/excel/import', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

/** 用户字段级修改历史（操作日志对比）；与后端 GET /api/admin/user/log 对齐（兼容旧路径 page 参数） */
async function getUserChangeHistory(
  userId: string,
  params: { pageIndex?: number; pageSize?: number; keyword?: string },
) {
  return requestClient.get<{ items: SystemUserApi.UserFieldChangeRow[]; total: number }>('/user/log', {
    params: {
      userId,
      page: params.pageIndex ?? 1,
      pageSize: params.pageSize ?? 20,
      keyword: params.keyword,
    },
  });
}

export {
  createUser,
  deleteUser,
  downloadUserImportTemplate,
  exportUsersExcel,
  getCurrentUserWorkflowRoutingRoles,
  getUser,
  getUserChangeHistory,
  getUserColumnFacets,
  getUserList,
  importUsersExcel,
  updateUser,
  updateUserRoles,
};
