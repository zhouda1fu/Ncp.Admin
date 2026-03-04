import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 订单类型：0 销售 1 售后 2 样品 3 普测 */
export const OrderTypeEnum = {
  Sales: 0,
  AfterSales: 1,
  Sample: 2,
  GeneralTest: 3,
} as const;

/** 订单状态：1 审核中 2 已下单 3 已完成 4 已驳回 5 未到款 */
export const OrderStatusEnum = {
  PendingAudit: 1,
  Ordered: 2,
  Completed: 3,
  Rejected: 4,
  Unpaid: 5,
} as const;

export namespace OrderApi {
  export interface OrderItemDto {
    id: string;
    productId: string;
    productName: string;
    model: string;
    number: string;
    qty: number;
    unit: string;
    price: number;
    amount: number;
    remark: string;
  }

  export interface OrderItem {
    id: string;
    customerId: string;
    customerName: string;
    projectId: string;
    contractId: string;
    orderNumber: string;
    type: number;
    status: number;
    amount: number;
    remark: string;
    ownerId: string;
    ownerName: string;
    deptId: string;
    deptName: string;
    projectContactName: string;
    projectContactPhone: string;
    warranty: string;
    contractSigningCompany: string;
    contractTrustee: string;
    needInvoice: boolean;
    installationFee: number;
    estimatedFreight: number;
    selectedContractFileId: string;
    isShipped: boolean;
    paymentStatus: string;
    contractNotCompanyTemplate: boolean;
    contractDiscount: number;
    contractAmount: number;
    createdAt: string;
  }

  export interface OrderContractFileItem {
    path: string;
    fileName: string;
    size: number;
    format: string;
    updatedAt: string;
  }

  export interface OrderDetail extends OrderItem {
    contractFiles: OrderContractFileItem[];
    receiverName: string;
    receiverPhone: string;
    receiverAddress: string;
    payDate: string;
    deliveryDate: string;
    creatorId: string;
    updatedAt: string;
    items: OrderItemDto[];
  }
}

async function getOrderList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: OrderApi.OrderItem[];
    total: number;
  }>('/orders', { params });
  return res;
}

async function getOrder(id: string) {
  return requestClient.get<OrderApi.OrderDetail>(`/orders/${id}`);
}

async function createOrder(data: Recordable<any>) {
  return requestClient.post<{ id: string }>('/orders', data);
}

async function updateOrder(data: Recordable<any>) {
  return requestClient.put<boolean>('/orders', data);
}

async function deleteOrder(id: string) {
  return requestClient.delete<boolean>(`/orders/${id}`);
}

export {
  getOrderList,
  getOrder,
  createOrder,
  updateOrder,
  deleteOrder,
};
