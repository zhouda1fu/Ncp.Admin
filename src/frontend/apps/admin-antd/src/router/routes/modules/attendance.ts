import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:clock-outline',
      order: 9994,
      title: $t('attendance.title'),
      authority: [
        PermissionCodes.AttendanceManagement,
        PermissionCodes.AttendanceRecordView,
        PermissionCodes.AttendanceCheckIn,
        PermissionCodes.ScheduleView,
        PermissionCodes.ScheduleEdit,
      ],
    },
    name: 'Attendance',
    path: '/attendance',
    children: [
      {
        path: '/attendance/records',
        name: 'AttendanceRecords',
        meta: {
          icon: 'mdi:clipboard-list',
          title: $t('attendance.record.list'),
          authority: [PermissionCodes.AttendanceRecordView],
        },
        component: () => import('#/views/attendance/records/list.vue'),
      },
      {
        path: '/attendance/schedules',
        name: 'AttendanceSchedules',
        meta: {
          icon: 'mdi:calendar-clock',
          title: $t('attendance.schedule.list'),
          authority: [PermissionCodes.ScheduleView],
        },
        component: () => import('#/views/attendance/schedules/list.vue'),
      },
    ],
  },
];

export default routes;
