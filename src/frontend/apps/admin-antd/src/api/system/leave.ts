import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace LeaveApi {
  /** 请假类型：0年假 1事假 2病假 3调休 */
  export type LeaveType = 0 | 1 | 2 | 3;
  /** 状态：0草稿 1待审批 2已通过 3已驳回 4已撤销 */
  export type LeaveRequestStatus = 0 | 1 | 2 | 3 | 4;

  export interface LeaveRequestItem {
    id: string;
    applicantId: string;
    applicantName: string;
    leaveType: LeaveType;
    startDate: string;
    endDate: string;
    days: number;
    reason: string;
    status: LeaveRequestStatus;
    workflowInstanceId?: string;
    createdAt: string;
  }

  export interface LeaveBalanceItem {
    id: string;
    userId: string;
    year: number;
    leaveType: LeaveType;
    totalDays: number;
    usedDays: number;
    remainingDays: number;
    createdAt: string;
  }
}

/**
 * 获取请假申请列表（分页）
 */
async function getLeaveRequestList(params: Recordable<any>) {
  const res = await requestClient.get<{ items: LeaveApi.LeaveRequestItem[]; total: number }>(
    '/leave/requests',
    { params },
  );
  return res;
}

/**
 * 获取请假申请详情
 */
async function getLeaveRequest(id: string) {
  return requestClient.get<LeaveApi.LeaveRequestItem>(`/leave/requests/${id}`);
}

/**
 * 创建请假申请（草稿）
 */
async function createLeaveRequest(data: {
  leaveType: LeaveApi.LeaveType;
  startDate: string;
  endDate: string;
  days: number;
  reason?: string;
}) {
  return requestClient.post<{ id: string }>('/leave/requests', data);
}

/**
 * 更新请假申请（仅草稿）
 */
async function updateLeaveRequest(
  id: string,
  data: {
    leaveType: LeaveApi.LeaveType;
    startDate: string;
    endDate: string;
    days: number;
    reason?: string;
  },
) {
  return requestClient.put(`/leave/requests/${id}`, data);
}

/**
 * 提交请假申请（发起审批，流程按分类内置，无需选择）
 */
async function submitLeaveRequest(id: string, remark?: string) {
  return requestClient.post(`/leave/requests/${id}/submit`, {
    remark: remark ?? '',
  });
}

/**
 * 撤销请假申请
 */
async function cancelLeaveRequest(id: string) {
  return requestClient.post(`/leave/requests/${id}/cancel`, { id });
}

/**
 * 获取请假余额列表（分页）
 */
async function getLeaveBalanceList(params: Recordable<any>) {
  const res = await requestClient.get<{ items: LeaveApi.LeaveBalanceItem[]; total: number }>(
    '/leave/balances',
    { params },
  );
  return res;
}

/**
 * 按用户+年度获取请假余额
 */
async function getLeaveBalanceByUserYear(userId: string, year: number) {
  return requestClient.get<LeaveApi.LeaveBalanceItem[]>(
    `/leave/balances/user/${userId}/year/${year}`,
  );
}

/**
 * 设置请假余额
 */
async function setLeaveBalance(data: {
  userId: string;
  year: number;
  leaveType: LeaveApi.LeaveType;
  totalDays: number;
}) {
  return requestClient.post<{ id: string }>('/leave/balances', data);
}

export {
  cancelLeaveRequest,
  createLeaveRequest,
  getLeaveBalanceByUserYear,
  getLeaveBalanceList,
  getLeaveRequest,
  getLeaveRequestList,
  setLeaveBalance,
  submitLeaveRequest,
  updateLeaveRequest,
};
