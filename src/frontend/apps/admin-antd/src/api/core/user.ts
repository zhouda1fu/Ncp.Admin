import type { UserInfo } from '@vben/types';

import { requestClient } from '#/api/request';

/**
 * 获取用户信息
 */
export async function getUserInfoApi(userId?: string) {
  if (userId) {
    return requestClient.get<UserInfo>(`/user/profile/${userId}`);
  }
  return requestClient.get<UserInfo>('/user/profile');
}

export interface ChangeCurrentPasswordParams {
  newPassword: string;
  oldPassword: string;
}

export function changeCurrentPasswordApi(data: ChangeCurrentPasswordParams) {
  return requestClient.put<boolean>('/user/change-password', data);
}

export interface UpdateCurrentUserAvatarParams {
  avatarUrl: string;
}

export function updateCurrentUserAvatarApi(
  data: UpdateCurrentUserAvatarParams,
) {
  return requestClient.put<boolean>('/user/avatar', data);
}

export async function uploadCurrentUserAvatarApi(file: File) {
  const res = await requestClient.upload<unknown>('/user/avatar/upload', { file });
  const avatarUrl = resolveAvatarUrl(res);
  if (!avatarUrl) {
    throw new Error('上传成功但未返回头像地址');
  }
  return { avatarUrl };
}

function resolveAvatarUrl(res: unknown): string {
  if (typeof res === 'string' && res.trim()) return res.trim();
  if (!res || typeof res !== 'object') return '';
  const r = res as Record<string, unknown>;
  const direct = r.avatarUrl ?? r.AvatarUrl;
  if (typeof direct === 'string' && direct.trim()) return direct.trim();
  const data = r.data;
  if (data && typeof data === 'object') {
    const nested =
      (data as Record<string, unknown>).avatarUrl ??
      (data as Record<string, unknown>).AvatarUrl;
    if (typeof nested === 'string' && nested.trim()) return nested.trim();
  }
  return '';
}
