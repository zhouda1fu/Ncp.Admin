import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:file-document-multiple',
      order: 9993,
      title: $t('document.title'),
      authority: [
        PermissionCodes.DocumentManagement,
        PermissionCodes.DocumentView,
        PermissionCodes.DocumentCreate,
        PermissionCodes.DocumentEdit,
        PermissionCodes.DocumentShare,
      ],
    },
    name: 'Document',
    path: '/document',
    children: [
      {
        path: '/document/list',
        name: 'DocumentList',
        meta: {
          icon: 'mdi:folder-open',
          title: $t('document.list'),
          authority: [PermissionCodes.DocumentView],
        },
        component: () => import('#/views/document/list.vue'),
      },
    ],
  },
];

export default routes;
