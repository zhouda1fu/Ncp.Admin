import { $t } from '#/locales';

/** 与后端 NotificationSenderDisplayName.SystemStoredName 一致 */
export const NOTIFICATION_SYSTEM_STORED_SENDER_NAME = '系统';

/** 与后端 NotificationSenderDisplayName.DisplayName 一致 */
export const NOTIFICATION_SYSTEM_DISPLAY_SENDER_NAME = 'OA系统';

/** 是否为系统发起或无发送人（含后端已解析为 OA系统 的情况）。 */
export function isNotificationSystemSender(senderName?: string | null): boolean {
  const trimmed = senderName?.trim() ?? '';
  return (
    !trimmed
    || trimmed === NOTIFICATION_SYSTEM_STORED_SENDER_NAME
    || trimmed === NOTIFICATION_SYSTEM_DISPLAY_SENDER_NAME
  );
}

/** 通知发送人展示名（系统通知走 i18n，与后端 Resolve 规则对齐）。 */
export function formatNotificationSenderName(senderName?: string | null): string {
  if (isNotificationSystemSender(senderName)) {
    return $t('page.dashboard.home.notify.systemPublisher');
  }
  return (senderName ?? '').trim();
}

/** 通知时间展示（绝对时间，用于弹窗/列表） */
export function formatNotificationDateTime(createdAt?: string | null): string {
  if (!createdAt?.trim()) return '';
  const date = new Date(createdAt);
  if (Number.isNaN(date.getTime())) return createdAt;
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
}

/** 站内通知弹窗「发送人」行（左上角） */
export function buildNotificationPublisherLine(senderName?: string | null): string {
  const publisher = formatNotificationSenderName(senderName);
  if (!publisher) return '';
  return `${$t('page.ui.widgets.notificationPublisher')}：${publisher}`;
}
