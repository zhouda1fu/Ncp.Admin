import type { Router } from 'vue-router';
import type { UserInfo } from '@vben/types';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { startProgress, stopProgress } from '@vben/utils';

import { accessRoutes, coreRouteNames } from '#/router/routes';
import { useAuthStore } from '#/store';

import { decodeListReturn } from '#/utils/list-return-state';

import { generateAccess } from './access';

const WORKFLOW_INSTANCE_DETAIL_RETURN_PATHS = [
  '/workflow/pending',
  '/workflow/completed',
  '/workflow/my-workflows',
  '/workflow/monitor',
] as const;

const WORKFLOW_INSTANCE_DETAIL_ACTIVE_PATH_ROUTE_NAMES = new Set(['WorkflowInstanceDetail']);

function resolveWorkflowInstanceDetailActivePath(query: Record<string, unknown>): string {
  const decoded = decodeListReturn(query.return, WORKFLOW_INSTANCE_DETAIL_RETURN_PATHS);
  if (decoded) {
    return decoded.split('?')[0] ?? '/workflow/pending';
  }
  return '/workflow/pending';
}

/**
 * 通用守卫配置
 * @param router
 */
function setupCommonGuard(router: Router) {
  const loadedPaths = new Set<string>();

  router.beforeEach((to) => {
    to.meta.loaded = loadedPaths.has(to.path);

    if (WORKFLOW_INSTANCE_DETAIL_ACTIVE_PATH_ROUTE_NAMES.has(String(to.name))) {
      to.meta.activePath = resolveWorkflowInstanceDetailActivePath(to.query);
    }

    if (!to.meta.loaded && preferences.transition.progress) {
      startProgress();
    }
    return true;
  });

  router.afterEach((to) => {
    loadedPaths.add(to.path);

    if (preferences.transition.progress) {
      stopProgress();
    }
  });
}

/**
 * 权限访问守卫配置
 * @param router
 */
function setupAccessGuard(router: Router) {
  router.beforeEach(async (to, from) => {
    const accessStore = useAccessStore();
    const userStore = useUserStore();
    const authStore = useAuthStore();

    if (coreRouteNames.includes(to.name as string)) {
      if (to.path === LOGIN_PATH && accessStore.accessToken) {
        return decodeURIComponent(
          (to.query?.redirect as string) ||
            userStore.userInfo?.homePath ||
            preferences.app.defaultHomePath,
        );
      }
      return true;
    }

    if (!accessStore.accessToken) {
      if (to.meta.ignoreAccess) {
        return true;
      }

      if (to.fullPath !== LOGIN_PATH) {
        return {
          path: LOGIN_PATH,
          query:
            to.fullPath === preferences.app.defaultHomePath
              ? {}
              : { redirect: encodeURIComponent(to.fullPath) },
          replace: true,
        };
      }
      return to;
    }

    if (accessStore.isAccessChecked) {
      return true;
    }

    let userInfo: UserInfo | null = null;
    let accessibleMenus: any[] = [];
    let accessibleRoutes = accessRoutes;

    try {
      if (userStore.userInfo) {
        userInfo = userStore.userInfo as UserInfo;
      } else {
        const fetchedUserInfo = await authStore.fetchUserInfo();
        userInfo = fetchedUserInfo as UserInfo | null;
      }

      const permissionCodes = accessStore.accessCodes || [];

      const result = await generateAccess({
        roles: permissionCodes,
        router,
        routes: accessRoutes,
      });
      accessibleMenus = result.accessibleMenus;
      accessibleRoutes = result.accessibleRoutes;
    } catch (error) {
      console.error('生成路由失败，使用静态路由:', error);
      accessibleMenus = [];
      accessibleRoutes = accessRoutes;
      userInfo = (userStore.userInfo as UserInfo | null) || null;
    }

    accessStore.setAccessMenus(accessibleMenus);
    accessStore.setAccessRoutes(accessibleRoutes);
    accessStore.setIsAccessChecked(true);
    const redirectPath = (from.query.redirect ??
      (to.path === preferences.app.defaultHomePath
        ? userInfo?.homePath || preferences.app.defaultHomePath
        : to.fullPath)) as string;

    return {
      ...router.resolve(decodeURIComponent(redirectPath)),
      replace: true,
    };
  });
}

/**
 * 项目守卫配置
 * @param router
 */
function createRouterGuard(router: Router) {
  setupCommonGuard(router);
  setupAccessGuard(router);
}

export { createRouterGuard };
