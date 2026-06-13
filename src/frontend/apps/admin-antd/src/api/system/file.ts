import { requestClient } from '#/api/request';

/** 通用文件上传，返回存储 path（用于如 Customer.BusinessLicense） */
async function uploadFile(file: File): Promise<{ path: string }> {
  // 须用 upload()：默认 Content-Type 为 application/json，直接 post FormData 会 415
  const res = await requestClient.upload<unknown>('/files/upload', { file });
  const path = resolveUploadedFilePath(res);
  if (!path) {
    throw new Error('上传成功但未返回文件路径');
  }
  return { path };
}

/** 解析上传接口返回的存储路径（兼容 path / Path / data 包装） */
function resolveUploadedFilePath(res: unknown): string {
  if (typeof res === 'string' && res.trim()) return res.trim();
  if (!res || typeof res !== 'object') return '';
  const r = res as Record<string, unknown>;
  const direct = r.path ?? r.Path;
  if (typeof direct === 'string' && direct.trim()) return direct.trim();
  const data = r.data;
  if (data && typeof data === 'object') {
    const nested = (data as Record<string, unknown>).path ?? (data as Record<string, unknown>).Path;
    if (typeof nested === 'string' && nested.trim()) return nested.trim();
  }
  return '';
}

/** 文件缺失或未加载时的通用图片占位（SVG，无需额外请求） */
const IMAGE_PLACEHOLDER_DATA_URL =
  `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(
    '<svg xmlns="http://www.w3.org/2000/svg" width="80" height="80" viewBox="0 0 80 80"><rect fill="#e5e7eb" width="80" height="80" rx="6"/><g fill="#9ca3af"><path d="M16 56l12-16 8 10 12-16 16 22H16z"/><circle cx="28" cy="28" r="5"/></g></svg>',
  )}`;

export type FetchFileBlobOptions = {
  /** 为 true 时不弹出全局错误提示（用于列表缩略图等可缺失的资源） */
  silentError?: boolean;
};

/** 下载路径（相对 apiURL /api/admin），供 requestClient 带 Token 请求 */
function getFileDownloadPath(path: string): string {
  return `/files/download?path=${encodeURIComponent(path)}`;
}

/** 从后端返回的下载引用或原始存储 path 中解析存储 path */
function parseStoragePathFromFileRef(ref: string): string {
  const trimmed = ref?.trim() ?? '';
  if (!trimmed) return '';
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    try {
      const url = new URL(trimmed);
      const path = url.searchParams.get('path');
      return path ? decodeURIComponent(path) : '';
    } catch {
      return '';
    }
  }
  if (trimmed.includes('path=')) {
    const query = trimmed.includes('?') ? trimmed.slice(trimmed.indexOf('?')) : '';
    if (query) {
      const path = new URLSearchParams(query).get('path');
      if (path) return decodeURIComponent(path);
    }
  }
  if (trimmed.startsWith('/api/')) return '';
  return trimmed;
}

/** 拼出浏览器可打开的绝对下载地址（不含 Token；图片预览请用 fetchFileBlob） */
function resolveAdminFileUrl(ref: string, apiURL: string): string {
  const trimmed = ref?.trim() ?? '';
  if (!trimmed) return '';
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) return trimmed;
  const origin = apiURL.replace(/\/api\/admin\/?$/i, '').replace(/\/$/, '');
  if (trimmed.startsWith('/api/admin/')) return `${origin}${trimmed}`;
  const adminBase = apiURL.replace(/\/$/, '');
  if (trimmed.startsWith('/files/')) return `${adminBase}${trimmed}`;
  const storagePath = parseStoragePathFromFileRef(trimmed) || trimmed;
  return `${adminBase}/files/download?path=${encodeURIComponent(storagePath)}`;
}

/** 批量拉取图片为 blob URL（用于定制流程硬件图预览） */
async function fetchImageBlobUrls(refs: string[]): Promise<string[]> {
  const urls: string[] = [];
  for (const ref of refs) {
    const storagePath = parseStoragePathFromFileRef(ref);
    if (!storagePath) continue;
    try {
      const blob = await fetchFileBlob(storagePath, { silentError: true });
      urls.push(URL.createObjectURL(blob));
    } catch {
      /* 单张失败不影响其余 */
    }
  }
  return urls;
}

function revokeBlobUrls(urls: string[]) {
  for (const url of urls) {
    if (url.startsWith('blob:')) URL.revokeObjectURL(url);
  }
}

/**
 * 带认证拉取文件为 Blob，用于生成 blob URL 做预览（避免直接使用下载 URL 时浏览器请求无 Token 导致 401）
 * 使用 responseReturn: 'body' 避免 defaultResponseInterceptor 按 JSON 解析 blob 导致抛错并显示「内部服务器错误」。
 */
async function fetchFileBlob(path: string, options?: FetchFileBlobOptions): Promise<Blob> {
  const res = await requestClient.get<Blob>(getFileDownloadPath(path), {
    responseType: 'blob',
    responseReturn: 'body',
    silentError: options?.silentError ?? false,
  } as Parameters<typeof requestClient.get>[1] & { silentError?: boolean });
  return res as Blob;
}

/** 在线预览（.doc 由服务端转为 docx） */
function getFilePreviewPath(path: string): string {
  return `/files/preview?path=${encodeURIComponent(path)}`;
}

async function fetchFilePreviewBlob(path: string): Promise<Blob> {
  const res = await requestClient.get<Blob>(getFilePreviewPath(path), {
    responseType: 'blob',
    responseReturn: 'body',
  });
  return res as Blob;
}

export {
  uploadFile,
  resolveUploadedFilePath,
  getFileDownloadPath,
  parseStoragePathFromFileRef,
  resolveAdminFileUrl,
  fetchImageBlobUrls,
  revokeBlobUrls,
  fetchFileBlob,
  getFilePreviewPath,
  fetchFilePreviewBlob,
  IMAGE_PLACEHOLDER_DATA_URL,
};
