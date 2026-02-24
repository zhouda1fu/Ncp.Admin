import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace MeetingApi {
  /** 会议室状态：0 禁用 1 可用 */
  export type MeetingRoomStatus = 0 | 1;
  /** 预订状态 */
  export type MeetingBookingStatus = 0 | 1 | 2;

  export interface MeetingRoomItem {
    id: string;
    name: string;
    location?: string;
    capacity: number;
    equipment?: string;
    status: MeetingRoomStatus;
    createdAt: string;
  }

  export interface MeetingBookingItem {
    id: string;
    meetingRoomId: string;
    meetingRoomName: string;
    bookerId: string;
    title: string;
    startAt: string;
    endAt: string;
    status: MeetingBookingStatus;
    createdAt: string;
  }
}

/**
 * 获取会议室列表（分页）
 */
async function getMeetingRoomList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: MeetingApi.MeetingRoomItem[];
    total: number;
  }>('/meeting/rooms', { params });
  return res;
}

/**
 * 创建会议室
 */
async function createMeetingRoom(data: {
  name: string;
  location?: string;
  capacity: number;
  equipment?: string;
}) {
  return requestClient.post<{ id: string }>('/meeting/rooms', data);
}

/**
 * 获取会议室预订列表（分页）
 */
async function getMeetingBookingList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: MeetingApi.MeetingBookingItem[];
    total: number;
  }>('/meeting/bookings', { params });
  return res;
}

/**
 * 创建会议室预订
 */
async function createMeetingBooking(data: {
  meetingRoomId: string;
  title: string;
  startAt: string;
  endAt: string;
}) {
  return requestClient.post<{ id: string }>('/meeting/bookings', data);
}

export {
  createMeetingBooking,
  createMeetingRoom,
  getMeetingBookingList,
  getMeetingRoomList,
};
