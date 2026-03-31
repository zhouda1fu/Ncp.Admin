import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ion:settings-outline',
      order: 9997,
      title: $t('system.title'),
      // 父路由需要任一权限即可显示
      authority: [PermissionCodes.RoleManagement, PermissionCodes.DeptManagement, PermissionCodes.UserManagement, PermissionCodes.UserView, PermissionCodes.UserExport, PermissionCodes.UserImport, PermissionCodes.PositionManagement, PermissionCodes.OperationLogManagement, PermissionCodes.OperationLogView],
    },
    name: 'System',
    path: '/system',
    children: [
      {
        path: '/system/role',
        name: 'SystemRole',
        meta: {
          icon: 'mdi:account-group',
          title: $t('system.role.title'),
          authority: [PermissionCodes.RoleManagement], // 使用权限码控制访问
        },
        component: () => import('#/views/system/role/list.vue'),
      },
      {
        path: '/system/dept',
        name: 'SystemDept',
        meta: {
          icon: 'charm:organisation',
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
          title: $t('system.user.title'),
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
          hideInMenu: true,
          title: $t('common.edit', [$t('system.user.name')]),
        },
        component: () => import('#/views/system/user/form.vue'),
      },
      {
        path: '/system/position',
        name: 'SystemPosition',
        meta: {
          icon: 'mdi:briefcase-outline',
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
          title: $t('system.operationLog.title'),
          authority: [PermissionCodes.OperationLogView],
        },
        component: () => import('#/views/system/operation-log/list.vue'),
      },
    ],
  },
];

export default routes;
