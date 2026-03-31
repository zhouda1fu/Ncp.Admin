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
      {
        path: '/product/form',
        name: 'ProductFormCreate',
        meta: {
          hideInMenu: true,
          title: $t('product.create'),
          authority: [PermissionCodes.ProductView, PermissionCodes.ProductCreate],
        },
        component: () => import('#/views/product/form-page.vue'),
      },
      {
        path: '/product/form/:id',
        name: 'ProductFormEdit',
        meta: {
          hideInMenu: true,
          title: $t('product.edit'),
          authority: [PermissionCodes.ProductView, PermissionCodes.ProductEdit],
        },
        component: () => import('#/views/product/form-page.vue'),
      },
      {
        path: '/product/category',
        name: 'ProductCategory',
        meta: {
          icon: 'mdi:sitemap',
          title: $t('product.categoryList'),
          authority: [PermissionCodes.ProductView],
        },
        component: () => import('#/views/product/category-list.vue'),
      },
      {
        path: '/product/type',
        name: 'ProductType',
        meta: {
          icon: 'mdi:tag-multiple',
          title: $t('product.productTypeList'),
          authority: [PermissionCodes.ProductView],
        },
        component: () => import('#/views/product/product-type-list.vue'),
      },
      {
        path: '/product/supplier',
        name: 'ProductSupplier',
        meta: {
          icon: 'mdi:truck-delivery',
          title: $t('product.supplierList'),
          authority: [PermissionCodes.ProductView],
        },
        component: () => import('#/views/product/supplier-list.vue'),
      },
    ],
  },
];

export default routes;
