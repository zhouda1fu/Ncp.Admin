import type {
  ComponentRecordType,
  GenerateMenuAndRoutesOptions,
  MenuRecordRaw,
} from '@vben/types';

import { generateAccessible } from '@vben/access';
import { preferences } from '@vben/preferences';
import { MODULE_CATEGORY_DEFINITIONS } from '#/constants/module-menu-categories';
import { BasicLayout, IFrameView } from '#/layouts';
import { normalizeDynamicListMenuPaths } from '#/utils/normalize-dynamic-list-menu-paths';

import {
  pruneEmptyModuleCategoryRoutes,
  wrapRoutesByModule,
} from './wrap-routes-by-module';

function pruneEmptyModuleCategoryMenus(menus: MenuRecordRaw[]): MenuRecordRaw[] {
  const modulePaths = new Set(
    MODULE_CATEGORY_DEFINITIONS.map((item) => `/${item.permissionCode}`),
  );

  return menus
    .filter((menu) => {
      if (!modulePaths.has(menu.path)) {
        return true;
      }
      return (menu.children?.length ?? 0) > 0;
    })
    .map((menu) => ({
      ...menu,
      children: menu.children?.length
        ? pruneEmptyModuleCategoryMenus(menu.children)
        : menu.children,
    }));
}

const forbiddenComponent = () => import('#/views/_core/fallback/forbidden.vue');

async function generateAccess(options: GenerateMenuAndRoutesOptions) {
  const pageMap: ComponentRecordType = import.meta.glob('../views/**/*.vue');

  const layoutMap: ComponentRecordType = {
    BasicLayout,
    IFrameView,
  };

  const { accessibleMenus, accessibleRoutes } = await generateAccessible(
    preferences.app.accessMode,
    {
      ...options,
      forbiddenComponent,
      layoutMap,
      pageMap,
      routes: wrapRoutesByModule(options.routes),
    },
  );

  const prunedRoutes = pruneEmptyModuleCategoryRoutes(accessibleRoutes);
  const groupedMenus = normalizeDynamicListMenuPaths(
    pruneEmptyModuleCategoryMenus(accessibleMenus),
  );

  return {
    accessibleRoutes: prunedRoutes,
    accessibleMenus: groupedMenus,
  };
}

export { generateAccess };
