import { requestClient } from '#/api/request';

export namespace OrderInvoiceTypeOptionApi {
  export interface OrderInvoiceTypeOptionItem {
    id: number;
    name: string;
    typeValue: number;
    sortOrder: number;
  }
}

function unwrap<T>(res: T[] | { data?: T[] }): T[] {
  if (Array.isArray(res)) return res;
  const data = (res as { data?: T[] })?.data;
  return Array.isArray(data) ? data : [];
}

async function getOrderInvoiceTypeOptionList() {
  const res = await requestClient.get<
    | OrderInvoiceTypeOptionApi.OrderInvoiceTypeOptionItem[]
    | { data?: OrderInvoiceTypeOptionApi.OrderInvoiceTypeOptionItem[] }
  >('/order-invoice-type-options');
  return unwrap(res);
}

async function createOrderInvoiceTypeOption(data: {
  name: string;
  typeValue: number;
  sortOrder?: number;
}) {
  const result = await requestClient.post<{ id: number }>(
    '/order-invoice-type-options',
    {
      name: data.name,
      typeValue: data.typeValue,
      sortOrder: data.sortOrder ?? 0,
    },
  );
  return (result as { data?: { id: number } })?.data ?? result;
}

async function updateOrderInvoiceTypeOption(
  id: number,
  data: { name: string; typeValue: number; sortOrder: number },
) {
  return requestClient.put(`/order-invoice-type-options/${id}`, data);
}

async function deleteOrderInvoiceTypeOption(id: number) {
  return requestClient.delete(`/order-invoice-type-options/${id}`);
}

export {
  createOrderInvoiceTypeOption,
  deleteOrderInvoiceTypeOption,
  getOrderInvoiceTypeOptionList,
  updateOrderInvoiceTypeOption,
};
