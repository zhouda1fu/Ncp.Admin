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

/** 下载路径（相对 baseURL），与 apiURL 拼接后得到完整下载 URL，用于预览/链接 */
function getFileDownloadPath(path: string): string {
  return `/files/download?path=${encodeURIComponent(path)}`;
}

export { uploadFile, getFileDownloadPath };
