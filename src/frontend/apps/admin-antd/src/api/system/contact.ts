import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace ContactApi {
  export interface ContactItem {
    id: string;
    name: string;
    phone?: string;
    email?: string;
    company?: string;
    groupId?: string;
    creatorId: string;
    createdAt: string;
  }
}

async function getContactList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ContactApi.ContactItem[];
    total: number;
  }>('/contacts', { params });
  return res;
}

async function createContact(data: {
  name: string;
  phone?: string;
  email?: string;
  company?: string;
  groupId?: string;
}) {
  return requestClient.post<{ id: string }>('/contacts', data);
}

async function updateContact(
  id: string,
  data: {
    name: string;
    phone?: string;
    email?: string;
    company?: string;
    groupId?: string;
  },
) {
  return requestClient.put(`/contacts/${id}`, data);
}

export { createContact, getContactList, updateContact };
