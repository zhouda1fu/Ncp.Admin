import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:package-variant',
      order: 9987,
      title: $t('product.title'),
      authority: [
        PermissionCodes.ProductManagement,
        PermissionCodes.ProductView,
        PermissionCodes.ProductCreate,
        PermissionCodes.ProductEdit,
        PermissionCodes.ProductDelete,
      ],
    },
    name: 'Product',
    path: '/product',
    redirect: '/product/list',
    children: [
      {
        path: '/product/list',
        name: 'ProductList',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('product.list'),
          authority: [PermissionCodes.ProductView],
        },
        component: () => import('#/views/product/list.vue'),
      },
    ],
  },
];

export default routes;
