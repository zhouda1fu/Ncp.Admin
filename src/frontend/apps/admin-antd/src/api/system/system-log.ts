import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export type SystemLogLevel =
  | 'Critical'
  | 'Debug'
  | 'Error'
  | 'Information'
  | 'Trace'
  | 'Warning';

export namespace SystemLogApi {
  export interface SystemLogItem {
    id: number;
    timestamp: string;
    level: SystemLogLevel | string;
    category: string;
    eventId?: number | null;
    message: string;
    hasException: boolean;
    traceId?: string | null;
    userId?: string | null;
    requestPath?: string | null;
    clientIp?: string | null;
  }

  export interface SystemLogDetail extends SystemLogItem {
    exception?: string | null;
    propertiesJson?: string | null;
    createdAt: string;
  }

  export interface SystemLogOptions {
    levels: string[];
    categories: string[];
  }
}

export interface GetSystemLogListParams extends Recordable<any> {
  pageIndex?: number;
  pageSize?: number;
  countTotal?: boolean;
  level?: string;
  category?: string;
  keyword?: string;
  traceId?: string;
  hasException?: boolean;
  startTime?: string;
  endTime?: string;
}

async function getSystemLogList(params: GetSystemLogListParams) {
  return requestClient.get<{
    items: SystemLogApi.SystemLogItem[];
    total: number;
  }>('/system-logs', { params });
}

async function getSystemLogDetail(id: number) {
  return requestClient.get<SystemLogApi.SystemLogDetail>(`/system-logs/${id}`);
}

async function getSystemLogOptions() {
  return requestClient.get<SystemLogApi.SystemLogOptions>('/system-logs/options');
}

export { getSystemLogDetail, getSystemLogList, getSystemLogOptions };
