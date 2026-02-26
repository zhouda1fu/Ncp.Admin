import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:file-document-edit',
      order: 9991,
      title: $t('contract.title'),
      authority: [
        PermissionCodes.ContractManagement,
        PermissionCodes.ContractView,
        PermissionCodes.ContractCreate,
        PermissionCodes.ContractEdit,
        PermissionCodes.ContractSubmit,
        PermissionCodes.ContractApprove,
        PermissionCodes.ContractArchive,
      ],
    },
    name: 'Contract',
    path: '/contract',
    children: [
      {
        path: '/contract/list',
        name: 'ContractList',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('contract.list'),
          authority: [PermissionCodes.ContractView],
        },
        component: () => import('#/views/contract/list.vue'),
      },
    ],
  },
];

export default routes;
