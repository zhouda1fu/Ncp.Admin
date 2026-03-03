import { requestClient } from '#/api/request';

export namespace ProjectTypeApi {
  export interface ProjectTypeItem {
    id: string;
    name: string;
    sortOrder: number;
  }
}

async function getProjectTypeList() {
  const res = await requestClient.get<ProjectTypeApi.ProjectTypeItem[]>(
    '/project-types',
  );
  const list = Array.isArray(res)
    ? res
    : (res as { data?: ProjectTypeApi.ProjectTypeItem[] })?.data ?? [];
  return list;
}

async function createProjectType(data: { name: string; sortOrder?: number }) {
  return requestClient.post<{ id: string }>('/project-types', {
    name: data.name,
    sortOrder: data.sortOrder ?? 0,
  });
}

async function updateProjectType(
  id: string,
  data: { name: string; sortOrder: number },
) {
  return requestClient.put(`/project-types/${id}`, data);
}

async function deleteProjectType(id: string) {
  return requestClient.delete(`/project-types/${id}`);
}

export {
  getProjectTypeList,
  createProjectType,
  updateProjectType,
  deleteProjectType,
};
