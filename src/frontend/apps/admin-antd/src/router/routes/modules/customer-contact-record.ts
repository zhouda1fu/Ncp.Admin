import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:card-account-details-outline',
      order: 9988,
      title: '客户联络',
      hideInMenu: true,
      authority: [
        PermissionCodes.CustomerContactRecordView,
        PermissionCodes.CustomerContactRecordCreate,
        PermissionCodes.CustomerContactRecordEdit,
      ],
    },
    name: 'CustomerContactRecord',
    path: '/customer-contact-record',
    redirect: '/customer-contact-record/list',
    children: [
      {
        path: '/customer-contact-record/list',
        name: 'CustomerContactRecordListLegacy',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: '客户联络',
          hideInMenu: true,
          authority: [PermissionCodes.CustomerContactRecordView],
        },
        component: () => import('#/views/customer-contact-record/list.vue'),
      },
    ],
  },
];

export default routes;

