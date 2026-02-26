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
  return requestClient.get<IndustryApi.IndustryItem[]>('/industries');
}

export { getIndustryList };
