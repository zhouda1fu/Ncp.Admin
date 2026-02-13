import * as signalR from '@microsoft/signalr';
import { onUnmounted, watch } from 'vue';

import { useAppConfig } from '@vben/hooks';
import { useAccessStore } from '@vben/stores';

/**
 * SignalR 通知 Hub 连接
 * 连接后监听 ReceiveNotification，收到新通知时触发 onNotification
 */
export function useNotificationHub(onNotification: () => void | Promise<void>) {
  const accessStore = useAccessStore();
  const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);

  let connection: signalR.HubConnection | null = null;

  async function connect() {
    const token = accessStore.accessToken;
    if (!token) return;

    const baseUrl = apiURL.replace(/\/api\/admin\/?$/, '') || apiURL.split('/api')[0];
    const hubUrl = `${baseUrl}/notification`;

    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessStore.accessToken ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', () => {
      onNotification();
    });

    try {
      await connection.start();
    } catch (err) {
      console.warn('[NotificationHub] 连接失败:', err);
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
    }
  }

  watch(
    () => accessStore.accessToken,
    async (token) => {
      await disconnect();
      if (token) {
        await connect();
      }
    },
    { immediate: true },
  );

  onUnmounted(() => {
    disconnect();
  });

  return { connect, disconnect };
}
