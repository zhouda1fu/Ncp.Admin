import { onActivated, onMounted, watch } from 'vue';
import type { RouteLocationNormalizedLoaded } from 'vue-router';

import {
  collectRouteStringParams,
  hasListRestoreQuery,
  LIST_RETURN_REFRESH_QUERY_KEY,
  readPagerFromRouteQuery,
  type ListPager,
} from '#/utils/list-return-state';

export interface UseSimpleListRouteRestoreOptions {
  route: RouteLocationNormalizedLoaded;
  searchKeys?: readonly string[];
  extraRestoreQueryKeys?: readonly string[];
  /** 恢复分页与 query 字段到页面状态（不含拉数） */
  applyRestore: (pager: ListPager, fields: Record<string, string>) => void;
  reload: () => void | Promise<void>;
  buildRouteRestoreKey?: () => string;
}

/** 非 VxeGrid 列表（Ant Design Table 等）：返回详情时恢复分页与筛选 */
export function useSimpleListRouteRestore(options: UseSimpleListRouteRestoreOptions) {
  let restoring = false;
  let lastRestoredRouteKey = '';

  function buildDefaultRouteRestoreKey() {
    return JSON.stringify({
      page: options.route.query.page,
      pageSize: options.route.query.pageSize,
      search: (options.searchKeys ?? []).map((k) => options.route.query[k]),
      extra: (options.extraRestoreQueryKeys ?? []).map((k) => options.route.query[k]),
    });
  }

  async function restoreFromRoute() {
    if (
      !hasListRestoreQuery(
        options.route.query,
        options.searchKeys ?? [],
        options.extraRestoreQueryKeys ?? [],
      )
    ) {
      return;
    }
    const routeKey = options.buildRouteRestoreKey?.() ?? buildDefaultRouteRestoreKey();
    if (restoring || routeKey === lastRestoredRouteKey) return;
    restoring = true;
    try {
      const pager = readPagerFromRouteQuery(options.route.query);
      const fields = collectRouteStringParams(options.route.query, [
        ...(options.searchKeys ?? []),
        ...(options.extraRestoreQueryKeys ?? []),
      ]);
      options.applyRestore(pager, fields);
      await options.reload();
      lastRestoredRouteKey = routeKey;
    } finally {
      restoring = false;
    }
  }

  function clearRestoreKey() {
    lastRestoredRouteKey = '';
  }

  async function reloadFromRefreshQuery() {
    clearRestoreKey();
    await options.reload();
  }

  onMounted(() => {
    void (async () => {
      if (options.route.query[LIST_RETURN_REFRESH_QUERY_KEY]) {
        await reloadFromRefreshQuery();
        return;
      }
      if (
        hasListRestoreQuery(
          options.route.query,
          options.searchKeys ?? [],
          options.extraRestoreQueryKeys ?? [],
        )
      ) {
        await restoreFromRoute();
      } else {
        await options.reload();
      }
    })();
  });

  watch(
    () =>
      [
        options.route.query.page,
        options.route.query.pageSize,
        ...(options.searchKeys ?? []).map((k) => options.route.query[k]),
        ...(options.extraRestoreQueryKeys ?? []).map((k) => options.route.query[k]),
      ] as const,
    () => {
      void restoreFromRoute();
    },
  );

  watch(
    () => options.route.query[LIST_RETURN_REFRESH_QUERY_KEY],
    (refresh) => {
      if (!refresh) return;
      void reloadFromRefreshQuery();
    },
  );

  onActivated(() => {
    void (async () => {
      if (options.route.query[LIST_RETURN_REFRESH_QUERY_KEY]) {
        await reloadFromRefreshQuery();
        return;
      }
      await restoreFromRoute();
    })();
  });

  return { restoreFromRoute, clearRestoreKey, reloadFromRefreshQuery };
}
