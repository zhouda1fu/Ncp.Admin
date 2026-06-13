import { fetchFileBlob } from '#/api/system/file';

let activeBlobUrl: string | null = null;
let activeStoragePath: string | null = null;

/** 释放当前缓存的用户头像 blob URL */
export function revokeUserAvatarBlobUrl() {
  if (activeBlobUrl) {
    URL.revokeObjectURL(activeBlobUrl);
    activeBlobUrl = null;
    activeStoragePath = null;
  }
}

function isDirectDisplayUrl(url: string) {
  return (
    url.startsWith('blob:')
    || url.startsWith('data:')
    || url.startsWith('http://')
    || url.startsWith('https://')
  );
}

/**
 * 将后端存储 path 转为可展示的 blob URL（带 Token 下载，避免 img 直接请求 401）
 */
export async function resolveUserAvatarBlobUrl(
  avatarUrl?: string,
): Promise<string> {
  const path = avatarUrl?.trim() ?? '';
  if (!path) {
    return '';
  }
  if (isDirectDisplayUrl(path)) {
    return path;
  }
  if (activeBlobUrl && activeStoragePath === path) {
    return activeBlobUrl;
  }

  revokeUserAvatarBlobUrl();
  try {
    const blob = await fetchFileBlob(path, { silentError: true });
    activeBlobUrl = URL.createObjectURL(blob);
    activeStoragePath = path;
    return activeBlobUrl;
  } catch {
    return '';
  }
}
