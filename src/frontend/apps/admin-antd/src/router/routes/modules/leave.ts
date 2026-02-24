import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:calendar-account',
      order: 9995,
      title: $t('leave.title'),
      // 拥有任一请假相关权限即可显示父菜单
      authority: [
        PermissionCodes.LeaveManagement,
        PermissionCodes.LeaveRequestView,
        PermissionCodes.LeaveRequestCreate,
        PermissionCodes.LeaveBalanceView,
        PermissionCodes.LeaveBalanceEdit,
      ],
    },
    name: 'Leave',
    path: '/leave',
    children: [
      {
        path: '/leave/requests',
        name: 'LeaveRequests',
        meta: {
          icon: 'mdi:file-document-edit',
          title: $t('leave.request.title'),
          authority: [PermissionCodes.LeaveRequestView],
        },
        component: () => import('#/views/leave/requests/list.vue'),
      },
      {
        path: '/leave/balances',
        name: 'LeaveBalances',
        meta: {
          icon: 'mdi:counter',
          title: $t('leave.balance.title'),
          authority: [PermissionCodes.LeaveBalanceView],
        },
        component: () => import('#/views/leave/balances/list.vue'),
      },
    ],
  },
];

export default routes;
