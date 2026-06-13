import type { TabDefinition } from '@vben/types';
import type { RouteLocationNormalized, Router } from 'vue-router';

import { toRaw } from 'vue';
import { isNavigationFailure, NavigationFailureType } from 'vue-router';

import { getTabKey, useTabbarStore } from '@vben/stores';

import { decodeListReturnLoose, parseListReturnRouteLocation, syncListTabFullPath } from '#/utils/list-return-state';

function getTabKeyFromTab(tab: TabDefinition): string {
  return tab.key ?? getTabKey(tab as RouteLocationNormalized);
}

function equalTab(a: TabDefinition, b: TabDefinition): boolean {
  return getTabKeyFromTab(a) === getTabKeyFromTab(b);
}

/** 路由与 tab 是否同一页（忽略 query 差异，避免详情 tab key 与 fullPath 不一致） */
function isTabActive(tab: TabDefinition, route: RouteLocationNormalized): boolean {
  if (getTabKey(route) === getTabKeyFromTab(tab)) return true;
  const tabPath = (tab.path ?? tab.fullPath ?? '').split('?')[0] ?? '';
  return Boolean(tab.name && tab.name === route.name && tabPath === route.path);
}

function findListTabByPath(tabs: TabDefinition[], pathOnly: string): TabDefinition | undefined {
  return tabs.find((t) => {
    const tabPath = (t.path ?? t.fullPath ?? '').split('?')[0] ?? '';
    return tabPath === pathOnly;
  });
}

function readReturnFromTabOrRoute(
  tab: TabDefinition,
  route: RouteLocationNormalized,
  active: boolean,
): unknown {
  if (active) return route.query.return;
  return tab.query?.return;
}

async function navigateToListAndCloseTab(
  tabbarStore: ReturnType<typeof useTabbarStore>,
  tab: TabDefinition,
  router: Router,
  returnPath: string,
): Promise<void> {
  const pathOnly = returnPath.split('?')[0] ?? '';
  syncListTabFullPath(returnPath);

  let index = tabbarStore.getTabs.findIndex((item) => equalTab(item, tab));
  if (index === -1) {
    index = tabbarStore.getTabs.findIndex((item) => isTabActive(item, router.currentRoute.value));
  }

  const before = index >= 0 ? tabbarStore.getTabs[index - 1] : undefined;
  const after = index >= 0 ? tabbarStore.getTabs[index + 1] : undefined;
  const fallbackNext =
    after ??
    before ??
    tabbarStore.getTabs.find((item) => !equalTab(item, tab));

  const listTab = findListTabByPath(tabbarStore.getTabs, pathOnly) ?? fallbackNext;
  if (!listTab) return;

  const { path, query } = parseListReturnRouteLocation(returnPath);
  try {
    await router.replace({ path, query });
  } catch (error) {
    const duplicated =
      isNavigationFailure(error, NavigationFailureType.duplicated) ||
      isNavigationFailure(error, NavigationFailureType.cancelled);
    if (!duplicated) {
      throw error;
    }
  }
  /** Vben tabbar 内部关闭方法，无公开 API 时用于先导航再移除当前 tab */
  tabbarStore._close(tab);
  await tabbarStore.updateCacheTabs();
}

/**
 * 同一路由 name + path 仅 query 不同时合并 tab（仅 fullPathKey:false 的列表页强制 path 级 key）。
 */
function patchAddTabMergeByRouteIdentity(tabbarStore: ReturnType<typeof useTabbarStore>): void {
  const originalAddTab = tabbarStore.addTab.bind(tabbarStore);

  tabbarStore.addTab = function patchedAddTab(routeTab: TabDefinition) {
    if (routeTab.name && routeTab.path) {
      const existingIdx = tabbarStore.tabs.findIndex(
        (item) => item.name === routeTab.name && item.path === routeTab.path,
      );
      if (existingIdx >= 0) {
        const currentTab = toRaw(tabbarStore.tabs)[existingIdx];
        const mergedTab: TabDefinition = {
          ...currentTab,
          ...routeTab,
          meta: { ...currentTab?.meta, ...routeTab.meta },
        };
        if (currentTab?.meta?.affixTab) {
          mergedTab.meta = { ...mergedTab.meta, affixTab: true };
        }
        if (Reflect.has(currentTab?.meta ?? {}, 'newTabTitle')) {
          mergedTab.meta = {
            ...mergedTab.meta,
            newTabTitle: currentTab?.meta?.newTabTitle,
          };
        }
        const usePathKey =
          routeTab.meta?.fullPathKey === false || currentTab?.meta?.fullPathKey === false;
        mergedTab.key = usePathKey
          ? routeTab.path
          : getTabKey(routeTab as RouteLocationNormalized);
        tabbarStore.tabs.splice(existingIdx, 1, mergedTab);
        void tabbarStore.updateCacheTabs();
        return mergedTab;
      }
    }
    return originalAddTab(routeTab);
  };
}

/**
 * 关闭带 `return` 的详情 tab：回到已有列表 tab 并带上分页/搜索 query，不新增重复列表 tab。
 */
function patchCloseTabWithListReturn(tabbarStore: ReturnType<typeof useTabbarStore>): void {
  const originalCloseTab = tabbarStore.closeTab.bind(tabbarStore);

  tabbarStore.closeTab = async function patchedCloseTab(tab: TabDefinition, router: Router) {
    const { currentRoute } = router;
    const active = isTabActive(tab, currentRoute.value);
    const returnPath = decodeListReturnLoose(
      readReturnFromTabOrRoute(tab, currentRoute.value, active),
    );

    if (returnPath && active) {
      await navigateToListAndCloseTab(tabbarStore, tab, router, returnPath);
      return;
    }

    await originalCloseTab(tab, router);
  };
}

export function setupListReturnTabClose(): void {
  const tabbarStore = useTabbarStore();
  patchAddTabMergeByRouteIdentity(tabbarStore);
  patchCloseTabWithListReturn(tabbarStore);
}
