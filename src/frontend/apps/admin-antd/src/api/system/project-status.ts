import { requestClient } from '#/api/request';

export namespace ProjectStatusApi {
  export interface ProjectStatusItem {
    id: string;
    name: string;
    code?: string;
    sortOrder: number;
  }
}

async function getProjectStatusList() {
  const res = await requestClient.get<ProjectStatusApi.ProjectStatusItem[]>(
    '/project-status-options',
  );
  const list = Array.isArray(res)
    ? res
    : (res as { data?: ProjectStatusApi.ProjectStatusItem[] })?.data ?? [];
  return list;
}

async function createProjectStatus(data: {
  name: string;
  code?: string;
  sortOrder?: number;
}) {
  return requestClient.post<{ id: string }>('/project-status-options', {
    name: data.name,
    code: data.code,
    sortOrder: data.sortOrder ?? 0,
  });
}

async function updateProjectStatus(
  id: string,
  data: { name: string; code?: string; sortOrder: number },
) {
  return requestClient.put(`/project-status-options/${id}`, data);
}

async function deleteProjectStatus(id: string) {
  return requestClient.delete(`/project-status-options/${id}`);
}

export {
  getProjectStatusList,
  createProjectStatus,
  updateProjectStatus,
  deleteProjectStatus,
};
