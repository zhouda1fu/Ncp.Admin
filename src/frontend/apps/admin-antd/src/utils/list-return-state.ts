import type {
  RouteLocationNormalizedLoaded,
  RouteRecordNameGeneric,
  Router,
} from 'vue-router';

import { nextTick } from 'vue';

import { useTabbarStore } from '@vben/stores';

export type ListPager = {
  currentPage: number;
  pageSize: number;
};

export type ListReturnFieldValue = string | number | boolean | undefined | null;

/** vben-form submitOnChange 防抖间隔（见 form-ui/vben-use-form.vue） */
const FORM_SUBMIT_ON_CHANGE_DEBOUNCE_MS = 300;
const FORM_SUBMIT_ON_CHANGE_BUFFER_MS = 150;

/** keepAlive 列表在离开页后仍会收到全局 route 更新，须用 path/name 判断是否仍在列表页 */
export function isRouteOnListPath(
  route: RouteLocationNormalizedLoaded,
  listPath: string,
  listRouteName?: RouteRecordNameGeneric | null,
): boolean {
  if (listRouteName != null && route.name != null) {
    return route.name === listRouteName;
  }
  const currentPath = route.path.split('?')[0] ?? '';
  const listPathOnly = listPath.split('?')[0] ?? '';
  return currentPath === listPathOnly;
}

export type ListReturnGridApi = {
  formApi?: {
    getValues?: () => Promise<Record<string, unknown>>;
    setValues?: (values: Record<string, unknown>) => Promise<void>;
    setLatestSubmissionValues?: (values: Record<string, unknown>) => void;
    setState?: (state: { submitOnChange?: boolean }) => void;
  };
  grid?: {
    reactData?: {
      tablePage?: { currentPage: number; pageSize: number; total?: number };
    };
    setCurrentPage?: (page: number) => Promise<void> | void;
    setPageSize?: (size: number) => Promise<void> | void;
    commitProxy?: (code: string, params?: Record<string, unknown>) => Promise<unknown>;
  };
  setGridOptions?: (options: {
    pagerConfig?: { currentPage: number; pageSize: number };
  }) => void;
  query?: (params?: Record<string, unknown>) => void | Promise<void>;
  reload?: (params?: Record<string, unknown>) => void | Promise<void>;
};

function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/** 路由 query 是否携带需恢复的列表状态（用于跳过 autoLoad、避免与 init 竞态） */
export function hasListRestoreQuery(
  query: Record<string, unknown>,
  searchKeys: readonly string[] = [],
  extraKeys: readonly string[] = [],
): boolean {
  const pager = readPagerFromRouteQuery(query);
  if (pager.currentPage > 1) return true;
  for (const key of searchKeys) {
    if (readStringQuery(query, key) !== undefined) return true;
  }
  for (const key of extraKeys) {
    if (readStringQuery(query, key) !== undefined) return true;
  }
  return false;
}

/** 等待 VxeGrid 挂载完成（子组件 onMounted / init 异步，父级 restore 须在其后） */
export async function waitForGridReady(
  gridApi: ListReturnGridApi,
  maxAttempts = 60,
): Promise<boolean> {
  for (let i = 0; i < maxAttempts; i += 1) {
    if (typeof gridApi.grid?.commitProxy === 'function') {
      await nextTick();
      await nextTick();
      return true;
    }
    await delay(16);
  }
  return false;
}

/** 将目标页码写入 VxeGrid 内部分页状态（commitProxy('query') 从 tablePage 读取 page） */
async function syncGridPagerBeforeQuery(
  gridApi: ListReturnGridApi,
  pager: ListPager,
): Promise<void> {
  gridApi.setGridOptions?.({
    pagerConfig: {
      currentPage: pager.currentPage,
      pageSize: pager.pageSize,
    },
  });
  const grid = gridApi.grid;
  if (grid?.setPageSize && grid?.setCurrentPage) {
    await grid.setPageSize(pager.pageSize);
    await grid.setCurrentPage(pager.currentPage);
  } else {
    const tablePage = grid?.reactData?.tablePage;
    if (tablePage) {
      tablePage.pageSize = pager.pageSize;
      tablePage.currentPage = pager.currentPage;
    }
  }
  await nextTick();
}

/** 从路由 query 读取分页（与列表页 `?page=&pageSize=` 配合） */
export function readPagerFromRouteQuery(
  query: Record<string, unknown>,
): ListPager {
  const rawPage = query.page;
  const rawPageSize = query.pageSize;
  const p = Number(Array.isArray(rawPage) ? rawPage[0] : rawPage);
  const ps = Number(Array.isArray(rawPageSize) ? rawPageSize[0] : rawPageSize);
  return {
    currentPage: Number.isFinite(p) && p >= 1 ? Math.floor(p) : 1,
    pageSize: Number.isFinite(ps) && ps >= 1 && ps <= 500 ? Math.floor(ps) : 20,
  };
}

/** 读取单个 query 字符串参数 */
export function readStringQuery(
  query: Record<string, unknown>,
  key: string,
): string | undefined {
  const raw = query[key];
  const v = Array.isArray(raw) ? raw[0] : raw;
  if (v == null || v === '') return undefined;
  return String(v);
}

/** 批量读取 query 字符串参数 */
export function collectRouteStringParams(
  query: Record<string, unknown>,
  keys: readonly string[],
): Record<string, string> {
  const result: Record<string, string> = {};
  for (const key of keys) {
    const v = readStringQuery(query, key);
    if (v !== undefined) result[key] = v;
  }
  return result;
}

/** 构建带 query 的列表 path */
export function buildListPath(
  basePath: string,
  params: Record<string, ListReturnFieldValue>,
): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue;
    search.set(key, String(value));
  }
  const qs = search.toString();
  return qs ? `${basePath}?${qs}` : basePath;
}

export function encodeListReturn(path: string): string {
  return encodeURIComponent(path);
}

export function isSafeListReturnPath(
  decoded: string,
  allowedPaths: readonly string[],
): boolean {
  const s = decoded.trim();
  if (s.startsWith('//') || s.includes('://')) return false;
  const pathOnly = s.split('?')[0] ?? '';
  return allowedPaths.some((p) => pathOnly === p);
}

export function decodeListReturn(
  raw: unknown,
  allowedPaths: readonly string[],
): string | null {
  const ret = Array.isArray(raw) ? raw[0] : raw;
  if (typeof ret !== 'string' || ret.length === 0) return null;
  try {
    const decoded = decodeURIComponent(ret);
    return isSafeListReturnPath(decoded, allowedPaths) ? decoded : null;
  } catch {
    return null;
  }
}

/** 保存/编辑后返回列表时携带，用于触发 keepAlive 列表刷新 */
export const LIST_RETURN_REFRESH_QUERY_KEY = '_refresh';

/** 详情页返回列表：优先 `return` 参数，其次 history.back，最后 fallback */
export async function navigateBackToList(
  router: Router,
  route: RouteLocationNormalizedLoaded,
  allowedPaths: readonly string[],
  fallbackPath: string,
  options?: { reload?: boolean },
): Promise<void> {
  const refreshQuery = options?.reload
    ? { [LIST_RETURN_REFRESH_QUERY_KEY]: String(Date.now()) }
    : {};
  const decoded = decodeListReturn(route.query.return, allowedPaths);
  if (decoded) {
    const { path, query } = parseListReturnRouteLocation(decoded);
    await router.push({ path, query: { ...query, ...refreshQuery } });
    return;
  }
  if (!options?.reload && typeof window !== 'undefined' && window.history.length > 1) {
    await router.back();
    return;
  }
  await router.push({ path: fallbackPath, query: refreshQuery });
}

/** 构建列表 return 路径（分页 + 任意 query 字段） */
export function buildListReturnPath(
  listPath: string,
  pager: ListPager,
  extra: Record<string, ListReturnFieldValue> = {},
): string {
  return buildListPath(listPath, {
    page: pager.currentPage,
    pageSize: pager.pageSize,
    ...extra,
  });
}

export function buildListReturnEncoded(
  listPath: string,
  pager: ListPager,
  extra: Record<string, ListReturnFieldValue> = {},
): string {
  return encodeListReturn(buildListReturnPath(listPath, pager, extra));
}

/** 构建 return 并同步多标签页中对应列表 tab 的 fullPath（关闭详情 tab 时可恢复分页/搜索） */
export function buildListReturnPayload(
  listPath: string,
  pager: ListPager,
  extra: Record<string, ListReturnFieldValue> = {},
): { encoded: string; fullPath: string } {
  const fullPath = buildListReturnPath(listPath, pager, extra);
  syncListTabFullPath(fullPath);
  return { encoded: encodeListReturn(fullPath), fullPath };
}

/** 内部应用 path 基本校验（不含白名单，仅用于 tab 关闭时解码本系统写入的 return） */
export function isSafeInternalReturnPath(decoded: string): boolean {
  const s = decoded.trim();
  if (!s.startsWith('/') || s.startsWith('//') || s.includes('://')) return false;
  return true;
}

export function decodeListReturnLoose(raw: unknown): string | null {
  const ret = Array.isArray(raw) ? raw[0] : raw;
  if (typeof ret !== 'string' || ret.length === 0) return null;
  try {
    const decoded = decodeURIComponent(ret);
    return isSafeInternalReturnPath(decoded) ? decoded : null;
  } catch {
    return null;
  }
}

function parseFullPathQuery(fullPath: string): Record<string, string> {
  const qs = fullPath.includes('?') ? (fullPath.split('?')[1] ?? '') : '';
  if (!qs) return {};
  const result: Record<string, string> = {};
  const params = new URLSearchParams(qs);
  params.forEach((value, key) => {
    result[key] = value;
  });
  return result;
}

/** 将 buildListReturnPath 解码结果转为 router.push 可用的 location */
export function parseListReturnRouteLocation(decoded: string): {
  path: string;
  query: Record<string, string>;
} {
  const trimmed = decoded.trim();
  const path = trimmed.split('?')[0] ?? trimmed;
  return {
    path,
    query: parseFullPathQuery(trimmed),
  };
}

/** 将列表当前分页/搜索状态写入 tabbar 中对应列表 tab，供关闭详情 tab 时恢复 */
export function syncListTabFullPath(fullPath: string): void {
  const pathOnly = fullPath.split('?')[0] ?? '';
  if (!pathOnly) return;
  try {
    const tabbarStore = useTabbarStore();
    const query = parseFullPathQuery(fullPath);
    const index = tabbarStore.tabs.findIndex((tab) => {
      const tabPath = (tab.fullPath ?? tab.path ?? '').split('?')[0] ?? '';
      return tabPath === pathOnly;
    });
    if (index === -1) return;
    const currentTab = tabbarStore.tabs[index];
    if (!currentTab) return;
    tabbarStore.tabs.splice(index, 1, {
      ...currentTab,
      fullPath,
      query: { ...currentTab.query, ...query },
      key: pathOnly,
    });
  } catch {
    // tabbar 未初始化时忽略
  }
}

/** 将 route query 中的分页同步到 VxeGrid 并触发查询 */
export async function reloadGridFromRouteQuery(
  gridApi: ListReturnGridApi,
  query: Record<string, unknown>,
  formValues: Record<string, unknown> = {},
): Promise<ListPager> {
  const pager = readPagerFromRouteQuery(query);
  await syncGridPagerBeforeQuery(gridApi, pager);

  if (gridApi.query) {
    await gridApi.query(formValues);
  } else if (gridApi.grid?.commitProxy) {
    await gridApi.grid.commitProxy('query', formValues);
  }
  return pager;
}

/** 恢复搜索表单并刷新列表（须同步 latestSubmissionValues，且避免 submitOnChange 触发二次 reload） */
export async function applyGridSearchFromRouteAndReload(
  gridApi: ListReturnGridApi,
  searchValues: Record<string, unknown>,
  query: Record<string, unknown>,
): Promise<ListPager> {
  const formApi = gridApi.formApi;
  formApi?.setState?.({ submitOnChange: false });
  try {
    await waitForGridReady(gridApi);

    const currentValues = (await formApi?.getValues?.()) ?? {};
    const merged: Record<string, unknown> = { ...currentValues, ...searchValues };

    // 必须先写入 latestSubmissionValues，再 setValues（否则 VxeGrid 请求仍用旧条件）
    formApi?.setLatestSubmissionValues?.(merged);
    if (Object.keys(searchValues).length > 0) {
      await formApi?.setValues?.(searchValues);
    }
    await nextTick();
    return reloadGridFromRouteQuery(gridApi, query, merged);
  } finally {
    // setValues 会触发 300ms 防抖的 submitOnChange → reload（重置到第 1 页），须等防抖结束后再恢复
    await delay(FORM_SUBMIT_ON_CHANGE_DEBOUNCE_MS + FORM_SUBMIT_ON_CHANGE_BUFFER_MS);
    formApi?.setState?.({ submitOnChange: true });
  }
}

/** 从表单值中提取需写入 return URL 的搜索字段（跳过空值） */
export function pickSearchFieldsForReturn(
  formValues: Record<string, unknown>,
  keys: readonly string[],
): Record<string, string> {
  const result: Record<string, string> = {};
  for (const key of keys) {
    const raw = formValues[key];
    if (raw === undefined || raw === null || raw === '') continue;
    result[key] = String(raw);
  }
  return result;
}

/** 布尔搜索项：URL 存 true/false 字符串 */
export function parseBooleanQuery(value: string | undefined): boolean | undefined {
  if (value === 'true') return true;
  if (value === 'false') return false;
  return undefined;
}

/** 数字搜索项 */
export function parseNumberQuery(value: string | undefined): number | undefined {
  if (value === undefined) return undefined;
  const n = Number(value);
  return Number.isFinite(n) ? n : undefined;
}
