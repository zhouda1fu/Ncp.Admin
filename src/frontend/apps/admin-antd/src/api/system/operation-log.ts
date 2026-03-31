import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 操作类型：0 创建 1 更新 2 删除 3 提交 4 审批 5 其他 */
export type OperationLogType = 0 | 1 | 2 | 3 | 4 | 5;

export namespace OperationLogApi {
  export interface OperationLogItem {
    id: string;
    operatorUserId: number;
    operatorUserName: string;
    module: string;
    operationType: OperationLogType;
    requestPath: string;
    requestMethod: string;
    httpStatusCode: number;
    isSuccess: boolean;
    ipAddress: string;
    userAgent: string;
    requestBody: string;
    responseBody: string;
    durationMs: number;
    createdAt: string;
  }
}

export interface GetOperationLogListParams extends Recordable<any> {
  pageIndex?: number;
  pageSize?: number;
  countTotal?: boolean;
  operatorUserId?: number;
  module?: string;
  operationType?: OperationLogType;
  startTime?: string;
  endTime?: string;
}

/**
 * 获取操作日志列表（分页）
 */
async function getOperationLogList(params: GetOperationLogListParams) {
  const res = await requestClient.get<{
    items: OperationLogApi.OperationLogItem[];
    total: number;
  }>('/operation-logs', { params });
  return res;
}

export { getOperationLogList };
