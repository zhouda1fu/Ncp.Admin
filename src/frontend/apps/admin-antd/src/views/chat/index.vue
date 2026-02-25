<script lang="ts" setup>
import type { ChatApi } from '#/api/system/chat';
import type { ChatMessagePushDto } from '#/hooks/useChatHub';

import { ref, watch } from 'vue';

import { Page, useVbenModal } from '@vben/common-ui';

import {
  Button,
  Checkbox,
  Input,
  List,
  message as antMessage,
  Spin,
} from 'ant-design-vue';

import {
  createGroupChat,
  createSingleChat,
  getChatMessages,
  getMyChatGroups,
} from '#/api/system/chat';
import { getUserList } from '#/api/system/user';
import { useChatHub } from '#/hooks/useChatHub';
import { useUserStore } from '@vben/stores';
import { $t } from '#/locales';

interface UserItem {
  userId: string;
  name: string;
  realName?: string;
}

const userStore = useUserStore();
const groups = ref<ChatApi.ChatGroupItem[]>([]);
const currentGroupId = ref<string | null>(null);
const messages = ref<ChatApi.ChatMessageItem[]>([]);
const inputContent = ref('');
const loading = ref(false);
const sendLoading = ref(false);

const currentUserId = ref(String(userStore.userInfo?.userId ?? ''));

// 单聊：用户列表与选中
const singleUserList = ref<UserItem[]>([]);
const selectedSingleUserId = ref<string | null>(null);
const singleModalLoading = ref(false);

// 群聊：用户列表、群名称、选中的成员
const groupUserList = ref<UserItem[]>([]);
const groupName = ref('');
const selectedMemberIds = ref<string[]>([]);
const groupModalLoading = ref(false);

async function loadGroups() {
  loading.value = true;
  try {
    const res = await getMyChatGroups();
    groups.value = Array.isArray(res) ? res : [];
  } finally {
    loading.value = false;
  }
}

async function loadMessages(groupId: string) {
  if (!groupId) return;
  loading.value = true;
  try {
    const res = await getChatMessages(groupId, { pageIndex: 1, pageSize: 50 });
    const items = (res?.items ?? []) as ChatApi.ChatMessageItem[];
    messages.value = items
      .map((m) => ({
        ...m,
        senderId: String(m.senderId),
        senderName: (m as { senderName?: string; SenderName?: string }).senderName ?? (m as { senderName?: string; SenderName?: string }).SenderName ?? '',
      }))
      .reverse();
  } finally {
    loading.value = false;
  }
}

function normalizeGroupId(id: string | null | undefined) {
  return id?.toLowerCase()?.trim() ?? '';
}

function handleReceiveMessage(msg: ChatMessagePushDto) {
  if (normalizeGroupId((msg as any).chatGroupId ?? (msg as any).ChatGroupId) !== normalizeGroupId(currentGroupId.value)) return;
  const rawId = (msg as any).id ?? (msg as any).Id;
  const existing = messages.value.some((m) => m.id === rawId);
  if (existing) return;
  const senderStr = String((msg as any).senderId ?? (msg as any).SenderId ?? '');
  const senderName = (msg as any).senderName ?? (msg as any).SenderName ?? '';
  // 用「当前用户的 pending + 同内容 + 时间接近」识别为刚发出的消息，替换时强制用当前用户 id/姓名，避免服务端序列化导致左侧重复
  const pendingIdx = messages.value.findIndex(
    (m) =>
      m.id.startsWith('pending-') &&
      m.senderId === currentUserId.value &&
      m.content === msg.content &&
      Math.abs(new Date(m.createdAt).getTime() - new Date(msg.createdAt).getTime()) < 10000,
  );
  if (pendingIdx !== -1) {
    messages.value.splice(pendingIdx, 1, {
      id: rawId,
      chatGroupId: (msg as any).chatGroupId ?? (msg as any).ChatGroupId,
      senderId: currentUserId.value,
      senderName: currentUserDisplayName(),
      content: msg.content,
      replyToMessageId: msg.replyToMessageId,
      createdAt: msg.createdAt,
    });
    return;
  }
  messages.value.push({
    id: rawId,
    chatGroupId: (msg as any).chatGroupId ?? (msg as any).ChatGroupId,
    senderId: senderStr,
    senderName,
    content: msg.content,
    replyToMessageId: msg.replyToMessageId,
    createdAt: msg.createdAt,
  });
}

const { connect, joinGroup, leaveGroup, sendMessage } = useChatHub(handleReceiveMessage);

async function selectGroup(group: ChatApi.ChatGroupItem) {
  if (currentGroupId.value === group.id) return;
  if (currentGroupId.value) {
    await leaveGroup(currentGroupId.value);
  }
  currentGroupId.value = group.id;
  await joinGroup(group.id);
  await loadMessages(group.id);
}

function currentUserDisplayName() {
  const u = userStore.userInfo;
  return (u as { realName?: string; name?: string })?.realName ?? (u as { realName?: string; name?: string })?.name ?? currentUserId.value;
}

async function onSend() {
  const content = inputContent.value.trim();
  if (!content || !currentGroupId.value) return;
  const groupId = currentGroupId.value;
  const tempId = `pending-${Date.now()}`;
  const now = new Date().toISOString();
  messages.value.push({
    id: tempId,
    chatGroupId: groupId,
    senderId: currentUserId.value,
    senderName: currentUserDisplayName(),
    content,
    replyToMessageId: null,
    createdAt: now,
  });
  inputContent.value = '';
  sendLoading.value = true;
  try {
    await sendMessage(groupId, content);
  } catch (e) {
    antMessage.error(String(e));
    const idx = messages.value.findIndex((m) => m.id === tempId);
    if (idx !== -1) messages.value.splice(idx, 1);
  } finally {
    sendLoading.value = false;
  }
}

async function loadUsersForSingle() {
  singleModalLoading.value = true;
  try {
    const res = await getUserList({ pageIndex: 1, pageSize: 500 });
    const items = (res?.items ?? []) as UserItem[];
    singleUserList.value = items.filter((u) => u.userId !== currentUserId.value);
    selectedSingleUserId.value = null;
  } finally {
    singleModalLoading.value = false;
  }
}

async function loadUsersForGroup() {
  groupModalLoading.value = true;
  try {
    const res = await getUserList({ pageIndex: 1, pageSize: 500 });
    groupUserList.value = (res?.items ?? []) as UserItem[];
    groupName.value = '';
    selectedMemberIds.value = [];
  } finally {
    groupModalLoading.value = false;
  }
}

const [SingleModal, singleModalApi] = useVbenModal({
  onOpenChange(isOpen) {
    if (isOpen) loadUsersForSingle();
  },
  async onConfirm() {
    const uid = selectedSingleUserId.value;
    if (!uid) {
      antMessage.warning($t('chat.selectUser'));
      return;
    }
    singleModalApi.lock();
    try {
      const res = await createSingleChat(Number(uid));
      singleModalApi.close();
      await loadGroups();
      const g = groups.value.find((x) => x.id === res.id);
      if (g) await selectGroup(g);
    } catch (e) {
      antMessage.error(String(e));
    } finally {
      singleModalApi.lock(false);
    }
  },
});

const [GroupModal, groupModalApi] = useVbenModal({
  onOpenChange(isOpen) {
    if (isOpen) loadUsersForGroup();
  },
  async onConfirm() {
    const name = groupName.value.trim();
    if (!name) {
      antMessage.warning($t('chat.groupName'));
      return;
    }
    if (selectedMemberIds.value.length === 0) {
      antMessage.warning($t('chat.selectUsers'));
      return;
    }
    groupModalApi.lock();
    try {
      const memberIds = selectedMemberIds.value.map((id) => Number(id));
      const res = await createGroupChat({ name, memberIds });
      groupModalApi.close();
      await loadGroups();
      const g = groups.value.find((x) => x.id === res.id);
      if (g) await selectGroup(g);
    } catch (e) {
      antMessage.error(String(e));
    } finally {
      groupModalApi.lock(false);
    }
  },
});

function openSingleModal() {
  singleModalApi.open();
}

function openGroupModal() {
  groupModalApi.open();
}

function userDisplayName(u: UserItem) {
  return u.realName || u.name || u.userId;
}

loadGroups();
connect();

watch(
  () => userStore.userInfo?.userId,
  (id) => {
    currentUserId.value = String(id ?? '');
  },
);
</script>

<template>
  <Page auto-content-height>
    <!-- 单聊：选择单个用户 -->
    <SingleModal :title="$t('chat.createSingle')">
      <template #default>
        <div class="max-h-80 overflow-auto">
          <List
            :data-source="singleUserList"
            :loading="singleModalLoading"
            size="small"
          >
            <template #renderItem="{ item }">
              <List.Item
                class="cursor-pointer rounded-md px-2 py-2 transition-colors hover:bg-accent"
                :class="{
                  'bg-primary/15 text-primary dark:bg-primary/20': selectedSingleUserId === item.userId,
                }"
                @click="selectedSingleUserId = item.userId"
              >
                {{ userDisplayName(item) }}
                <template v-if="item.name && item.realName && item.name !== item.realName">
                  <span class="text-muted-foreground text-xs ml-1">({{ item.name }})</span>
                </template>
              </List.Item>
            </template>
          </List>
        </div>
      </template>
    </SingleModal>

    <!-- 群聊：群名称 + 多选用户 -->
    <GroupModal :title="$t('chat.createGroup')">
      <template #default>
        <div class="space-y-4">
          <div>
            <label class="mb-1 block text-sm text-foreground">{{ $t('chat.groupName') }}</label>
            <Input
              v-model:value="groupName"
              :placeholder="$t('chat.groupName')"
              allow-clear
              class="w-full"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm text-foreground">{{ $t('chat.selectUsers') }}</label>
            <Spin :spinning="groupModalLoading">
              <div class="max-h-60 overflow-auto rounded border border-border bg-card p-2">
                <Checkbox.Group v-model:value="selectedMemberIds" class="flex flex-col gap-1">
                  <Checkbox
                    v-for="u in groupUserList"
                    :key="u.userId"
                    :value="u.userId"
                    class="mx-0"
                  >
                    {{ userDisplayName(u) }}
                    <template v-if="u.name && u.realName && u.name !== u.realName">
                      <span class="text-muted-foreground text-xs">({{ u.name }})</span>
                    </template>
                  </Checkbox>
                </Checkbox.Group>
              </div>
            </Spin>
          </div>
        </div>
      </template>
    </GroupModal>

    <div
      class="flex h-full gap-4 overflow-hidden rounded-lg border border-border bg-card p-4 text-card-foreground"
    >
      <!-- 会话列表 -->
      <div class="flex w-72 shrink-0 flex-col border-r border-border pr-4">
        <div class="mb-2 flex items-center justify-between">
          <span class="font-medium text-foreground">{{ $t('chat.list') }}</span>
          <div class="flex gap-1">
            <Button type="primary" size="small" @click="openSingleModal">
              {{ $t('chat.singleChat') }}
            </Button>
            <Button size="small" @click="openGroupModal">{{ $t('chat.groupChat') }}</Button>
          </div>
        </div>
        <List
          v-if="groups.length"
          :data-source="groups"
          :loading="loading"
          class="flex-1 overflow-auto"
        >
          <template #renderItem="{ item }">
            <List.Item
              class="cursor-pointer rounded-md px-2 py-2 transition-colors hover:bg-accent hover:text-accent-foreground"
              :class="{
                'bg-primary/15 text-primary dark:bg-primary/20 dark:text-primary-foreground':
                  currentGroupId === item.id,
              }"
              @click="selectGroup(item)"
            >
              <div class="w-full truncate">
                {{ item.name || `会话 ${item.id.slice(0, 8)}` }}
              </div>
              <template #extra>
                <span class="text-xs text-muted-foreground">{{ item.memberCount }}人</span>
              </template>
            </List.Item>
          </template>
        </List>
        <div
          v-else
          class="flex flex-1 items-center justify-center text-muted-foreground"
        >
          {{ $t('chat.noConversation') }}
        </div>
      </div>
      <!-- 消息区 -->
      <div class="flex min-w-0 flex-1 flex-col">
        <template v-if="currentGroupId">
          <div
            class="mb-2 border-b border-border pb-2 font-medium text-foreground"
          >
            {{ groups.find((g) => g.id === currentGroupId)?.name || currentGroupId }}
          </div>
          <div
            class="flex-1 space-y-2 overflow-auto rounded-lg bg-muted/30 p-2 dark:bg-background-deep/50"
          >
            <div
              v-for="m in messages"
              :key="m.id"
              class="flex"
              :class="m.senderId === currentUserId ? 'justify-end' : 'justify-start'"
            >
              <div
                class="max-w-[80%] rounded-lg px-3 py-2"
                :class="
                  m.senderId === currentUserId
                    ? 'bg-primary text-primary-foreground'
                    : 'border border-border bg-card text-card-foreground shadow-sm'
                "
              >
                <div class="text-xs opacity-80">
                  {{ m.senderName || m.senderId }} · {{ new Date(m.createdAt).toLocaleString() }}
                </div>
                <div class="whitespace-pre-wrap break-words">{{ m.content }}</div>
              </div>
            </div>
            <div
              v-if="!messages.length && !loading"
              class="py-8 text-center text-muted-foreground"
            >
              {{ $t('chat.noMessages') }}
            </div>
          </div>
          <div class="mt-2 flex gap-2">
            <Input.TextArea
              v-model:value="inputContent"
              :placeholder="$t('chat.placeholder')"
              :rows="2"
              class="flex-1"
              @press-enter="(e: KeyboardEvent) => !e.shiftKey && onSend()"
            />
            <Button type="primary" :loading="sendLoading" @click="onSend">
              {{ $t('chat.send') }}
            </Button>
          </div>
        </template>
        <div
          v-else
          class="flex flex-1 items-center justify-center text-muted-foreground"
        >
          {{ $t('chat.noConversation') }}
        </div>
      </div>
    </div>
  </Page>
</template>
