import type { RouteRecordRaw } from 'vue-router';

import {
  isModuleCategoryNonMenuRootPermission,
  MODULE_CATEGORY_DEFINITIONS,
} from '#/constants/module-menu-categories';

function toAuthorityList(authority: unknown): string[] {
  if (typeof authority === 'string') {
    return [authority];
  }
  if (Array.isArray(authority)) {
    return authority.filter((item): item is string => typeof item === 'string');
  }
  return [];
}

function collectRouteAuthorities(route: RouteRecordRaw): string[] {
  const codes = new Set(toAuthorityList(route.meta?.authority));

  for (const child of route.children ?? []) {
    for (const code of collectRouteAuthorities(child)) {
      codes.add(code);
    }
  }

  return [...codes];
}

function uniqueAuthorities(codes: string[]): string[] {
  return [...new Set(codes)];
}

function collectDirectRouteAuthorities(route: RouteRecordRaw): string[] {
  return toAuthorityList(route.meta?.authority);
}

function collectModuleEntryAuthorities(children: RouteRecordRaw[]): string[] {
  return uniqueAuthorities(
    children
      .flatMap(collectDirectRouteAuthorities)
      .filter((code) => !isModuleCategoryNonMenuRootPermission(code)),
  );
}

export function pruneEmptyModuleCategoryRoutes(
  routes: RouteRecordRaw[],
): RouteRecordRaw[] {
  const moduleNames = new Set(
    MODULE_CATEGORY_DEFINITIONS.map((item) => item.permissionCode),
  );

  return routes
    .filter((route) => {
      if (!moduleNames.has(String(route.name ?? ''))) {
        return true;
      }
      return (route.children?.length ?? 0) > 0;
    })
    .map((route) => {
      if (!route.children?.length) {
        return route;
      }

      return {
        ...route,
        children: pruneEmptyModuleCategoryRoutes(route.children),
      } as RouteRecordRaw;
    });
}

function withDescendantAuthorities(route: RouteRecordRaw): RouteRecordRaw {
  const cloned = {
    ...route,
    children: route.children?.map(withDescendantAuthorities),
    meta: route.meta ? { ...route.meta } : route.meta,
  } as RouteRecordRaw;

  // 父路由已显式配置 authority 时保留原值（如 CustomerManagement），
  // 避免子菜单权限（CustomerView 等）导致父级「客户管理」误显示。
  const explicitAuthority = toAuthorityList(route.meta?.authority);
  if (explicitAuthority.length > 0) {
    return cloned;
  }

  const authorities = collectRouteAuthorities(cloned);

  if (authorities.length > 0) {
    cloned.meta = {
      ...cloned.meta,
      authority: authorities,
    } as RouteRecordRaw['meta'];
  }

  return cloned;
}

export function wrapRoutesByModule(routes: RouteRecordRaw[]): RouteRecordRaw[] {
  const moduleChildren = new Map<string, RouteRecordRaw[]>(
    MODULE_CATEGORY_DEFINITIONS.map((item) => [item.permissionCode, []]),
  );
  const ungroupedRoutes: RouteRecordRaw[] = [];

  for (const route of routes) {
    const moduleCode = route.meta?.moduleCode;
    const group = moduleCode ? moduleChildren.get(moduleCode) : undefined;

    if (group) {
      const cloned = withDescendantAuthorities(route);
      if (cloned.meta?.flattenInModule && cloned.children?.length) {
        group.push(...cloned.children.map(withDescendantAuthorities));
      } else {
        group.push(cloned);
      }
      continue;
    }

    ungroupedRoutes.push(withDescendantAuthorities(route));
  }

  const moduleRoutes: RouteRecordRaw[] = [];

  for (const definition of MODULE_CATEGORY_DEFINITIONS) {
    const children = moduleChildren.get(definition.permissionCode) ?? [];
    if (children.length === 0) continue;

    moduleRoutes.push({
      name: definition.permissionCode,
      path: `/${definition.permissionCode}`,
      redirect: children[0]?.path,
      meta: {
        // 一级模块入口：模块权限码 + 各侧栏子模块 Management 权限；不含子页面 View 级权限，
        // 也不含通知管理/首页工作台等无侧栏菜单的权限根。
        authority: uniqueAuthorities([
          definition.permissionCode,
          ...collectModuleEntryAuthorities(children),
        ]),
        icon: definition.icon,
        order: definition.order,
        title: definition.title,
      },
      children,
    } as RouteRecordRaw);
  }

  return [...ungroupedRoutes, ...moduleRoutes];
}
