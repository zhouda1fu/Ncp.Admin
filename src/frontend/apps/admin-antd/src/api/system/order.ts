import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 订单类型：0 销售 1 售后 2 样品 3 普测 */
export const OrderTypeEnum = {
  Sales: 0,
  AfterSales: 1,
  Sample: 2,
  GeneralTest: 3,
} as const;

/** 订单状态：0 草稿 1 审核中 2 已下单 3 已完成 4 已驳回 5 未到款 */
export const OrderStatusEnum = {
  Draft: 0,
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
    productCategoryId: string;
    productTypeId: string;
    imagePath: string;
    installNotes: string;
    trainingDuration: string;
    packingStatus: number;
    reviewStatus: number;
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
    projectName: string;
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
    invoiceTypeId: number;
    installationFee: number;
    estimatedFreight: number;
    orderLogisticsCompanyId: string;
    orderLogisticsMethodId: string;
    selectedContractFileId: number;
    isShipped: boolean;
    paymentStatus: number;
    contractNotCompanyTemplate: boolean;
    contractAmount: number;
    logisticsPaymentMethodId: number;
    waybillNumber: string;
    shippingFee: number;
    shippingFeeIsPay: boolean;
    surcharge: number;
    warehouseStatus: number;
    workflowInstanceId: string | null;
    createdAt: string;
  }

  export interface OrderContractFileItem {
    path: string;
    fileName: string;
    size: number;
    format: string;
    updatedAt: string;
  }

  /** 按产品分类的合同优惠（order_band） */
  export interface OrderCategoryDto {
    id: string;
    productCategoryId: string;
    categoryName: string;
    discountPoints: number;
    remark: string;
  }

  export interface OrderRemarkDto {
    id: string;
    typeId: number;
    content: string;
    userId: string;
    userName: string;
    addedAt: string;
  }

  export interface OrderDetail extends OrderItem {
    contractFiles: OrderContractFileItem[];
    stockFiles?: OrderContractFileItem[];
    receiverName: string;
    receiverPhone: string;
    receiverAddress: string;
    payDate: string;
    deliveryDate: string;
    creatorId: string;
    updatedAt: string;
    workflowInstanceId: string | null;
    items: OrderItemDto[];
    /** 按分类的合同优惠明细 */
    orderCategories?: OrderCategoryDto[];
  }

  export interface OrderQueryDto extends OrderItem {}
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

async function updateOrder(id: string, data: Recordable<any>) {
  return requestClient.put<boolean>(`/orders/${id}`, data);
}

async function deleteOrder(id: string) {
  return requestClient.delete<boolean>(`/orders/${id}`);
}

async function submitOrder(id: string, remark: string = '') {
  return requestClient.post<boolean>(`/orders/${id}/submit`, { id, remark });
}

async function getOrderRemarks(orderId: string) {
  return requestClient.get<OrderApi.OrderRemarkDto[]>(`/orders/${orderId}/remarks`);
}

async function createOrderRemark(orderId: string, content: string) {
  return requestClient.post<boolean>(`/orders/${orderId}/remarks`, { content });
}

async function updateOrderRemark(orderId: string, remarkId: string, content: string) {
  return requestClient.put<boolean>(`/orders/${orderId}/remarks/${remarkId}`, { content });
}

async function deleteOrderRemark(orderId: string, remarkId: string) {
  return requestClient.delete<boolean>(`/orders/${orderId}/remarks/${remarkId}`);
}

interface OrderPushRecordResponse {
  pushId: string;
  deptName?: string;
  pusherName: string;
  pushTime: string;
  processName: string;
  reason: string;
}

async function getOrderPushRecords(orderId: string) {
  return requestClient.get<OrderPushRecordResponse[]>(
    `/orders/${orderId}/push-records`,
  );
}

export {
  getOrderList,
  getOrder,
  createOrder,
  updateOrder,
  deleteOrder,
  submitOrder,
  getOrderRemarks,
  createOrderRemark,
  updateOrderRemark,
  deleteOrderRemark,
  getOrderPushRecords,
};
