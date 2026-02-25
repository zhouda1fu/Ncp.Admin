import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:card-account-details',
      order: 9992,
      title: $t('contact.title'),
      authority: [
        PermissionCodes.ContactManagement,
        PermissionCodes.ContactGroupView,
        PermissionCodes.ContactGroupCreate,
        PermissionCodes.ContactGroupEdit,
        PermissionCodes.ContactView,
        PermissionCodes.ContactCreate,
        PermissionCodes.ContactEdit,
      ],
    },
    name: 'Contact',
    path: '/contact',
    children: [
      {
        path: '/contact/groups',
        name: 'ContactGroups',
        meta: {
          icon: 'mdi:folder-account',
          title: $t('contact.group.list'),
          authority: [PermissionCodes.ContactGroupView],
        },
        component: () => import('#/views/contact/groups/list.vue'),
      },
      {
        path: '/contact/contacts',
        name: 'ContactContacts',
        meta: {
          icon: 'mdi:account-multiple',
          title: $t('contact.contact.list'),
          authority: [PermissionCodes.ContactView],
        },
        component: () => import('#/views/contact/contacts/list.vue'),
      },
    ],
  },
];

export default routes;
