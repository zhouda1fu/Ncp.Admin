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
        PermissionCodes.CustomerSeaVoid,
        PermissionCodes.CustomerSeaConsultationEdit,
        PermissionCodes.IndustryView,
        PermissionCodes.IndustryCreate,
        PermissionCodes.IndustryEdit,
        PermissionCodes.CustomerSourceView,
        PermissionCodes.CustomerSourceCreate,
        PermissionCodes.CustomerSourceEdit,
        PermissionCodes.CustomerContactRecordView,
        PermissionCodes.CustomerContactRecordCreate,
        PermissionCodes.CustomerContactRecordEdit,
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
        path: '/customer/create',
        name: 'CustomerCreate',
        meta: {
          activePath: '/customer/list',
          hideInMenu: true,
          title: $t('customer.create'),
        },
        component: () => import('#/views/customer/form.vue'),
      },
      {
        path: '/customer/:id/edit',
        name: 'CustomerEdit',
        meta: {
          activePath: '/customer/list',
          hideInMenu: true,
          title: $t('customer.edit'),
        },
        component: () => import('#/views/customer/form.vue'),
      },
      {
        path: '/customer/:customerId/contacts/:contactId',
        name: 'CustomerContactEdit',
        meta: {
          activePath: '/customer/list',
          hideInMenu: true,
          title: '联系人编辑',
          authority: [PermissionCodes.CustomerContactEdit],
        },
        component: () => import('#/views/customer/contact-edit.vue'),
      },
      {
        path: '/customer/:customerId/contact-records/create',
        name: 'CustomerContactRecordCreate',
        meta: {
          activePath: '/customer/contact-record',
          hideInMenu: true,
          title: $t('customer.addRecordTitle'),
          // 与可进入客户编辑/联系人编辑页的权限对齐，避免仅有 CustomerEdit 却无 CustomerContactRecordCreate 时路由被过滤导致 push 报 No match
          authority: [
            PermissionCodes.CustomerContactRecordCreate,
            PermissionCodes.CustomerEdit,
            PermissionCodes.CustomerContactEdit,
          ],
        },
        component: () => import('#/views/customer/contact-record-edit.vue'),
      },
      {
        path: '/customer/:customerId/contact-records/:recordId/edit',
        name: 'CustomerContactRecordEdit',
        meta: {
          activePath: '/customer/contact-record',
          hideInMenu: true,
          title: '编辑联络记录',
          authority: [
            PermissionCodes.CustomerContactRecordEdit,
            PermissionCodes.CustomerEdit,
            PermissionCodes.CustomerContactEdit,
          ],
        },
        component: () => import('#/views/customer/contact-record-edit.vue'),
      },
      {
        path: '/customer/contact-record',
        name: 'CustomerContactRecordListInCustomerManagement',
        meta: {
          icon: 'mdi:card-account-details-outline',
          title: '客户联络',
          authority: [PermissionCodes.CustomerContactRecordView],
          order: 9991,
        },
        component: () => import('#/views/customer-contact-record/list.vue'),
      },
      {
        path: '/customer/industry',
        name: 'CustomerIndustryList',
        meta: {
          icon: 'mdi:briefcase-outline',
          title: $t('customer.industryList'),
          authority: [
            PermissionCodes.IndustryView,
            PermissionCodes.IndustryCreate,
            PermissionCodes.IndustryEdit,
          ],
        },
        component: () => import('#/views/customer/industry/list.vue'),
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
          authority: [
            PermissionCodes.CustomerView,
            PermissionCodes.CustomerClaimFromSea,
            PermissionCodes.CustomerSeaVoid,
            PermissionCodes.CustomerSeaConsultationEdit,
          ],
        },
        component: () => import('#/views/customer/sea/list.vue'),
      },
      {
        path: '/customer/sea/region-assign',
        name: 'CustomerSeaRegionAssign',
        meta: {
          activePath: '/customer/sea/region-assign',
          icon: 'mdi:map',
          title: $t('customer.seaRegionAssign'),
          authority: [PermissionCodes.CustomerSeaRegionAssignView, PermissionCodes.CustomerSeaRegionAssignEdit],
        },
        component: () => import('#/views/customer/sea/region-assign/list.vue'),
      },
    ],
  },
];

export default routes;
