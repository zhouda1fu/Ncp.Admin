import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 合同状态：0 草稿 1 审批中 2 已生效 3 已归档 */
export type ContractStatus = 0 | 1 | 2 | 3;

/** 合同类型为 int，取值来自合同类型选项（ContractTypeOption）的 typeValue */
export type ContractType = number;

/** 收支类型为 int，取值来自收支类型选项（IncomeExpenseTypeOption）的 typeValue */
export type IncomeExpenseType = number;

export namespace ContractApi {
  export interface ContractItem {
    id: string;
    code: string;
    title: string;
    partyA: string;
    partyB: string;
    amount: number;
    startDate: string;
    endDate: string;
    status: ContractStatus;
    fileStorageKey?: string;
    creatorId: string;
    createdAt: string;
    orderId?: string;
    customerId?: string;
    customerName?: string;
    contractType: ContractType;
    contractTypeName?: string;
    incomeExpenseType: IncomeExpenseType;
    incomeExpenseTypeName?: string;
    signDate?: string;
    note?: string;
    description?: string;
    approvedBy?: string;
    approvedAt?: string;
    hasAttachment?: boolean;
    departmentId?: string;
    businessManager?: string;
    responsibleProject?: string;
    inputCustomer?: string;
    nextPaymentReminder?: boolean;
    contractExpiryReminder?: boolean;
    singleDoubleSeal?: number;
    invoicingInformation?: string;
    paymentStatus?: number;
    warrantyPeriod?: string;
    isInstallmentPayment?: boolean;
    accumulatedAmount?: number;
    invoices?: ContractApi.ContractInvoiceItem[];
  }

  /** 合同发票子项（详情接口返回） */
  export interface ContractInvoiceItem {
    id: string;
    type: number;
    invoiceNumber: string;
    taxRate: number;
    amountExclTax: number;
    source: string;
    status: boolean;
    title: string;
    taxAmount: number;
    invoicedAmount: number;
    handler: string;
    billingDate: string;
    remarks: string;
    attachmentStorageKey: string;
  }

  /** 新增/更新发票请求体 */
  export interface CreateContractInvoicePayload {
    type: number;
    invoiceNumber: string;
    taxRate: number;
    amountExclTax: number;
    source: string;
    status: boolean;
    title: string;
    taxAmount: number;
    invoicedAmount: number;
    handler?: string;
    billingDate?: string;
    remarks?: string;
    attachmentStorageKey?: string;
  }
}

/**
 * 获取合同列表（分页）
 */
async function getContractList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ContractApi.ContractItem[];
    total: number;
  }>('/contracts', { params });
  return res;
}

/**
 * 获取合同详情
 */
async function getContract(id: string) {
  return requestClient.get<ContractApi.ContractItem>(`/contracts/${id}`);
}

/**
 * 创建合同
 */
async function createContract(data: {
  code: string;
  title: string;
  partyA: string;
  partyB: string;
  amount: number;
  startDate: string;
  endDate: string;
  fileStorageKey?: string;
  orderId?: string;
  customerId?: string;
  contractType?: ContractType;
  incomeExpenseType?: IncomeExpenseType;
  signDate?: string;
  note?: string;
  description?: string;
  departmentId?: string;
  businessManager?: string;
  responsibleProject?: string;
  inputCustomer?: string;
  nextPaymentReminder?: boolean;
  contractExpiryReminder?: boolean;
  singleDoubleSeal?: number;
  invoicingInformation?: string;
  paymentStatus?: number;
  warrantyPeriod?: string;
  isInstallmentPayment?: boolean;
  accumulatedAmount?: number;
}) {
  return requestClient.post<{ id: string }>('/contracts', data);
}

/**
 * 更新合同
 */
async function updateContract(
  id: string,
  data: {
    code: string;
    title: string;
    partyA: string;
    partyB: string;
    amount: number;
    startDate: string;
    endDate: string;
    fileStorageKey?: string;
    orderId?: string;
    customerId?: string;
    contractType?: ContractType;
    incomeExpenseType?: IncomeExpenseType;
    signDate?: string;
    note?: string;
    description?: string;
    departmentId?: string;
    businessManager?: string;
    responsibleProject?: string;
    inputCustomer?: string;
    nextPaymentReminder?: boolean;
    contractExpiryReminder?: boolean;
    singleDoubleSeal?: number;
    invoicingInformation?: string;
    paymentStatus?: number;
    warrantyPeriod?: string;
    isInstallmentPayment?: boolean;
    accumulatedAmount?: number;
  },
) {
  return requestClient.put(`/contracts/${id}`, data);
}

/**
 * 删除合同（仅草稿可删，软删）
 */
async function deleteContract(id: string) {
  return requestClient.delete(`/contracts/${id}`);
}

/**
 * 提交合同审批
 */
async function submitContract(id: string) {
  return requestClient.post(`/contracts/${id}/submit`);
}

/**
 * 审批通过合同
 */
async function approveContract(id: string) {
  return requestClient.post(`/contracts/${id}/approve`);
}

/**
 * 归档合同
 */
async function archiveContract(id: string) {
  return requestClient.post(`/contracts/${id}/archive`);
}

/**
 * 新增合同发票
 */
async function createContractInvoice(contractId: string, data: ContractApi.CreateContractInvoicePayload) {
  return requestClient.post<{ id: string }>(`/contracts/${contractId}/invoices`, data);
}

/**
 * 更新合同发票
 */
async function updateContractInvoice(
  contractId: string,
  invoiceId: string,
  data: ContractApi.CreateContractInvoicePayload,
) {
  return requestClient.put(`/contracts/${contractId}/invoices/${invoiceId}`, data);
}

/**
 * 删除合同发票
 */
async function removeContractInvoice(contractId: string, invoiceId: string) {
  return requestClient.delete(`/contracts/${contractId}/invoices/${invoiceId}`);
}

export {
  getContractList,
  getContract,
  createContract,
  updateContract,
  deleteContract,
  submitContract,
  approveContract,
  archiveContract,
  createContractInvoice,
  updateContractInvoice,
  removeContractInvoice,
};
