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
        PermissionCodes.ProjectTypeView,
        PermissionCodes.ProjectTypeCreate,
        PermissionCodes.ProjectTypeEdit,
        PermissionCodes.ProjectStatusOptionView,
        PermissionCodes.ProjectStatusOptionCreate,
        PermissionCodes.ProjectStatusOptionEdit,
        PermissionCodes.ProjectIndustryView,
        PermissionCodes.ProjectIndustryCreate,
        PermissionCodes.ProjectIndustryEdit,
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
        path: '/task/projects/create',
        name: 'TaskProjectCreate',
        meta: {
          activePath: '/task/projects',
          hideInMenu: true,
          title: $t('ui.actionTitle.create', [$t('task.project.name')]),
        },
        component: () => import('#/views/task/projects/form.vue'),
      },
      {
        path: '/task/projects/:id/edit',
        name: 'TaskProjectEdit',
        meta: {
          activePath: '/task/projects',
          hideInMenu: true,
          title: $t('ui.actionTitle.edit', [$t('task.project.name')]),
        },
        component: () => import('#/views/task/projects/form.vue'),
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
      {
        path: '/task/project-type',
        name: 'TaskProjectTypeList',
        meta: {
          icon: 'mdi:tag-outline',
          title: $t('task.projectType.list'),
          authority: [PermissionCodes.ProjectTypeView],
        },
        component: () => import('#/views/task/project-type/list.vue'),
      },
      {
        path: '/task/project-status',
        name: 'TaskProjectStatusList',
        meta: {
          icon: 'mdi:state-machine',
          title: $t('task.projectStatus.list'),
          authority: [PermissionCodes.ProjectStatusOptionView],
        },
        component: () => import('#/views/task/project-status/list.vue'),
      },
      {
        path: '/task/project-industry',
        name: 'TaskProjectIndustryList',
        meta: {
          icon: 'mdi:briefcase-outline',
          title: $t('task.projectIndustry.list'),
          authority: [PermissionCodes.ProjectIndustryView],
        },
        component: () => import('#/views/task/project-industry/list.vue'),
      },
    ],
  },
];

export default routes;
