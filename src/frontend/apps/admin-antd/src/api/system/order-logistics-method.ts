import { requestClient } from '#/api/request';

export namespace OrderLogisticsMethodApi {
  export interface OrderLogisticsMethodItem {
    id: string;
    name: string;
    typeValue: number;
    sort: number;
  }
}

function unwrap<T>(res: T[] | { data?: T[] }): T[] {
  if (Array.isArray(res)) return res;
  const data = (res as { data?: T[] })?.data;
  return Array.isArray(data) ? data : [];
}

async function getOrderLogisticsMethodList() {
  const res = await requestClient.get<
    | OrderLogisticsMethodApi.OrderLogisticsMethodItem[]
    | { data?: OrderLogisticsMethodApi.OrderLogisticsMethodItem[] }
  >('/order-logistics-methods');
  return unwrap(res);
}

async function createOrderLogisticsMethod(data: {
  name: string;
  typeValue?: number;
  sort?: number;
}) {
  const result = await requestClient.post<{ id: string }>(
    '/order-logistics-methods',
    {
      name: data.name,
      typeValue: data.typeValue ?? 0,
      sort: data.sort ?? 0,
    },
  );
  return (result as { data?: { id: string } })?.data ?? result;
}

async function updateOrderLogisticsMethod(
  id: string,
  data: { name: string; typeValue: number; sort: number },
) {
  return requestClient.put(`/order-logistics-methods/${id}`, data);
}

async function deleteOrderLogisticsMethod(id: string) {
  return requestClient.delete(`/order-logistics-methods/${id}`);
}

export {
  createOrderLogisticsMethod,
  deleteOrderLogisticsMethod,
  getOrderLogisticsMethodList,
  updateOrderLogisticsMethod,
};
