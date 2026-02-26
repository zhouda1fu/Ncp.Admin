import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:package-variant',
      order: 9990,
      title: $t('asset.title'),
      authority: [
        PermissionCodes.AssetManagement,
        PermissionCodes.AssetView,
        PermissionCodes.AssetAllocationView,
      ],
    },
    name: 'Asset',
    path: '/asset',
    children: [
      {
        path: '/asset/list',
        name: 'AssetList',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('asset.list'),
          authority: [PermissionCodes.AssetView],
        },
        component: () => import('#/views/asset/list.vue'),
      },
      {
        path: '/asset/allocations',
        name: 'AssetAllocationList',
        meta: {
          icon: 'mdi:hand-extended',
          title: $t('asset.allocationList'),
          authority: [PermissionCodes.AssetAllocationView],
        },
        component: () => import('#/views/asset/allocations/list.vue'),
      },
    ],
  },
];

export default routes;
