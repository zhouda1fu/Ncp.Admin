<script lang="ts" setup>
import type { NotificationItem } from '@vben/layouts';

import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import { AuthenticationLoginExpiredModal } from '@vben/common-ui';
import { VBEN_DOC_URL, VBEN_GITHUB_URL } from '@vben/constants';
import { useWatermark } from '@vben/hooks';
import { BookOpenText, CircleHelp, SvgGithubIcon } from '@vben/icons';
import {
  BasicLayout,
  LockScreen,
  Notification,
  UserDropdown,
} from '@vben/layouts';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { openWindow } from '@vben/utils';

import {
  deleteNotification,
  getNotificationList,
  markAllNotificationsRead,
  markNotificationRead,
} from '#/api/notification';
import { useNotificationHub } from '#/hooks/useNotificationHub';
import { $t } from '#/locales';
import { useAuthStore } from '#/store';
import LoginForm from '#/views/_core/authentication/login.vue';

const notifications = ref<NotificationItem[]>([]);
const loading = ref(false);

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
  businessId?: string;
  businessType?: string;
}): NotificationItem {
  return {
    id: item.id,
    title: item.title,
    message: item.content,
    date: formatNotificationDate(item.createdAt),
    isRead: item.isRead,
    avatar: preferences.app.defaultAvatar,
    link: item.businessId && item.businessType
      ? `/workflow/instance/${item.businessId}`
      : undefined,
  };
}

async function loadNotifications() {
  if (loading.value) return;
  loading.value = true;
  try {
    const res = await getNotificationList({
      pageIndex: 1,
      pageSize: 20,
    });
    notifications.value = (res.items || []).map(mapToLayoutItem);
  } finally {
    loading.value = false;
  }
}

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const accessStore = useAccessStore();
const { destroyWatermark, updateWatermark } = useWatermark();
const showDot = computed(() =>
  notifications.value.some((item) => !item.isRead),
);

watch(
  () => accessStore.accessToken,
  (token) => {
    if (token) {
      loadNotifications();
    } else {
      notifications.value = [];
    }
  },
  { immediate: true },
);

// SignalR 实时推送：收到新通知时刷新列表
useNotificationHub(loadNotifications);

const menus = computed(() => [
  {
    handler: () => {
      router.push({ name: 'Profile' });
    },
    icon: 'lucide:user',
    text: $t('page.auth.profile'),
  },
  {
    handler: () => {
      openWindow(VBEN_DOC_URL, {
        target: '_blank',
      });
    },
    icon: BookOpenText,
    text: $t('ui.widgets.document'),
  },
  {
    handler: () => {
      openWindow(VBEN_GITHUB_URL, {
        target: '_blank',
      });
    },
    icon: SvgGithubIcon,
    text: 'GitHub',
  },
  {
    handler: () => {
      openWindow(`${VBEN_GITHUB_URL}/issues`, {
        target: '_blank',
      });
    },
    icon: CircleHelp,
    text: $t('ui.widgets.qa'),
  },
]);

const avatar = computed(() => {
  return userStore.userInfo?.avatar ?? preferences.app.defaultAvatar;
});

async function handleLogout() {
  await authStore.logout(false);
}

function handleNoticeClear() {
  notifications.value = [];
}

async function markRead(id: number | string) {
  try {
    await markNotificationRead(id);
    const item = notifications.value.find((item) => item.id === id);
    if (item) {
      item.isRead = true;
    }
  } catch {
    // ignore
  }
}

async function remove(id: number | string) {
  try {
    await deleteNotification(id);
    notifications.value = notifications.value.filter((item) => item.id !== id);
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
    notifications.value.forEach((item) => (item.isRead = true));
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
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="userStore.userInfo?.realName"
        description="ann.vben@gmail.com"
        tag-text="Pro"
        @logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
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
</template>
