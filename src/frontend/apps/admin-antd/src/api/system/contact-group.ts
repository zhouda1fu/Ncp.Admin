import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace ContactGroupApi {
  export interface ContactGroupItem {
    id: string;
    name: string;
    creatorId: string;
    sortOrder: number;
    createdAt: string;
  }
}

async function getContactGroupList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ContactGroupApi.ContactGroupItem[];
    total: number;
  }>('/contact-groups', { params });
  return res;
}

async function createContactGroup(data: { name: string; sortOrder?: number }) {
  return requestClient.post<{ id: string }>('/contact-groups', {
    name: data.name,
    sortOrder: data.sortOrder ?? 0,
  });
}

async function updateContactGroup(
  id: string,
  data: { name: string; sortOrder: number },
) {
  return requestClient.put(`/contact-groups/${id}`, data);
}

export {
  createContactGroup,
  getContactGroupList,
  updateContactGroup,
};
