import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace ProjectApi {
  /** 项目状态：0 进行中 1 已归档 */
  export type ProjectStatus = 0 | 1;

  /** 项目联系人 */
  export interface ProjectContactItem {
    id: string;
    customerContactId?: string;
    name: string;
    position?: string;
    mobile?: string;
    officePhone?: string;
    qq?: string;
    wechat?: string;
    email?: string;
    isPrimary: boolean;
    remark?: string;
  }

  /** 项目跟进记录 */
  export interface ProjectFollowUpRecordItem {
    id: string;
    title: string;
    visitDate?: string;
    reminderIntervalDays: number;
    content?: string;
    createdAt: string;
    creatorId?: string;
  }

  export interface ProjectItem {
    id: string;
    name: string;
    description?: string;
    creatorId: string;
    creatorName?: string;
    status: ProjectStatus;
    createdAt: string;
    customerId: string;
    customerName?: string;
    projectTypeId: string;
    projectTypeName?: string;
    projectStatusOptionId: string;
    projectStatusOptionName?: string;
    projectNumber?: string;
    projectIndustryId: string;
    projectIndustryName?: string;
    provinceRegionId: number;
    provinceName?: string;
    cityRegionId: number;
    cityName?: string;
    districtRegionId: number;
    districtName?: string;
    startDate?: string;
    projectEstimate?: string;
    purchaseAmount?: number;
    projectContent?: string;
    /** 项目联系人（详情接口返回） */
    contacts?: ProjectContactItem[];
    /** 项目跟进记录（详情接口返回） */
    followUpRecords?: ProjectFollowUpRecordItem[];
  }

  export interface CreateProjectParams {
    name: string;
    description?: string;
    customerId: string;
    customerName: string;
    projectTypeId: string;
    projectTypeName: string;
    projectStatusOptionId: string;
    projectStatusOptionName: string;
    projectIndustryId: string;
    projectIndustryName: string;
    provinceRegionId: number;
    provinceName: string;
    cityRegionId: number;
    cityName: string;
    districtRegionId: number;
    districtName: string;
    projectNumber?: string;
    startDate?: string;
    projectEstimate?: string;
    purchaseAmount?: number;
    projectContent?: string;
  }

  export interface UpdateProjectParams {
    name: string;
    description?: string;
    projectTypeId: string;
    projectTypeName: string;
    projectStatusOptionId: string;
    projectStatusOptionName: string;
    projectNumber?: string;
    projectIndustryId: string;
    projectIndustryName: string;
    provinceRegionId: number;
    provinceName: string;
    cityRegionId: number;
    cityName: string;
    districtRegionId: number;
    districtName: string;
    startDate?: string;
    projectEstimate?: string;
    purchaseAmount?: number;
    projectContent?: string;
  }

  /** 添加/更新项目联系人参数 */
  export interface ProjectContactParams {
    customerContactId?: string;
    name: string;
    position?: string;
    mobile?: string;
    officePhone?: string;
    qq?: string;
    wechat?: string;
    email?: string;
    isPrimary: boolean;
    remark?: string;
  }

  /** 添加/更新项目跟进记录参数 */
  export interface ProjectFollowUpRecordParams {
    title: string;
    visitDate?: string;
    reminderIntervalDays: number;
    content?: string;
  }
}

/**
 * 获取项目列表（分页）
 */
async function getProjectList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ProjectApi.ProjectItem[];
    total: number;
  }>('/projects', { params });
  return res;
}

/**
 * 获取项目详情
 */
async function getProject(id: string) {
  return requestClient.get<ProjectApi.ProjectItem>(`/projects/${id}`);
}

/**
 * 创建项目
 */
async function createProject(data: ProjectApi.CreateProjectParams) {
  return requestClient.post<{ id: string }>('/projects', data);
}

/**
 * 更新项目
 */
async function updateProject(id: string, data: ProjectApi.UpdateProjectParams) {
  return requestClient.put(`/projects/${id}`, data);
}

/**
 * 删除项目
 */
async function deleteProject(id: string) {
  return requestClient.delete(`/projects/${id}`);
}

/**
 * 归档项目
 */
async function archiveProject(id: string) {
  return requestClient.put(`/projects/${id}/archive`);
}

/**
 * 激活项目
 */
async function activateProject(id: string) {
  return requestClient.put(`/projects/${id}/activate`);
}

/**
 * 添加项目联系人
 */
async function addProjectContact(projectId: string, data: ProjectApi.ProjectContactParams) {
  return requestClient.post<{ id: string }>(`/projects/${projectId}/contacts`, data);
}

/**
 * 更新项目联系人
 */
async function updateProjectContact(
  projectId: string,
  contactId: string,
  data: ProjectApi.ProjectContactParams,
) {
  return requestClient.put(`/projects/${projectId}/contacts/${contactId}`, data);
}

/**
 * 删除项目联系人
 */
async function removeProjectContact(projectId: string, contactId: string) {
  return requestClient.delete(`/projects/${projectId}/contacts/${contactId}`);
}

/**
 * 添加项目跟进记录
 */
async function addProjectFollowUpRecord(
  projectId: string,
  data: ProjectApi.ProjectFollowUpRecordParams,
) {
  return requestClient.post<{ id: string }>(`/projects/${projectId}/follow-up-records`, data);
}

/**
 * 更新项目跟进记录
 */
async function updateProjectFollowUpRecord(
  projectId: string,
  recordId: string,
  data: ProjectApi.ProjectFollowUpRecordParams,
) {
  return requestClient.put(`/projects/${projectId}/follow-up-records/${recordId}`, data);
}

/**
 * 删除项目跟进记录
 */
async function removeProjectFollowUpRecord(projectId: string, recordId: string) {
  return requestClient.delete(`/projects/${projectId}/follow-up-records/${recordId}`);
}

export {
  getProjectList,
  getProject,
  createProject,
  updateProject,
  deleteProject,
  archiveProject,
  activateProject,
  addProjectContact,
  updateProjectContact,
  removeProjectContact,
  addProjectFollowUpRecord,
  updateProjectFollowUpRecord,
  removeProjectFollowUpRecord,
};
