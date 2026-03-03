import { requestClient } from '#/api/request';

export namespace ProjectIndustryApi {
  export interface ProjectIndustryItem {
    id: string;
    name: string;
    sortOrder: number;
  }
}

async function getProjectIndustryList() {
  const res = await requestClient.get<ProjectIndustryApi.ProjectIndustryItem[]>(
    '/project-industries',
  );
  const list = Array.isArray(res)
    ? res
    : (res as { data?: ProjectIndustryApi.ProjectIndustryItem[] })?.data ?? [];
  return list;
}

async function createProjectIndustry(data: {
  name: string;
  sortOrder?: number;
}) {
  return requestClient.post<{ id: string }>('/project-industries', {
    name: data.name,
    sortOrder: data.sortOrder ?? 0,
  });
}

async function updateProjectIndustry(
  id: string,
  data: { name: string; sortOrder: number },
) {
  return requestClient.put(`/project-industries/${id}`, data);
}

async function deleteProjectIndustry(id: string) {
  return requestClient.delete(`/project-industries/${id}`);
}

export {
  getProjectIndustryList,
  createProjectIndustry,
  updateProjectIndustry,
  deleteProjectIndustry,
};
