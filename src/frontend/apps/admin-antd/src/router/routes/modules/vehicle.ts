import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:car',
      order: 9989,
      title: $t('vehicle.title'),
      authority: [
        PermissionCodes.VehicleManagement,
        PermissionCodes.VehicleView,
        PermissionCodes.VehicleBookingView,
      ],
    },
    name: 'Vehicle',
    path: '/vehicle',
    children: [
      {
        path: '/vehicle/list',
        name: 'VehicleList',
        meta: {
          icon: 'mdi:format-list-bulleted',
          title: $t('vehicle.list'),
          authority: [PermissionCodes.VehicleView],
        },
        component: () => import('#/views/vehicle/list.vue'),
      },
      {
        path: '/vehicle/bookings',
        name: 'VehicleBookingList',
        meta: {
          icon: 'mdi:calendar-check',
          title: $t('vehicle.bookingList'),
          authority: [PermissionCodes.VehicleBookingView],
        },
        component: () => import('#/views/vehicle/bookings/list.vue'),
      },
    ],
  },
];

export default routes;
