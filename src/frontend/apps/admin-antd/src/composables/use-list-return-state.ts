import { onActivated, ref, watch } from 'vue';
import type { RouteLocationNormalizedLoaded, RouteRecordNameGeneric } from 'vue-router';

import {
  buildListReturnPayload,
  collectRouteStringParams,
  applyGridSearchFromRouteAndReload,
  hasListRestoreQuery,
  isRouteOnListPath,
  LIST_RETURN_REFRESH_QUERY_KEY,
  pickSearchFieldsForReturn,
  readPagerFromRouteQuery,
  type ListPager,
  type ListReturnFieldValue,
  type ListReturnGridApi,
} from '#/utils/list-return-state';

export type { ListReturnGridApi } from '#/utils/list-return-state';

export interface UseListReturnStateOptions {
  route: RouteLocationNormalizedLoaded;
  /** 列表页 path，如 `/customer/list` */
  listPath: string;
  /** 动态列表 path（优先于 listPath，用于带 Tab 段的路由） */
  getListPath?: () => string;
  /** keepAlive 下列表路由 name（与 path 二选一，动态 path 列表建议传 name） */
  listRouteName?: RouteRecordNameGeneric | null;
  /** 搜索表单 fieldName，写入 return URL */
  searchKeys?: readonly string[];
  /** 高级筛选、Tab 等额外 query 字段（参与 hasListRestoreQuery / autoLoad 判断） */
  extraRestoreQueryKeys?: readonly string[];
  getExtraReturnFields?: () => Record<string, ListReturnFieldValue>;
  /** 自定义从 route.query 恢复搜索表单（如布尔/数字字段） */
  parseRouteToSearchValues?: (query: Record<string, unknown>) => Record<string, unknown>;
  /** 恢复列表前同步高级筛选、表头筛选等非表单 state */
  onBeforeRestoreFromRoute?: () => void;
}

/**
 * 分页列表：跳转详情前保存 return，返回列表时恢复页码与搜索条件。
 */
export function useListReturnState(options: UseListReturnStateOptions) {
  const listPagerInit = readPagerFromRouteQuery(options.route.query);
  const lastListPage = ref<ListPager>(listPagerInit);
  let routeRestoreBound = false;
  const searchKeys = options.searchKeys ?? [];
  const extraRestoreQueryKeys = options.extraRestoreQueryKeys ?? [];
  /** 路由带分页/搜索 query 时为 true，proxyConfig 应设 autoLoad: !shouldDeferGridAutoLoad */
  const shouldDeferGridAutoLoad = hasListRestoreQuery(
    options.route.query,
    searchKeys,
    extraRestoreQueryKeys,
  );
  let restoringListRoute = false;
  let lastRestoredRouteKey = '';
  /** keepAlive 首次激活与 onMounted 同轮，避免与 autoLoad 重复请求 */
  let listActivatedOnce = false;

  function resolveListPath() {
    return options.getListPath?.() ?? options.listPath;
  }

  function isActiveListRoute(): boolean {
    return isRouteOnListPath(
      options.route,
      resolveListPath(),
      options.listRouteName,
    );
  }

  function buildDefaultRouteRestoreKey() {
    return JSON.stringify({
      page: options.route.query.page,
      pageSize: options.route.query.pageSize,
      search: searchKeys.map((k) => options.route.query[k]),
      extra: extraRestoreQueryKeys.map((k) => options.route.query[k]),
    });
  }

  function clearRestoreKey() {
    lastRestoredRouteKey = '';
  }

  function trackPage(page: { currentPage: number; pageSize: number }) {
    lastListPage.value = {
      currentPage: page.currentPage,
      pageSize: page.pageSize,
    };
  }

  async function buildReturnQuery(
    gridApi: ListReturnGridApi,
    extraQuery?: Record<string, string>,
  ): Promise<Record<string, string>> {
    // 无搜索表单时勿调用 formApi.getValues()，否则会等待未挂载的 Form 而永久挂起
    const formValues =
      (options.searchKeys?.length ?? 0) > 0
        ? ((await gridApi.formApi?.getValues?.()) ?? {})
        : {};
    const searchFields = options.searchKeys
      ? pickSearchFieldsForReturn(formValues, options.searchKeys)
      : {};
    const extra = options.getExtraReturnFields?.() ?? {};
    const { encoded } = buildListReturnPayload(resolveListPath(), lastListPage.value, {
      ...searchFields,
      ...extra,
    });
    return {
      return: encoded,
      ...extraQuery,
    };
  }

  async function restoreFromRoute(gridApi: ListReturnGridApi) {
    if (!isActiveListRoute()) return;
    if (
      !hasListRestoreQuery(
        options.route.query,
        options.searchKeys ?? [],
        options.extraRestoreQueryKeys ?? [],
      )
    ) {
      return;
    }
    const routeKey = buildDefaultRouteRestoreKey();
    if (restoringListRoute || routeKey === lastRestoredRouteKey) return;
    restoringListRoute = true;
    try {
      const pager = readPagerFromRouteQuery(options.route.query);
      lastListPage.value = pager;
      options.onBeforeRestoreFromRoute?.();

      const searchValues = options.parseRouteToSearchValues
        ? options.parseRouteToSearchValues(options.route.query)
        : options.searchKeys
          ? collectRouteStringParams(options.route.query, options.searchKeys)
          : {};
      await applyGridSearchFromRouteAndReload(
        gridApi,
        searchValues,
        options.route.query,
      );
      lastRestoredRouteKey = routeKey;
    } finally {
      restoringListRoute = false;
    }
  }

  async function restoreOnMount(gridApi: ListReturnGridApi) {
    bindRouteRestore(gridApi);
    await restoreFromRoute(gridApi);
  }

  async function reloadListFromCurrentRoute(gridApi: ListReturnGridApi) {
    const searchValues = options.parseRouteToSearchValues
      ? options.parseRouteToSearchValues(options.route.query)
      : options.searchKeys?.length
        ? collectRouteStringParams(options.route.query, options.searchKeys)
        : {};
    return applyGridSearchFromRouteAndReload(gridApi, searchValues, options.route.query);
  }

  function bindRouteRestore(gridApi: ListReturnGridApi) {
    if (routeRestoreBound) return;
    routeRestoreBound = true;
    watch(
      () =>
        [
          options.route.query.page,
          options.route.query.pageSize,
          ...(options.searchKeys ?? []).map((k) => options.route.query[k]),
          ...(options.extraRestoreQueryKeys ?? []).map((k) => options.route.query[k]),
        ] as const,
      () => {
        if (!isActiveListRoute()) return;
        void restoreFromRoute(gridApi);
      },
    );
    watch(
      () => options.route.query[LIST_RETURN_REFRESH_QUERY_KEY],
      (refresh) => {
        if (!refresh || !isActiveListRoute()) return;
        void (async () => {
          clearRestoreKey();
          await reloadListFromCurrentRoute(gridApi);
        })();
      },
    );
    onActivated(() => {
      if (!isActiveListRoute()) return;
      void (async () => {
        if (!listActivatedOnce) {
          listActivatedOnce = true;
          await restoreFromRoute(gridApi);
          return;
        }
        clearRestoreKey();
        if (options.route.query[LIST_RETURN_REFRESH_QUERY_KEY]) {
          await reloadListFromCurrentRoute(gridApi);
          return;
        }
        await restoreFromRoute(gridApi);
        const hasRestore = hasListRestoreQuery(
          options.route.query,
          searchKeys,
          extraRestoreQueryKeys,
        );
        if (!hasRestore) {
          await gridApi.reload?.();
        }
      })();
    });
  }

  const pagerConfig = {
    currentPage: listPagerInit.currentPage,
    pageSize: listPagerInit.pageSize,
  };

  return {
    listPagerInit,
    lastListPage,
    pagerConfig,
    shouldDeferGridAutoLoad,
    trackPage,
    buildReturnQuery,
    restoreOnMount,
    restoreFromRoute,
    bindRouteRestore,
    clearRestoreKey,
  };
}

/** 将简单对象序列化为 query 字符串字段（值用 | 连接数组） */
export function serializeQueryRecord(
  record: Record<string, unknown>,
  keys: readonly string[],
): Record<string, string> {
  const result: Record<string, string> = {};
  for (const key of keys) {
    const raw = record[key];
    if (raw === undefined || raw === null || raw === '') continue;
    if (Array.isArray(raw)) {
      const joined = raw.map(String).filter(Boolean).join('|');
      if (joined) result[key] = joined;
    } else {
      result[key] = String(raw);
    }
  }
  return result;
}

/** 从 query 恢复 serializeQueryRecord 写入的字段 */
export function deserializeQueryRecord(
  query: Record<string, unknown>,
  keys: readonly string[],
): Record<string, string> {
  return collectRouteStringParams(query, keys);
}
