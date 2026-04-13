import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  // 项目管理
  {
    meta: {
      icon: 'mdi:folder',
      order: 9990,
      title: $t('task.project.managementTitle'),
      authority: [
        PermissionCodes.ProjectManagement,
        PermissionCodes.ProjectView,
        PermissionCodes.ProjectCreate,
        PermissionCodes.ProjectEdit,
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
    name: 'Project',
    path: '/project',
    children: [
      {
        path: '/project/list',
        name: 'ProjectList',
        meta: {
          icon: 'mdi:folder',
          title: $t('task.project.list'),
          authority: [PermissionCodes.ProjectView],
        },
        component: () => import('#/views/task/projects/list.vue'),
      },
      {
        path: '/project/create',
        name: 'ProjectCreate',
        meta: {
          activePath: '/project/list',
          hideInMenu: true,
          title: $t('ui.actionTitle.create', [$t('task.project.name')]),
        },
        component: () => import('#/views/task/projects/form.vue'),
      },
      {
        path: '/project/:id/edit',
        name: 'ProjectEdit',
        meta: {
          activePath: '/project/list',
          hideInMenu: true,
          title: $t('ui.actionTitle.edit', [$t('task.project.name')]),
        },
        component: () => import('#/views/task/projects/form.vue'),
      },
      {
        path: '/project/type',
        name: 'ProjectTypeList',
        meta: {
          icon: 'mdi:tag-outline',
          title: $t('task.projectType.list'),
          authority: [PermissionCodes.ProjectTypeView],
        },
        component: () => import('#/views/task/project-type/list.vue'),
      },
      {
        path: '/project/status',
        name: 'ProjectStatusList',
        meta: {
          icon: 'mdi:state-machine',
          title: $t('task.projectStatus.list'),
          authority: [PermissionCodes.ProjectStatusOptionView],
        },
        component: () => import('#/views/task/project-status/list.vue'),
      },
      {
        path: '/project/industry',
        name: 'ProjectIndustryList',
        meta: {
          icon: 'mdi:briefcase-outline',
          title: $t('task.projectIndustry.list'),
          authority: [PermissionCodes.ProjectIndustryView],
        },
        component: () => import('#/views/task/project-industry/list.vue'),
      },
    ],
  },
  // 任务管理
  {
    meta: {
      icon: 'mdi:clipboard-list',
      order: 9991,
      title: $t('task.title'),
      authority: [
        PermissionCodes.TaskManagement,
        PermissionCodes.TaskView,
        PermissionCodes.TaskCreate,
        PermissionCodes.TaskEdit,
      ],
    },
    name: 'Task',
    path: '/task',
    children: [
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
