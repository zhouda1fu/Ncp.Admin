import type { RouteRecordRaw } from 'vue-router';

import { describe, expect, it } from 'vitest';

import { hasAuthority } from '@vben/utils';

import { PermissionCodes } from '#/constants/permission-codes';

import {
  pruneEmptyModuleCategoryRoutes,
  wrapRoutesByModule,
} from './wrap-routes-by-module';

describe('wrapRoutesByModule', () => {
  it('keeps explicit parent authority without merging child permissions', () => {
    const routes: RouteRecordRaw[] = [
      {
        name: 'System',
        path: '/system',
        meta: {
          moduleCode: PermissionCodes.SystemModule,
          authority: [PermissionCodes.RoleManagement],
          title: 'System',
        },
        children: [
          {
            name: 'SystemRole',
            path: '/system/role',
            component: { template: '<div />' },
            meta: {
              authority: [PermissionCodes.RoleView],
              title: 'SystemRole',
            },
          },
        ],
      },
    ];

    const wrapped = wrapRoutesByModule(routes);
    const systemModule = wrapped.find(
      (route) => route.name === PermissionCodes.SystemModule,
    );
    const systemRoute = systemModule?.children?.find(
      (route) => route.name === 'System',
    );

    expect(systemRoute?.meta?.authority).toEqual([
      PermissionCodes.RoleManagement,
    ]);
    expect(
      hasAuthority(systemRoute as RouteRecordRaw, [PermissionCodes.RoleView]),
    ).toBe(false);
    expect(
      hasAuthority(systemRoute as RouteRecordRaw, [
        PermissionCodes.RoleManagement,
      ]),
    ).toBe(true);
  });

  it('does not expose system module for notification and home dashboard only', () => {
    const routes: RouteRecordRaw[] = [
      {
        name: 'System',
        path: '/system',
        meta: {
          moduleCode: PermissionCodes.SystemModule,
          authority: [PermissionCodes.RoleManagement],
          title: 'System',
        },
        children: [
          {
            name: 'SystemRole',
            path: '/system/role',
            component: { template: '<div />' },
            meta: {
              authority: [PermissionCodes.RoleView],
              title: 'SystemRole',
            },
          },
        ],
      },
    ];

    const wrapped = wrapRoutesByModule(routes);
    const systemModule = wrapped.find(
      (route) => route.name === PermissionCodes.SystemModule,
    );

    expect(systemModule?.meta?.authority).toEqual([
      PermissionCodes.SystemModule,
      PermissionCodes.RoleManagement,
    ]);
    expect(
      hasAuthority(systemModule as RouteRecordRaw, [
        PermissionCodes.NotificationManagement,
        PermissionCodes.HomeDashboard,
      ]),
    ).toBe(false);
    expect(
      hasAuthority(systemModule as RouteRecordRaw, [
        PermissionCodes.RoleManagement,
      ]),
    ).toBe(true);
  });

  it('removes empty module category routes after filtering', () => {
    const routes: RouteRecordRaw[] = [
      {
        name: PermissionCodes.SystemModule,
        path: '/SystemModule',
        meta: {
          authority: [PermissionCodes.SystemModule],
          title: 'SystemModule',
        },
        children: [],
      },
      {
        name: 'Dashboard',
        path: '/dashboard',
        children: [],
      },
    ];

    expect(pruneEmptyModuleCategoryRoutes(routes)).toEqual([
      {
        name: 'Dashboard',
        path: '/dashboard',
        children: [],
      },
    ]);
  });
});
