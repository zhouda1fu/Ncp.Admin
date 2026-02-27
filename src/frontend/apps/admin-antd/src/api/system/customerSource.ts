import { requestClient } from '#/api/request';

export namespace CustomerSourceApi {
  export interface CustomerSourceItem {
    id: string;
    name: string;
    sortOrder: number;
  }
}

async function getCustomerSourceList() {
  const res = await requestClient.get<CustomerSourceApi.CustomerSourceItem[]>(
    '/customer-sources',
  );
  const list = Array.isArray(res) ? res : (res as { data?: CustomerSourceApi.CustomerSourceItem[] })?.data ?? [];
  return list;
}

async function createCustomerSource(data: { name: string; sortOrder?: number }) {
  return requestClient.post<{ id: string }>('/customer-sources', {
    name: data.name,
    sortOrder: data.sortOrder ?? 0,
  });
}

async function updateCustomerSource(
  id: string,
  data: { name: string; sortOrder: number },
) {
  return requestClient.put(`/customer-sources/${id}`, data);
}

export {
  getCustomerSourceList,
  createCustomerSource,
  updateCustomerSource,
};
