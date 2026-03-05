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
        PermissionCodes.ContractDelete,
        PermissionCodes.ContractSubmit,
        PermissionCodes.ContractApprove,
        PermissionCodes.ContractArchive,
        PermissionCodes.ContractTypeView,
        PermissionCodes.ContractTypeCreate,
        PermissionCodes.ContractTypeEdit,
        PermissionCodes.ContractTypeDelete,
        PermissionCodes.IncomeExpenseTypeView,
        PermissionCodes.IncomeExpenseTypeCreate,
        PermissionCodes.IncomeExpenseTypeEdit,
        PermissionCodes.IncomeExpenseTypeDelete,
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
      {
        path: '/contract/create',
        name: 'ContractCreate',
        meta: {
          title: $t('contract.create'),
          authority: [PermissionCodes.ContractCreate],
          hideInMenu: true,
        },
        component: () => import('#/views/contract/form-page.vue'),
      },
      {
        path: '/contract/edit/:id',
        name: 'ContractEdit',
        meta: {
          title: $t('contract.edit'),
          authority: [PermissionCodes.ContractEdit],
          hideInMenu: true,
        },
        component: () => import('#/views/contract/form-page.vue'),
      },
      {
        path: '/contract/contract-type',
        name: 'ContractTypeList',
        meta: {
          icon: 'mdi:format-list-group',
          title: $t('contract.contractTypeList'),
          authority: [PermissionCodes.ContractTypeView],
        },
        component: () => import('#/views/contract/contract-type/list.vue'),
      },
      {
        path: '/contract/income-expense-type',
        name: 'IncomeExpenseTypeList',
        meta: {
          icon: 'mdi:cash-multiple',
          title: $t('contract.incomeExpenseTypeList'),
          authority: [PermissionCodes.IncomeExpenseTypeView],
        },
        component: () => import('#/views/contract/income-expense-type/list.vue'),
      },
    ],
  },
];

export default routes;
