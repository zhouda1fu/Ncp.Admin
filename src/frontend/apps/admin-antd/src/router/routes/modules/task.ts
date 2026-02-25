import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:clipboard-list',
      order: 9991,
      title: $t('task.title'),
      authority: [
        PermissionCodes.TaskManagement,
        PermissionCodes.ProjectView,
        PermissionCodes.ProjectCreate,
        PermissionCodes.ProjectEdit,
        PermissionCodes.TaskView,
        PermissionCodes.TaskCreate,
        PermissionCodes.TaskEdit,
      ],
    },
    name: 'Task',
    path: '/task',
    children: [
      {
        path: '/task/projects',
        name: 'TaskProjects',
        meta: {
          icon: 'mdi:folder',
          title: $t('task.project.list'),
          authority: [PermissionCodes.ProjectView],
        },
        component: () => import('#/views/task/projects/list.vue'),
      },
      {
        path: '/task/tasks',
        name: 'TaskTasks',
        meta: {
          icon: 'mdi:format-list-checks',
          title: $t('task.task.list'),
          authority: [PermissionCodes.TaskView],
        },
        component: () => import('#/views/task/tasks/list.vue'),
      },
    ],
  },
];

export default routes;
