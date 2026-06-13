import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';
import { listReturnListRouteMeta } from '#/router/list-return-route-meta';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      moduleCode: PermissionCodes.SystemModule,
      icon: 'mdi:monitor-dashboard',
      order: 30,
      title: $t('system.title'),
      // 侧栏模块入口仅由各子模块 Management 权限码控制；子菜单仍按各自 authority 过滤。
      authority: [
        PermissionCodes.RoleManagement,
        PermissionCodes.RoleView,
        PermissionCodes.RoleCreate,
        PermissionCodes.RoleEdit,
        PermissionCodes.RoleDelete,
        PermissionCodes.RoleUpdatePermissions,
        PermissionCodes.DeptManagement,
        PermissionCodes.UserManagement,
        PermissionCodes.PositionManagement,
        PermissionCodes.OperationLogManagement,
        PermissionCodes.SystemLogManagement,
        PermissionCodes.BackgroundJobManagement,
        PermissionCodes.WorkflowManagement,
      ],
    },
    name: 'System',
    path: '/system',
    redirect: '/system/role',
    children: [
      {
        path: '/system/role',
        name: 'SystemRole',
        meta: {
          icon: 'mdi:format-list-bulleted',
          order: 10,
          title: $t('system.role.list'),
          authority: [
            PermissionCodes.RoleManagement,
            PermissionCodes.RoleView,
            PermissionCodes.RoleCreate,
            PermissionCodes.RoleEdit,
            PermissionCodes.RoleDelete,
            PermissionCodes.RoleUpdatePermissions,
          ], // 使用权限码控制访问
        },
        component: () => import('#/views/system/role/list.vue'),
      },
      {
        path: '/system/dept',
        name: 'SystemDept',
        meta: {
          icon: 'charm:organisation',
          order: 40,
          title: $t('system.dept.title'),
          authority: [PermissionCodes.DeptManagement], // 使用权限码控制访问
        },
        component: () => import('#/views/system/dept/list.vue'),
      },
      {
        path: '/system/user',
        name: 'SystemUser',
        meta: {
          icon: 'mdi:account',
          order: 20,
          title: $t('system.user.title'),
          ...listReturnListRouteMeta,
          authority: [PermissionCodes.UserManagement], // 使用权限码控制访问
        },
        component: () => import('#/views/system/user/list.vue'),
      },
      {
        path: '/system/user/create',
        name: 'SystemUserCreate',
        meta: {
          activePath: '/system/user',
          hideInMenu: true,
          title: $t('common.create', [$t('system.user.name')]),
        },
        component: () => import('#/views/system/user/form.vue'),
      },
      {
        path: '/system/user/:id/edit',
        name: 'SystemUserEdit',
        meta: {
          activePath: '/system/user',
          authority: [PermissionCodes.UserEdit],
          hideInMenu: true,
          title: $t('common.edit', [$t('system.user.name')]),
        },
        component: () => import('#/views/system/user/form.vue'),
      },
      {
        path: '/system/user/:id/view',
        name: 'SystemUserView',
        meta: {
          activePath: '/system/user',
          authority: [PermissionCodes.UserView],
          hideInMenu: true,
          title: $t('common.view', [$t('system.user.name')]),
        },
        component: () => import('#/views/system/user/form.vue'),
      },
      {
        path: '/system/position',
        name: 'SystemPosition',
        meta: {
          icon: 'mdi:briefcase-outline',
          order: 50,
          title: $t('system.position.title'),
          authority: [PermissionCodes.PositionManagement],
        },
        component: () => import('#/views/system/position/list.vue'),
      },
      {
        path: '/system/org-users',
        name: 'SystemOrgUsers',
        meta: {
          icon: 'charm:people',
          order: 60,
          title: $t('system.orgUsers.title'),
          authority: [PermissionCodes.UserView],
        },
        component: () => import('#/views/system/org-users/index.vue'),
      },
      {
        path: '/system/operation-log',
        name: 'SystemOperationLog',
        meta: {
          icon: 'mdi:history',
          order: 70,
          title: $t('system.operationLog.title'),
          authority: [PermissionCodes.OperationLogView],
        },
        component: () => import('#/views/system/operation-log/list.vue'),
      },
      {
        path: '/system/system-log',
        name: 'SystemLog',
        meta: {
          icon: 'mdi:alert-circle-outline',
          order: 80,
          title: $t('system.systemLog.title'),
          authority: [PermissionCodes.SystemLogView],
        },
        component: () => import('#/views/system/system-log/list.vue'),
      },
      {
        path: '/system/background-jobs',
        name: 'SystemBackgroundJobs',
        meta: {
          icon: 'mdi:timer-cog-outline',
          order: 90,
          title: '定时任务',
          authority: [PermissionCodes.BackgroundJobView],
        },
        component: () => import('#/views/system/background-jobs/list.vue'),
      },
    ],
  },
];

export default routes;
