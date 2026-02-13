/**
 * 通知 API
 * 后端路径为 /api/notification，相对 baseURL /api/admin 使用 ../notification
 */
import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

const NOTIFICATION_BASE = '../notification';

export namespace NotificationApi {
  export type NotificationType = 0 | 1 | 2; // System, Workflow, Announcement
  export type NotificationLevel = 0 | 1 | 2 | 3; // Info, Success, Warning, Error

  export interface NotificationItem {
    id: string | number;
    title: string;
    content: string;
    type: NotificationType;
    level: NotificationLevel;
    senderName: string;
    isRead: boolean;
    readAt?: string;
    businessId?: string;
    businessType?: string;
    createdAt: string;
  }
}

/**
 * 获取通知列表
 */
async function getNotificationList(params?: Recordable<any>) {
  return requestClient.get<{
    items: NotificationApi.NotificationItem[];
    total: number;
    unreadCount: number;
  }>(NOTIFICATION_BASE, { params });
}

/**
 * 获取未读数量
 */
async function getUnreadCount() {
  return requestClient.get<{ count: number }>(
    `${NOTIFICATION_BASE}/unread-count`,
  );
}

/**
 * 标记单条已读
 */
async function markNotificationRead(id: string | number) {
  return requestClient.put<boolean>(`${NOTIFICATION_BASE}/${id}/read`);
}

/**
 * 标记全部已读
 */
async function markAllNotificationsRead() {
  return requestClient.put<{ count: number }>(
    `${NOTIFICATION_BASE}/read-all`,
  );
}

/**
 * 删除通知
 */
async function deleteNotification(id: string | number) {
  return requestClient.delete<boolean>(`${NOTIFICATION_BASE}/${id}`);
}

export {
  getNotificationList,
  getUnreadCount,
  markNotificationRead,
  markAllNotificationsRead,
  deleteNotification,
};
