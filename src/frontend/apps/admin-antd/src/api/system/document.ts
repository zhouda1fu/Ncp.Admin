import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace DocumentApi {
  export interface DocumentVersionItem {
    id: string;
    versionNumber: number;
    fileName: string;
    fileSize: number;
    createdAt: string;
  }

  export interface DocumentItem {
    id: string;
    title: string;
    creatorId: string;
    createdAt: string;
    versionCount: number;
    currentVersion?: DocumentVersionItem;
  }
}

async function getDocumentList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: DocumentApi.DocumentItem[];
    total: number;
  }>('/documents', { params });
  return res;
}

async function getDocumentById(id: string) {
  return requestClient.get<DocumentApi.DocumentItem>(`/documents/${id}`);
}

/** 上传文档（multipart: title + file） */
async function uploadDocument(formData: FormData) {
  return requestClient.post<{ id: string }>('/documents/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

/** 添加新版本（multipart: file） */
async function addDocumentVersion(documentId: string, formData: FormData) {
  return requestClient.post<boolean>(`/documents/${documentId}/versions`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

async function updateDocumentTitle(id: string, title: string) {
  return requestClient.put(`/documents/${id}`, { title });
}

/** 创建共享链接 */
async function createShareLink(data: { documentId: string; expiresAt?: string }) {
  return requestClient.post<{ id: string; token: string }>('/share-links', data);
}

/** 文档版本下载路径（需带认证请求，列表页用此路径发起下载） */
function getVersionDownloadPath(documentId: string, versionId: string) {
  return `/documents/${documentId}/versions/${versionId}/download`;
}

export {
  addDocumentVersion,
  createShareLink,
  getDocumentById,
  getDocumentList,
  getVersionDownloadPath,
  updateDocumentTitle,
  uploadDocument,
};
