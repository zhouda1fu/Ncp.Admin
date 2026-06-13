<script lang="ts" setup>
import type { NotificationItem } from '@vben/layouts';

import { computed, onUnmounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import { AuthenticationLoginExpiredModal } from '@vben/common-ui';
import { useWatermark } from '@vben/hooks';
import {
  BasicLayout,
  LockScreen,
  Notification,
  UserDropdown,
} from '@vben/layouts';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { message } from 'ant-design-vue';
import {
  deleteNotification,
  getNotificationList,
  getUnreadCount,
  markAllNotificationsRead,
  markNotificationRead,
} from '#/api/notification';
import { useNotificationHub } from '#/hooks/useNotificationHub';
import { $t } from '#/locales';
import { useAuthStore } from '#/store';
import { notificationLinkFromItem } from '#/utils/notification-navigation';
import {
  buildNotificationPublisherLine,
  formatNotificationDateTime,
} from '#/utils/notification-display';
import LoginForm from '#/views/_core/authentication/login.vue';

const notifications = ref<NotificationItem[]>([]);
/** 未读总数（与列表分页无关，用于角标） */
const unreadCount = ref(0);
const loading = ref(false);
/** 头部通知下拉显隐（有未读时首次进入 / SignalR 新通知时主动展开） */
const notificationPopoverOpen = ref(false);
/** 避免 accessToken 静默续期时反复自动弹出：仅「本轮未登录→已登录」视为新会话拉取 */
const wasLoggedIn = ref(false);
function formatNotificationDate(createdAt: string) {
  if (!createdAt) return '';
  const date = new Date(createdAt);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);
  if (diffMins < 1) return $t('page.ui.widgets.justNow');
  if (diffMins < 60) return `${diffMins}${$t('page.ui.widgets.minutesAgo')}`;
  if (diffHours < 24) return `${diffHours}${$t('page.ui.widgets.hoursAgo')}`;
  if (diffDays < 7) return `${diffDays}${$t('page.ui.widgets.daysAgo')}`;
  return date.toLocaleDateString();
}

function mapToLayoutItem(item: {
  id: string | number;
  title: string;
  content: string;
  isRead: boolean;
  createdAt: string;
  senderName?: string;
  businessId?: string;
  businessType?: string;
  linkPath?: string | null;
  linkQuery?: Record<string, string> | null;
}): NotificationItem {
  return {
    id: item.id,
    title: item.title,
    message: item.content,
    publisherLine: buildNotificationPublisherLine(item.senderName),
    dateTime: formatNotificationDateTime(item.createdAt),
    date: formatNotificationDate(item.createdAt),
    isRead: item.isRead,
    avatar: preferences.app.defaultAvatar,
    ...notificationLinkFromItem(item),
  };
}

let loadNotificationsPending = false;

/** SignalR 推送可能略早于读库可见，带退避重试拉取未读列表 */
async function loadNotificationsWithRetry(
  options?: { autoOpenIfUnread?: boolean },
  maxAttempts = 5,
) {
  const previousCount = unreadCount.value;
  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    await loadNotifications(
      attempt === 0 ? options : { autoOpenIfUnread: false },
    );
    if (unreadCount.value > previousCount) {
      if (
        options?.autoOpenIfUnread
        && unreadCount.value > 0
      ) {
        notificationPopoverOpen.value = true;
      }
      return;
    }
    if (attempt < maxAttempts - 1) {
      await new Promise((resolve) => setTimeout(resolve, 300 * (attempt + 1)));
    }
  }
}

async function loadNotifications(options?: { autoOpenIfUnread?: boolean }) {
  if (loading.value) {
    loadNotificationsPending = true;
    return;
  }
  loading.value = true;
  try {
    const res = await getNotificationList({
      pageIndex: 1,
      pageSize: 20,
      /** 头部下拉仅展示未读；已读后由接口排除 */
      isRead: false,
      includeUnreadCount: true,
    });
    unreadCount.value = res.unreadCount ?? 0;
    notifications.value = (res.items || []).map(mapToLayoutItem);
    if (
      options?.autoOpenIfUnread
      && unreadCount.value > 0
    ) {
      notificationPopoverOpen.value = true;
    }
  } finally {
    loading.value = false;
    if (loadNotificationsPending) {
      loadNotificationsPending = false;
      await loadNotifications(options);
    }
  }
}

/** Hub 未连接时轮询未读数，避免 WebSocket/反向代理异常时角标不更新 */
async function refreshUnreadBadge() {
  if (!accessStore.accessToken || !accessStore.isAccessChecked) {
    return;
  }
  try {
    const res = await getUnreadCount();
    const count = res.count ?? 0;
    if (count !== unreadCount.value) {
      unreadCount.value = count;
      if (count > 0) {
        await loadNotifications();
      } else {
        notifications.value = [];
      }
    }
  } catch {
    // ignore
  }
}

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const accessStore = useAccessStore();
const { destroyWatermark, updateWatermark } = useWatermark();
const showDot = computed(() => unreadCount.value > 0);
watch(
  () => [accessStore.accessToken, accessStore.isAccessChecked] as const,
  async ([token, accessChecked]) => {
    if (token && accessChecked) {
      const isFreshLogin = !wasLoggedIn.value;
      wasLoggedIn.value = true;
      await loadNotifications({
        autoOpenIfUnread: isFreshLogin,
      });
    } else if (!token) {
      wasLoggedIn.value = false;
      notifications.value = [];
      unreadCount.value = 0;
      notificationPopoverOpen.value = false;
    }
  },
  { immediate: true },
);

// SignalR 实时推送：收到新通知时刷新列表与弹窗队列；有待弹窗时不自动展开通知下拉
useNotificationHub(async () => {
  await loadNotificationsWithRetry({
    autoOpenIfUnread: true,
  });
}, async () => {
  message.warning('账号已在其他设备登录，当前会话已退出');
  await authStore.forceLogout();
});

/** 登录后定期轮询未读数（Hub 失败或生产未配置 WebSocket 代理时的兜底） */
const NOTIFICATION_POLL_MS = 10_000;
let notificationPollTimer: ReturnType<typeof setInterval> | null = null;

function stopNotificationPoll() {
  if (notificationPollTimer) {
    clearInterval(notificationPollTimer);
    notificationPollTimer = null;
  }
}

function startNotificationPoll() {
  stopNotificationPoll();
  if (!accessStore.accessToken || !accessStore.isAccessChecked) {
    return;
  }
  notificationPollTimer = setInterval(() => {
    void refreshUnreadBadge();
  }, NOTIFICATION_POLL_MS);
}

watch(
  () => [accessStore.accessToken, accessStore.isAccessChecked] as const,
  ([token, accessChecked]) => {
    stopNotificationPoll();
    if (token && accessChecked) {
      startNotificationPoll();
    }
  },
  { immediate: true },
);

onUnmounted(() => {
  stopNotificationPoll();
});

const menus = computed(() => [
  {
    handler: () => {
      router.push({ name: 'Profile' });
    },
    icon: 'lucide:user',
    text: $t('page.auth.profile'),
  },
]);

const avatar = computed(() => {
  const info = userStore.userInfo as Record<string, unknown> | undefined;
  const url = (info?.avatar as string | undefined) ?? '';
  if (
    url.startsWith('blob:')
    || url.startsWith('data:')
    || url.startsWith('http://')
    || url.startsWith('https://')
  ) {
    return url;
  }
  return preferences.app.defaultAvatar;
});

const userDescription = computed(() => {
  const info = userStore.userInfo as Record<string, unknown> | undefined;
  return (
    (info?.email as string | undefined) ||
    (info?.username as string | undefined) ||
    (userStore.userInfo?.username as string | undefined) ||
    ''
  );
});

async function handleLogout() {
  await authStore.logout(false);
}

/** 清空：将所有未读通知标为已读（与信封「全部标已读」一致），并同步角标 */
async function handleNoticeClear() {
  await handleMakeAll();
}

async function markRead(id: number | string) {
  try {
    await markNotificationRead(id);
    notifications.value = notifications.value.filter(
      (item) => String(item.id) !== String(id),
    );
    unreadCount.value = Math.max(0, unreadCount.value - 1);
  } catch {
    // ignore
  }
}

async function remove(id: number | string) {
  try {
    await deleteNotification(id);
    const existed = notifications.value.some((item) => String(item.id) === String(id));
    notifications.value = notifications.value.filter(
      (item) => String(item.id) !== String(id),
    );
    if (existed) unreadCount.value = Math.max(0, unreadCount.value - 1);
  } catch {
    // ignore - error already shown by request interceptor
  }
}

function handleViewAll() {
  router.push('/workflow/pending');
}

async function handleMakeAll() {
  try {
    await markAllNotificationsRead();
    notifications.value = [];
    unreadCount.value = 0;
  } catch {
    // ignore
  }
}

watch(
  () => ({
    enable: preferences.app.watermark,
    content: preferences.app.watermarkContent,
  }),
  async ({ enable, content }) => {
    if (enable) {
      await updateWatermark({
        content:
          content ||
          `${userStore.userInfo?.username} - ${userStore.userInfo?.realName}`,
      });
    } else {
      destroyWatermark();
    }
  },
  {
    immediate: true,
  },
);
</script>

<template>
  <BasicLayout @clear-preferences-and-logout="handleLogout">
    <template #logo-text>
      <span></span>
    </template>
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="userStore.userInfo?.realName"
        :description="userDescription"
        :on-logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
        v-model:open="notificationPopoverOpen"
        :dot="showDot"
        :notifications="notifications"
        @clear="handleNoticeClear"
        @read="(item) => item?.id != null && markRead(item.id)"
        @remove="(item) => item?.id != null && remove(item.id)"
        @make-all="handleMakeAll"
        @view-all="handleViewAll"
      />
    </template>
    <template #extra>
      <AuthenticationLoginExpiredModal
        v-model:open="accessStore.loginExpired"
        :avatar
      >
        <LoginForm />
      </AuthenticationLoginExpiredModal>
    </template>
    <template #lock-screen>
      <LockScreen :avatar @to-login="handleLogout" />
    </template>
  </BasicLayout>
  <!-- 须在 BasicLayout 外渲染：未放入具名插槽的子节点不会挂载，弹窗逻辑不会执行 -->
</template>
