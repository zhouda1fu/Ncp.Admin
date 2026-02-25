import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace ProjectApi {
  /** 项目状态：0 进行中 1 已归档 */
  export type ProjectStatus = 0 | 1;

  export interface ProjectItem {
    id: string;
    name: string;
    description?: string;
    creatorId: string;
    status: ProjectStatus;
    createdAt: string;
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
 * 创建项目
 */
async function createProject(data: { name: string; description?: string }) {
  return requestClient.post<{ id: string }>('/projects', data);
}

/**
 * 更新项目
 */
async function updateProject(
  id: string,
  data: { name: string; description?: string },
) {
  return requestClient.put(`/projects/${id}`, data);
}

export { createProject, getProjectList, updateProject };
