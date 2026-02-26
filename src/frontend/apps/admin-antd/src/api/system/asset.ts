import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 资产状态：0 可用 1 已领用 2 已报废 */
export type AssetStatus = 0 | 1 | 2;

export namespace AssetApi {
  export interface AssetItem {
    id: string;
    name: string;
    category: string;
    code: string;
    status: AssetStatus;
    purchaseDate: string;
    value: number;
    remark?: string;
    creatorId: string;
    createdAt: string;
  }

  export interface AllocationItem {
    id: string;
    assetId: string;
    assetName: string;
    assetCode: string;
    userId: string;
    allocatedAt: string;
    returnedAt?: string;
    note?: string;
  }
}

async function getAssetList(params: Recordable<any>) {
  const res = await requestClient.get<{ items: AssetApi.AssetItem[]; total: number }>(
    '/assets',
    { params },
  );
  return res;
}

async function getAsset(id: string) {
  return requestClient.get<AssetApi.AssetItem>(`/assets/${id}`);
}

async function createAsset(data: {
  name: string;
  category: string;
  code: string;
  purchaseDate: string;
  value: number;
  remark?: string;
}) {
  return requestClient.post<{ id: string }>('/assets', data);
}

async function updateAsset(
  id: string,
  data: {
    name: string;
    category: string;
    code: string;
    purchaseDate: string;
    value: number;
    remark?: string;
  },
) {
  return requestClient.put(`/assets/${id}`, data);
}

async function allocateAsset(assetId: string, data: { userId: number; note?: string }) {
  return requestClient.post<{ id: string }>(`/assets/${assetId}/allocate`, data);
}

async function returnAsset(allocationId: string) {
  return requestClient.post(`/asset-allocations/${allocationId}/return`);
}

async function scrapAsset(id: string) {
  return requestClient.post(`/assets/${id}/scrap`);
}

async function getAssetAllocationList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: AssetApi.AllocationItem[];
    total: number;
  }>('/asset-allocations', { params });
  return res;
}

export {
  getAssetList,
  getAsset,
  createAsset,
  updateAsset,
  allocateAsset,
  returnAsset,
  scrapAsset,
  getAssetAllocationList,
};
