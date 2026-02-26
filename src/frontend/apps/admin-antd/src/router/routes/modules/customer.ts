import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:account-group',
      order: 9989,
      title: $t('customer.title'),
      authority: [
        PermissionCodes.CustomerManagement,
        PermissionCodes.CustomerView,
        PermissionCodes.CustomerCreate,
        PermissionCodes.CustomerEdit,
        PermissionCodes.CustomerReleaseToSea,
        PermissionCodes.CustomerClaimFromSea,
        PermissionCodes.IndustryView,
      ],
    },
    name: 'Customer',
    path: '/customer',
    children: [
      {
        path: '/customer/list',
        name: 'CustomerList',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('customer.list'),
          authority: [PermissionCodes.CustomerView],
        },
        component: () => import('#/views/customer/list.vue'),
      },
    ],
  },
];

export default routes;
