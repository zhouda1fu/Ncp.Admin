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
        PermissionCodes.CustomerDelete,
        PermissionCodes.CustomerContactEdit,
        PermissionCodes.CustomerReleaseToSea,
        PermissionCodes.CustomerClaimFromSea,
        PermissionCodes.IndustryView,
        PermissionCodes.CustomerSourceView,
        PermissionCodes.CustomerSourceCreate,
        PermissionCodes.CustomerSourceEdit,
      ],
    },
    name: 'Customer',
    path: '/customer',
    redirect: '/customer/list',
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
      {
        path: '/customer/source',
        name: 'CustomerSourceList',
        meta: {
          icon: 'mdi:source-branch',
          title: $t('customer.sourceList'),
          authority: [PermissionCodes.CustomerSourceView],
        },
        component: () => import('#/views/customer/source/list.vue'),
      },
      {
        path: '/customer/sea',
        name: 'CustomerSeaList',
        meta: {
          icon: 'mdi:waves',
          title: $t('customer.sea'),
          authority: [PermissionCodes.CustomerView, PermissionCodes.CustomerClaimFromSea],
        },
        component: () => import('#/views/customer/sea/list.vue'),
      },
    ],
  },
];

export default routes;
