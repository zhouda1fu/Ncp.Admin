import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace AnnouncementApi {
  /** 状态：0 草稿 1 已发布 */
  export type AnnouncementStatus = 0 | 1;

  export interface AnnouncementItem {
    id: string;
    title: string;
    content: string;
    publisherId: string;
    publisherName: string;
    status: AnnouncementStatus;
    publishAt?: string;
    createdAt: string;
    isRead?: boolean;
  }
}

/**
 * 获取公告列表（分页）
 */
async function getAnnouncementList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: AnnouncementApi.AnnouncementItem[];
    total: number;
  }>('/announcements', { params });
  return res;
}

/**
 * 获取公告详情
 */
async function getAnnouncement(id: string) {
  return requestClient.get<AnnouncementApi.AnnouncementItem>(`/announcements/${id}`);
}

/**
 * 创建公告（草稿）
 */
async function createAnnouncement(data: { title: string; content: string }) {
  return requestClient.post<{ id: string }>('/announcements', data);
}

/**
 * 更新公告（仅草稿）
 */
async function updateAnnouncement(id: string, data: { title: string; content: string }) {
  return requestClient.put(`/announcements/${id}`, data);
}

/**
 * 发布公告
 */
async function publishAnnouncement(id: string) {
  return requestClient.post(`/announcements/${id}/publish`);
}

/**
 * 标记公告已读
 */
async function markAnnouncementRead(id: string) {
  return requestClient.post(`/announcements/${id}/read`);
}

export {
  createAnnouncement,
  getAnnouncement,
  getAnnouncementList,
  markAnnouncementRead,
  publishAnnouncement,
  updateAnnouncement,
};
