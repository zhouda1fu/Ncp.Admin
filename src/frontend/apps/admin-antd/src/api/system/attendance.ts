import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace AttendanceApi {
  /** 打卡来源：0 GPS 1 WiFi 2 手动 */
  export type AttendanceSource = 0 | 1 | 2;

  export interface AttendanceRecordItem {
    id: string;
    userId: string;
    checkInAt: string;
    checkOutAt?: string;
    source: AttendanceSource;
    location?: string;
    createdAt: string;
  }

  export interface ScheduleItem {
    id: string;
    userId: string;
    workDate: string;
    startTime: string;
    endTime: string;
    shiftName?: string;
    createdAt: string;
  }
}

/**
 * 获取考勤记录列表（分页）
 */
async function getAttendanceRecordList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: AttendanceApi.AttendanceRecordItem[];
    total: number;
  }>('/attendance/records', { params });
  return res;
}

/**
 * 获取排班列表（分页）
 */
async function getScheduleList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: AttendanceApi.ScheduleItem[];
    total: number;
  }>('/attendance/schedules', { params });
  return res;
}

/**
 * 创建排班
 */
async function createSchedule(data: {
  userId: number;
  workDate: string;
  startTime: string;
  endTime: string;
  shiftName?: string;
}) {
  return requestClient.post<{ id: string }>('/attendance/schedules', data);
}

/**
 * 更新排班
 */
async function updateSchedule(
  id: string,
  data: { startTime: string; endTime: string; shiftName?: string },
) {
  return requestClient.put(`/attendance/schedules/${id}`, data);
}

/**
 * 签到
 */
async function checkIn(data?: { source?: number; location?: string }) {
  return requestClient.post<{ id: string; checkInAt: string }>(
    '/attendance/check-in',
    data ?? {},
  );
}

/**
 * 签退
 */
async function checkOut() {
  return requestClient.post('/attendance/check-out');
}

export {
  checkIn,
  checkOut,
  createSchedule,
  getAttendanceRecordList,
  getScheduleList,
  updateSchedule,
};
