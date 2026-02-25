import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:message-text',
      order: 9994,
      title: $t('chat.title'),
      authority: [PermissionCodes.ChatManagement, PermissionCodes.ChatView, PermissionCodes.ChatCreate],
    },
    name: 'Chat',
    path: '/chat',
    children: [
      {
        path: '/chat/conversations',
        name: 'ChatConversations',
        meta: {
          icon: 'mdi:forum',
          title: $t('chat.list'),
          authority: [PermissionCodes.ChatView],
        },
        component: () => import('#/views/chat/index.vue'),
      },
    ],
  },
];

export default routes;
