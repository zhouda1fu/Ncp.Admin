import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace ExpenseApi {
  /** 报销单状态：0 草稿 1 已提交 2 已通过 3 已驳回 */
  export type ExpenseClaimStatus = 0 | 1 | 2 | 3;
  /** 费用类型：0 差旅 1 餐饮 2 住宿 3 办公 4 其他 */
  export type ExpenseType = 0 | 1 | 2 | 3 | 4;

  export interface ExpenseItemDto {
    id: string;
    type: ExpenseType;
    amount: number;
    description: string;
    invoiceUrl?: string;
  }

  export interface ExpenseClaimItem {
    id: string;
    applicantId: string;
    applicantName: string;
    totalAmount: number;
    status: ExpenseClaimStatus;
    workflowInstanceId?: string;
    createdAt: string;
    items: ExpenseItemDto[];
  }
}

/**
 * 获取报销单列表（分页）
 */
async function getExpenseClaimList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ExpenseApi.ExpenseClaimItem[];
    total: number;
  }>('/expense/claims', { params });
  return res;
}

/**
 * 获取报销单详情
 */
async function getExpenseClaim(id: string) {
  return requestClient.get<ExpenseApi.ExpenseClaimItem>(
    `/expense/claims/${id}`,
  );
}

/**
 * 创建报销单（草稿，含明细）
 */
async function createExpenseClaim(data: {
  items: Array<{
    type: number;
    amount: number;
    description: string;
    invoiceUrl?: string;
  }>;
}) {
  return requestClient.post<{ id: string }>('/expense/claims', data);
}

/**
 * 提交报销单
 */
async function submitExpenseClaim(id: string) {
  return requestClient.post(`/expense/claims/${id}/submit`);
}

export {
  createExpenseClaim,
  getExpenseClaim,
  getExpenseClaimList,
  submitExpenseClaim,
};
