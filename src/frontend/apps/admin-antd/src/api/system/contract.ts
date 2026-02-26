import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 合同状态：0 草稿 1 审批中 2 已生效 3 已归档 */
export type ContractStatus = 0 | 1 | 2 | 3;

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
  },
) {
  return requestClient.put(`/contracts/${id}`, data);
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

export {
  getContractList,
  getContract,
  createContract,
  updateContract,
  submitContract,
  approveContract,
  archiveContract,
};
