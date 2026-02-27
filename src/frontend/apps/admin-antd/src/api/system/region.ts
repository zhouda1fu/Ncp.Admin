import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace RegionApi {
  export interface RegionItem {
    id: number | string;
    name: string;
    parentId: number | string;
    level: number;
    sortOrder: number;
  }
}

/** 标准化区域项，兼容 API 返回的 PascalCase 或 number/string 类型 */
function normalizeRegionItem(raw: Recordable<any>): RegionApi.RegionItem {
  return {
    id: raw.id ?? raw.Id ?? 0,
    name: String(raw.name ?? raw.Name ?? ''),
    parentId: raw.parentId ?? raw.ParentId ?? 0,
    level: Number(raw.level ?? raw.Level ?? 0),
    sortOrder: Number(raw.sortOrder ?? raw.SortOrder ?? 0),
  };
}

async function getRegionList() {
  const res = await requestClient.get<RegionApi.RegionItem[] | { data?: RegionApi.RegionItem[] }>('/regions');
  const list = Array.isArray(res)
    ? res
    : (res as { data?: unknown[] })?.data ?? [];
  return (list as Recordable<any>[]).map(normalizeRegionItem);
}

export { getRegionList };
