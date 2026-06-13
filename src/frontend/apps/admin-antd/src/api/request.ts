/**
 * 该文件可自行根据业务逻辑进行调整
 */
import type { RequestClientOptions } from '@vben/request';

import { useAppConfig } from '@vben/hooks';
import { preferences } from '@vben/preferences';
import {
  authenticateResponseInterceptor,
  defaultResponseInterceptor,
  errorMessageResponseInterceptor,
  RequestClient,
} from '@vben/request';
import { useAccessStore } from '@vben/stores';

import { message } from 'ant-design-vue';

import { useAuthStore } from '#/store';

import { refreshTokenApi } from './core';

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);

/** 与 VITE_GLOB_API_URL（…/api/admin）对齐的根地址，用于 /api/public/* 等匿名接口 */
const publicApiBaseURL = apiURL.replace(/\/admin\/?$/i, '').replace(/\/$/, '');

function createRequestClient(baseURL: string, options?: RequestClientOptions) {
  const client = new RequestClient({
    ...options,
    baseURL,
  });
  let isReAuthenticating = false;

  /**
   * 重新认证逻辑
   */
  async function doReAuthenticate() {
    console.warn('Access token or refresh token is invalid or expired. ');
    if (isReAuthenticating) {
      return;
    }
    isReAuthenticating = true;
    const accessStore = useAccessStore();
    const authStore = useAuthStore();
    try {
      accessStore.setAccessToken(null);
      if (
        preferences.app.loginExpiredMode === 'modal' &&
        accessStore.isAccessChecked
      ) {
        accessStore.setLoginExpired(true);
      } else {
        await authStore.logout();
      }
    } finally {
      isReAuthenticating = false;
    }
  }

  /**
   * 刷新token逻辑
   */
  async function doRefreshToken() {
    const accessStore = useAccessStore();
    const resp = await refreshTokenApi();
    const newToken = resp.data;
    accessStore.setAccessToken(newToken);
    return newToken;
  }

  function formatToken(token: null | string) {
    return token ? `Bearer ${token}` : null;
  }

  // 请求头处理
  client.addRequestInterceptor({
    fulfilled: async (config) => {
      const accessStore = useAccessStore();

      config.headers.Authorization = formatToken(accessStore.accessToken);
      config.headers['Accept-Language'] = preferences.app.locale;
      return config;
    },
  });

  // 处理返回的响应数据格式
  client.addResponseInterceptor(
    defaultResponseInterceptor({
      codeField: 'code',
      dataField: 'data',
      successCode: 0,
    }),
  );

  client.addResponseInterceptor({
    rejected: async (error) => {
      if (error?.response?.headers?.['x-auth-reason'] !== 'session-replaced') {
        throw error;
      }
      if (!isReAuthenticating) {
        isReAuthenticating = true;
        try {
          message.warning('账号已在其他设备登录，当前会话已退出');
          await useAuthStore().forceLogout();
        } finally {
          isReAuthenticating = false;
        }
      }
      throw error;
    },
  });

  // token过期的处理
  client.addResponseInterceptor(
    authenticateResponseInterceptor({
      client,
      doReAuthenticate,
      doRefreshToken,
      enableRefreshToken: preferences.app.enableRefreshToken,
      formatToken,
    }),
  );

  // 通用的错误处理,如果没有进入上面的错误处理逻辑，就会进入这里
  client.addResponseInterceptor(
    errorMessageResponseInterceptor((msg: string, error) => {
      if ((error?.config as { silentError?: boolean } | undefined)?.silentError) {
        return;
      }
      // 这里可以根据业务进行定制,你可以拿到 error 内的信息进行定制化处理，根据不同的 code 做不同的提示，而不是直接使用 message.error 提示 msg
      // 当前mock接口返回的错误字段是 error 或者 message
      const responseData = error?.response?.data ?? {};
      const errorMessage = responseData?.error ?? responseData?.message ?? '';
      // 如果没有错误信息，则会根据状态码进行提示
      message.error(errorMessage || msg);
    }),
  );

  return client;
}

/**
 * 匿名公开 API（如董事长信箱）：与后台管理不同前缀，且不附加 Authorization、不做 token 刷新。
 */
function createPublicRequestClient() {
  const client = new RequestClient({
    baseURL: publicApiBaseURL,
    responseReturn: 'data',
  });
  client.addRequestInterceptor({
    fulfilled: async (config) => {
      config.headers['Accept-Language'] = preferences.app.locale;
      delete config.headers.Authorization;
      return config;
    },
  });
  client.addResponseInterceptor(
    defaultResponseInterceptor({
      codeField: 'code',
      dataField: 'data',
      successCode: 0,
    }),
  );
  return client;
}

export const requestClient = createRequestClient(apiURL, {
  responseReturn: 'data',
});

export const publicRequestClient = createPublicRequestClient();

export const baseRequestClient = new RequestClient({ baseURL: apiURL });
