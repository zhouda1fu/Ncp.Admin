import { requestClient } from '#/api/request';

/** 通用文件上传，返回存储 path（用于如 Customer.BusinessLicense） */
async function uploadFile(file: File): Promise<{ path: string }> {
  const formData = new FormData();
  formData.append('file', file);
  const res = await requestClient.post<{ path: string }>('/files/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return res;
}

/** 下载路径（相对 baseURL），与 apiURL 拼接后得到完整下载 URL（仅用于同源且带 Cookie 的场景；带 Token 时请用 fetchFileBlob + blob URL） */
function getFileDownloadPath(path: string): string {
  return `/files/download?path=${encodeURIComponent(path)}`;
}

/**
 * 带认证拉取文件为 Blob，用于生成 blob URL 做预览（避免直接使用下载 URL 时浏览器请求无 Token 导致 401）
 * 使用 responseReturn: 'body' 避免 defaultResponseInterceptor 按 JSON 解析 blob 导致抛错并显示「内部服务器错误」。
 */
async function fetchFileBlob(path: string): Promise<Blob> {
  const res = await requestClient.get<Blob>(getFileDownloadPath(path), {
    responseType: 'blob',
    responseReturn: 'body',
  });
  return res as Blob;
}

export { uploadFile, getFileDownloadPath, fetchFileBlob };
