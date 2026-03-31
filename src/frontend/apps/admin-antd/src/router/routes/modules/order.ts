import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:clipboard-list-outline',
      order: 9988,
      title: $t('order.title'),
      authority: [
        PermissionCodes.OrderManagement,
        PermissionCodes.OrderView,
        PermissionCodes.OrderCreate,
        PermissionCodes.OrderEdit,
        PermissionCodes.OrderDelete,
        PermissionCodes.OrderSubmit,
      ],
    },
    name: 'Order',
    path: '/order',
    redirect: '/order/list',
    children: [
      {
        path: '/order/list',
        name: 'OrderList',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('order.list'),
          authority: [PermissionCodes.OrderView],
        },
        component: () => import('#/views/order/list.vue'),
      },
      {
        path: '/order/create',
        name: 'OrderCreate',
        meta: {
          activePath: '/order/list',
          hideInMenu: true,
          title: $t('order.create'),
        },
        component: () => import('#/views/order/form.vue'),
      },
      {
        path: '/order/:id/edit',
        name: 'OrderEdit',
        meta: {
          activePath: '/order/list',
          hideInMenu: true,
          title: $t('order.edit'),
        },
        component: () => import('#/views/order/form.vue'),
      },
      {
        path: '/order/:id',
        name: 'OrderDetail',
        meta: {
          activePath: '/order/list',
          hideInMenu: true,
          title: $t('order.detail'),
        },
        component: () => import('#/views/order/detail.vue'),
      },
      {
        path: '/order/invoice-type',
        name: 'OrderInvoiceTypeList',
        meta: {
          icon: 'mdi:receipt-text-outline',
          title: $t('order.invoiceTypeList'),
          authority: [PermissionCodes.OrderView],
        },
        component: () => import('#/views/order/invoice-type/list.vue'),
      },
      {
        path: '/order/logistics-company',
        name: 'OrderLogisticsCompanyList',
        meta: {
          icon: 'mdi:truck-delivery-outline',
          title: $t('order.logisticsCompanyList'),
          authority: [PermissionCodes.OrderView],
        },
        component: () => import('#/views/order/logistics-company/list.vue'),
      },
      {
        path: '/order/logistics-method',
        name: 'OrderLogisticsMethodList',
        meta: {
          icon: 'mdi:truck-fast-outline',
          title: $t('order.logisticsMethodList'),
          authority: [PermissionCodes.OrderView],
        },
        component: () => import('#/views/order/logistics-method/list.vue'),
      },
    ],
  },
];

export default routes;
