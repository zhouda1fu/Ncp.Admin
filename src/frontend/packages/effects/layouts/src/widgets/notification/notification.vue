<script lang="ts" setup>
import type { NotificationItem } from './types';

import { useRouter } from 'vue-router';

import { Bell, CircleCheckBig, CircleX, MailCheck } from '@vben/icons';
import { $t } from '@vben/locales';

import {
  VbenButton,
  VbenIconButton,
  VbenPopover,
  VbenScrollbar,
} from '@vben-core/shadcn-ui';

interface Props {
  /**
   * 显示圆点
   */
  dot?: boolean;
  /**
   * 消息列表
   */
  notifications?: NotificationItem[];
}

defineOptions({ name: 'NotificationPopup' });

withDefaults(defineProps<Props>(), {
  dot: false,
  notifications: () => [],
});

/** 与 VbenPopover 同步；父级可 v-model:open 以在「有未读 / 新通知」时主动展开 */
const open = defineModel<boolean>('open', { default: false });

const emit = defineEmits<{
  clear: [];
  makeAll: [];
  read: [NotificationItem];
  remove: [NotificationItem];
  viewAll: [];
}>();

const router = useRouter();

function close() {
  open.value = false;
}

function toggleOpen() {
  open.value = !open.value;
}

function handleViewAll() {
  emit('viewAll');
  close();
}

function handleMakeAll() {
  emit('makeAll');
}

function handleClear() {
  emit('clear');
}

function handleClick(item: NotificationItem) {
  // 有链接：跳转并由父级标记已读；无链接的未读项点击也标记已读（无跳转）
  if (item.link) {
    navigateTo(item.link, item.query, item.state);
    if (!item.isRead) {
      emit('read', item);
    }
    close();
  } else if (!item.isRead) {
    emit('read', item);
    close();
  }
}

function navigateTo(
  link: string,
  query?: Record<string, any>,
  state?: Record<string, any>,
) {
  if (link.startsWith('http://') || link.startsWith('https://')) {
    // 外部链接，在新标签页打开
    window.open(link, '_blank');
  } else {
    // 内部路由链接，支持 query 参数和 state
    router.push({
      path: link,
      query: query || {},
      state,
    });
  }
}

const FALLBACK_AVATAR =
  'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="%239ca3af"%3E%3Cpath d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/%3E%3C/svg%3E';

function handleAvatarError(e: Event) {
  const target = e.target as HTMLImageElement;
  if (target) target.src = FALLBACK_AVATAR;
}
</script>
<template>
  <VbenPopover
    v-model:open="open"
    content-class="relative right-2 w-[380px] p-0"
  >
    <template #trigger>
      <div class="flex-center mr-2 h-full" @click.stop="toggleOpen">
        <VbenIconButton class="bell-button text-foreground relative">
          <span
            v-if="dot"
            class="bg-primary absolute right-0.5 top-0.5 h-2 w-2 rounded"
          ></span>
          <Bell class="size-4" />
        </VbenIconButton>
      </div>
    </template>

    <div class="relative">
      <div class="flex items-center justify-between p-4 py-3">
        <div class="text-foreground text-sm font-medium">{{ $t('ui.widgets.notifications') }}</div>
        <VbenIconButton
          :disabled="notifications.length <= 0"
          :tooltip="$t('ui.widgets.markAllAsRead')"
          @click="handleMakeAll"
        >
          <MailCheck class="size-4" />
        </VbenIconButton>
      </div>
      <VbenScrollbar v-if="notifications.length > 0">
        <ul class="!flex max-h-[360px] w-full flex-col">
          <template v-for="item in notifications" :key="item.id ?? item.title">
            <li
              class="hover:bg-accent border-border relative flex w-full cursor-pointer items-start gap-5 border-t px-3 py-3"
              @click="handleClick(item)"
            >
              <span
                v-if="!item.isRead"
                class="bg-primary absolute right-2 top-2 h-2 w-2 rounded"
              ></span>

              <span
                class="relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full bg-muted"
              >
                <img
                  :src="item.avatar"
                  class="aspect-square h-full w-full object-cover"
                  @error="handleAvatarError"
                />
              </span>
              <div class="flex min-w-0 flex-1 flex-col gap-1.5 pr-10 leading-normal">
                <div
                  v-if="item.publisherLine || item.dateTime"
                  class="text-muted-foreground flex items-start justify-between gap-2 text-sm"
                >
                  <span class="truncate">{{ item.publisherLine }}</span>
                  <span class="shrink-0 whitespace-nowrap">{{ item.dateTime }}</span>
                </div>
                <p class="text-muted-foreground line-clamp-3 text-sm leading-relaxed">
                  <span v-if="item.title" class="font-semibold text-foreground">{{ item.title }}</span>
                  <template v-if="item.title && item.message">&nbsp;</template>
                  {{ item.message }}
                </p>
                <p
                  v-if="!item.publisherLine && !item.dateTime"
                  class="text-muted-foreground line-clamp-2 text-sm"
                >
                  {{ item.date }}
                </p>
              </div>
              <div
                class="absolute right-3 top-1/2 flex -translate-y-1/2 flex-col gap-2"
              >
                <VbenIconButton
                  v-if="!item.isRead"
                  size="xs"
                  variant="ghost"
                  class="h-6 px-2"
                  :tooltip="$t('common.confirm')"
                  @click.stop="emit('read', item)"
                >
                  <CircleCheckBig class="size-4" />
                </VbenIconButton>
                <VbenIconButton
                  v-if="item.isRead"
                  size="xs"
                  variant="ghost"
                  class="text-destructive h-6 px-2"
                  :tooltip="$t('common.delete')"
                  @click.stop="emit('remove', item)"
                >
                  <CircleX class="size-4" />
                </VbenIconButton>
              </div>
            </li>
          </template>
        </ul>
      </VbenScrollbar>

      <template v-else>
        <div class="flex-center text-muted-foreground min-h-[150px] w-full">
          {{ $t('common.noData') }}
        </div>
      </template>

      <div
        class="border-border flex items-center justify-between border-t px-4 py-3"
      >
        <VbenButton
          :disabled="notifications.length <= 0"
          size="sm"
          variant="ghost"
          @click="handleClear"
        >
          {{ $t('ui.widgets.clearNotifications') }}
        </VbenButton>
        <VbenButton size="sm" @click="handleViewAll">
          {{ $t('ui.widgets.viewAll') }}
        </VbenButton>
      </div>
    </div>
  </VbenPopover>
</template>

<style scoped>
:deep(.bell-button) {
  &:hover {
    svg {
      animation: bell-ring 1s both;
    }
  }
}

@keyframes bell-ring {
  0%,
  100% {
    transform-origin: top;
  }

  15% {
    transform: rotateZ(10deg);
  }

  30% {
    transform: rotateZ(-10deg);
  }

  45% {
    transform: rotateZ(5deg);
  }

  60% {
    transform: rotateZ(-5deg);
  }

  75% {
    transform: rotateZ(2deg);
  }
}
</style>
