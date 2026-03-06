import { requestClient } from '#/api/request';

export namespace CustomerSourceApi {
  export interface CustomerSourceItem {
    id: string;
    name: string;
    sortOrder: number;
    /** 使用场景：0 公海 1 客户列表 2 通用 */
    usageScene?: number;
  }
}

/** scene: sea=公海，list=客户列表；不传则返回全部 */
async function getCustomerSourceList(params?: { scene?: 'sea' | 'list' }) {
  const res = await requestClient.get<CustomerSourceApi.CustomerSourceItem[]>(
    '/customer-sources',
    { params },
  );
  const raw = Array.isArray(res) ? res : (res as { data?: CustomerSourceApi.CustomerSourceItem[] })?.data ?? [];
  return raw.map((x) => {
    const item = x as unknown as Record<string, unknown>;
    return {
      id: String(item.id ?? item.Id ?? ''),
      name: String(item.name ?? item.Name ?? ''),
      sortOrder: Number(item.sortOrder ?? item.SortOrder ?? 0),
      usageScene: Number(item.usageScene ?? item.UsageScene ?? 2),
    } as CustomerSourceApi.CustomerSourceItem;
  });
}

async function createCustomerSource(data: { name: string; sortOrder?: number; usageScene?: number }) {
  return requestClient.post<{ id: string }>('/customer-sources', {
    name: data.name,
    sortOrder: data.sortOrder ?? 0,
    usageScene: data.usageScene ?? 2,
  });
}

async function updateCustomerSource(
  id: string,
  data: { name: string; sortOrder: number; usageScene: number },
) {
  return requestClient.put(`/customer-sources/${id}`, data);
}

export {
  getCustomerSourceList,
  createCustomerSource,
  updateCustomerSource,
};
