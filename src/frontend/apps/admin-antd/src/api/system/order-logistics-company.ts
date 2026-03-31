import { requestClient } from '#/api/request';

export namespace OrderLogisticsCompanyApi {
  export interface OrderLogisticsCompanyItem {
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

async function getOrderLogisticsCompanyList() {
  const res = await requestClient.get<
    | OrderLogisticsCompanyApi.OrderLogisticsCompanyItem[]
    | { data?: OrderLogisticsCompanyApi.OrderLogisticsCompanyItem[] }
  >('/order-logistics-companies');
  return unwrap(res);
}

async function createOrderLogisticsCompany(data: {
  name: string;
  typeValue?: number;
  sort?: number;
}) {
  const result = await requestClient.post<{ id: string }>(
    '/order-logistics-companies',
    {
      name: data.name,
      typeValue: data.typeValue ?? 0,
      sort: data.sort ?? 0,
    },
  );
  return (result as { data?: { id: string } })?.data ?? result;
}

async function updateOrderLogisticsCompany(
  id: string,
  data: { name: string; typeValue: number; sort: number },
) {
  return requestClient.put(`/order-logistics-companies/${id}`, data);
}

async function deleteOrderLogisticsCompany(id: string) {
  return requestClient.delete(`/order-logistics-companies/${id}`);
}

export {
  createOrderLogisticsCompany,
  deleteOrderLogisticsCompany,
  getOrderLogisticsCompanyList,
  updateOrderLogisticsCompany,
};
