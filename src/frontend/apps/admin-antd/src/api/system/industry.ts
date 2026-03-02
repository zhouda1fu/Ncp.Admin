import { requestClient } from '#/api/request';

export namespace IndustryApi {
  export interface IndustryItem {
    id: string;
    name: string;
    parentId?: string;
    sortOrder: number;
    remark?: string;
  }
}

async function getIndustryList() {
  const res = await requestClient.get<IndustryApi.IndustryItem[]>('/industries');
  const list = Array.isArray(res) ? res : (res as { data?: IndustryApi.IndustryItem[] })?.data ?? [];
  return list;
}

async function createIndustry(data: {
  name: string;
  parentId?: string;
  sortOrder?: number;
  remark?: string;
}) {
  return requestClient.post<{ id: string }>('/industries', {
    name: data.name,
    parentId: data.parentId ?? null,
    sortOrder: data.sortOrder ?? 0,
    remark: data.remark ?? null,
  });
}

async function updateIndustry(
  id: string,
  data: {
    name: string;
    parentId?: string | null;
    sortOrder: number;
    remark?: string;
  },
) {
  return requestClient.put(`/industries/${id}`, {
    name: data.name,
    parentId: data.parentId ?? null,
    sortOrder: data.sortOrder,
    remark: data.remark ?? null,
  });
}

async function deleteIndustry(id: string) {
  return requestClient.delete(`/industries/${id}`);
}

export { getIndustryList, createIndustry, updateIndustry, deleteIndustry };
