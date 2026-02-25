import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace ChatApi {
  export interface ChatGroupItem {
    id: string;
    name: string | null;
    type: number;
    creatorId: string;
    createdAt: string;
    memberCount: number;
  }

  export interface ChatMessageItem {
    id: string;
    chatGroupId: string;
    senderId: string;
    senderName?: string;
    content: string;
    replyToMessageId: string | null;
    createdAt: string;
  }
}

async function getMyChatGroups(): Promise<ChatApi.ChatGroupItem[]> {
  const res = await requestClient.get<ChatApi.ChatGroupItem[] | { data?: ChatApi.ChatGroupItem[] }>(
    '/chat/groups',
  );
  if (Array.isArray(res)) return res;
  return (res as { data?: ChatApi.ChatGroupItem[] })?.data ?? [];
}

async function getChatMessages(chatGroupId: string, params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ChatApi.ChatMessageItem[];
    total: number;
  }>(`/chat/groups/${chatGroupId}/messages`, { params });
  return res;
}

async function createSingleChat(otherUserId: number) {
  return requestClient.post<{ id: string }>('/chat/groups/single', { otherUserId });
}

async function createGroupChat(data: { name: string; memberIds: number[] }) {
  return requestClient.post<{ id: string }>('/chat/groups/group', data);
}

export { createGroupChat, createSingleChat, getChatMessages, getMyChatGroups };
