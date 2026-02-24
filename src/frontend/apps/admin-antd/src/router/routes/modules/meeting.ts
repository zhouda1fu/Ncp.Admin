import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:desk',
      order: 9992,
      title: $t('meeting.title'),
      authority: [
        PermissionCodes.MeetingManagement,
        PermissionCodes.MeetingRoomView,
        PermissionCodes.MeetingRoomEdit,
        PermissionCodes.MeetingBookingView,
        PermissionCodes.MeetingBookingCreate,
      ],
    },
    name: 'Meeting',
    path: '/meeting',
    children: [
      {
        path: '/meeting/rooms',
        name: 'MeetingRooms',
        meta: {
          icon: 'mdi:door',
          title: $t('meeting.room.list'),
          authority: [PermissionCodes.MeetingRoomView],
        },
        component: () => import('#/views/meeting/rooms/list.vue'),
      },
      {
        path: '/meeting/bookings',
        name: 'MeetingBookings',
        meta: {
          icon: 'mdi:calendar-check',
          title: $t('meeting.booking.list'),
          authority: [PermissionCodes.MeetingBookingView],
        },
        component: () => import('#/views/meeting/bookings/list.vue'),
      },
    ],
  },
];

export default routes;
