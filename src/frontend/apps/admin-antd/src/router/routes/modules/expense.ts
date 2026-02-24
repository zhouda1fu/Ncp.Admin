import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:receipt',
      order: 9993,
      title: $t('expense.title'),
      authority: [
        PermissionCodes.ExpenseManagement,
        PermissionCodes.ExpenseClaimView,
        PermissionCodes.ExpenseClaimCreate,
        PermissionCodes.ExpenseClaimSubmit,
      ],
    },
    name: 'Expense',
    path: '/expense',
    children: [
      {
        path: '/expense/claims',
        name: 'ExpenseClaims',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('expense.claim.list'),
          authority: [PermissionCodes.ExpenseClaimView],
        },
        component: () => import('#/views/expense/claims/list.vue'),
      },
    ],
  },
];

export default routes;
