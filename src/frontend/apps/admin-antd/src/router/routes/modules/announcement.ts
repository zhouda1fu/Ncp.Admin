import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:bullhorn',
      order: 9996,
      title: $t('announcement.title'),
      authority: [
        PermissionCodes.AnnouncementManagement,
        PermissionCodes.AnnouncementView,
        PermissionCodes.AnnouncementCreate,
        PermissionCodes.AnnouncementEdit,
        PermissionCodes.AnnouncementPublish,
      ],
    },
    name: 'Announcement',
    path: '/announcement',
    children: [
      {
        path: '/announcement/list',
        name: 'AnnouncementList',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('announcement.list'),
          authority: [PermissionCodes.AnnouncementView],
        },
        component: () => import('#/views/announcement/list.vue'),
      },
    ],
  },
];

export default routes;
