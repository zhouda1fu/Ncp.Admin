import * as signalR from '@microsoft/signalr';
import { onUnmounted, ref, watch } from 'vue';

import { useAppConfig } from '@vben/hooks';
import { useAccessStore } from '@vben/stores';

import { resolveSignalRHubUrl } from '#/utils/signalr-hub-url';

/**
 * SignalR 通知 Hub 连接
 * 连接后监听 ReceiveNotification，收到新通知时触发 onNotification
 */
export function useNotificationHub(
  onNotification: () => void | Promise<void>,
  onSessionReplaced?: () => void | Promise<void>,
) {
  const accessStore = useAccessStore();
  const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);
  const connected = ref(false);

  let connection: signalR.HubConnection | null = null;
  let notifyTimer: ReturnType<typeof setTimeout> | null = null;

  function scheduleNotification() {
    if (notifyTimer) {
      clearTimeout(notifyTimer);
    }
    notifyTimer = setTimeout(() => {
      notifyTimer = null;
      void Promise.resolve(onNotification());
    }, 200);
  }

  async function connect() {
    const token = accessStore.accessToken;
    if (!token || !accessStore.isAccessChecked) {
      return;
    }

    const hubUrl = resolveSignalRHubUrl('notification', apiURL);

    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessStore.accessToken ?? '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    connection.on('ReceiveNotification', () => {
      scheduleNotification();
    });

    connection.on('SessionReplaced', () => {
      void Promise.resolve(onSessionReplaced?.());
    });

    connection.onreconnected(() => {
      connected.value = true;
      scheduleNotification();
    });

    connection.onclose(() => {
      connected.value = false;
    });

    try {
      await connection.start();
      connected.value = true;
    } catch (err) {
      connected.value = false;
      console.warn('[NotificationHub] 连接失败:', err);
    }
  }

  async function disconnect() {
    if (notifyTimer) {
      clearTimeout(notifyTimer);
      notifyTimer = null;
    }
    if (connection) {
      try {
        await connection.stop();
      } catch {
        // ignore
      }
      connection = null;
    }
    connected.value = false;
  }

  watch(
    () => [accessStore.accessToken, accessStore.isAccessChecked] as const,
    async ([token, accessChecked]) => {
      await disconnect();
      if (token && accessChecked) {
        await connect();
      }
    },
    { immediate: true },
  );

  onUnmounted(() => {
    void disconnect();
  });

  return { connect, disconnect, connected };
}
