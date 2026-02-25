import * as signalR from '@microsoft/signalr';
import { onUnmounted, ref, watch } from 'vue';

import { message as antMessage } from 'ant-design-vue';
import { useAppConfig } from '@vben/hooks';
import { useAccessStore } from '@vben/stores';

export interface ChatMessagePushDto {
  id: string;
  chatGroupId: string;
  senderId: number;
  senderName?: string;
  content: string;
  replyToMessageId: string | null;
  createdAt: string;
}

/**
 * SignalR 聊天 Hub
 * 连接后可 JoinGroup、SendMessage；监听 ReceiveMessageDto（新消息）、ReceiveError（错误）
 */
export function useChatHub(onMessage?: (msg: ChatMessagePushDto) => void) {
  const accessStore = useAccessStore();
  const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);
  const connected = ref(false);

  let connection: signalR.HubConnection | null = null;

  async function connect() {
    const token = accessStore.accessToken;
    if (!token) return;

    const baseUrl = apiURL.replace(/\/api\/admin\/?$/, '') || apiURL.split('/api')[0];
    const hubUrl = `${baseUrl}/chat`;

    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessStore.accessToken ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveMessageDto', (msg: ChatMessagePushDto) => {
      onMessage?.(msg);
    });

    connection.on('ReceiveError', (err: string) => {
      antMessage.error(err || '发送失败');
    });

    connection.onclose(() => {
      connected.value = false;
    });

    try {
      await connection.start();
      connected.value = true;
    } catch (err) {
      console.warn('[ChatHub] 连接失败:', err);
    }
  }

  async function disconnect() {
    if (connection) {
      try {
        await connection.stop();
      } catch {
        // ignore
      }
      connection = null;
      connected.value = false;
    }
  }

  async function joinGroup(chatGroupId: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('JoinGroup', chatGroupId);
    }
  }

  async function leaveGroup(chatGroupId: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('LeaveGroup', chatGroupId);
    }
  }

  async function sendMessage(chatGroupId: string, content: string, replyToMessageId?: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('SendMessage', chatGroupId, content, replyToMessageId ?? null);
    }
  }

  watch(
    () => accessStore.accessToken,
    async (token) => {
      await disconnect();
      if (token && onMessage) {
        await connect();
      }
    },
    { immediate: false },
  );

  onUnmounted(() => {
    disconnect();
  });

  return { connect, disconnect, joinGroup, leaveGroup, sendMessage, connected };
}
