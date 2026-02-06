import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:workflow',
      order: 9996,
      title: $t('system.workflow.title'),
      authority: [PermissionCodes.WorkflowManagement],
    },
    name: 'Workflow',
    path: '/workflow',
    children: [
      {
        path: '/workflow/pending',
        name: 'WorkflowPending',
        meta: {
          icon: 'mdi:clipboard-text-clock',
          title: $t('system.workflow.task.pendingTitle'),
        },
        component: () => import('#/views/workflow/task/pending.vue'),
      },
      {
        path: '/workflow/completed',
        name: 'WorkflowCompleted',
        meta: {
          icon: 'mdi:clipboard-check',
          title: $t('system.workflow.task.completedTitle'),
        },
        component: () => import('#/views/workflow/task/completed.vue'),
      },
      {
        path: '/workflow/my-workflows',
        name: 'WorkflowMyWorkflows',
        meta: {
          icon: 'mdi:clipboard-account',
          title: $t('system.workflow.task.myWorkflows'),
        },
        component: () => import('#/views/workflow/instance/my-workflows.vue'),
      },
      {
        path: '/workflow/definitions',
        name: 'WorkflowDefinitions',
        meta: {
          icon: 'mdi:file-tree',
          title: $t('system.workflow.definition.title'),
          authority: [PermissionCodes.WorkflowDefinitionView],
        },
        component: () => import('#/views/workflow/definition/list.vue'),
      },
      {
        path: '/workflow/monitor',
        name: 'WorkflowMonitor',
        meta: {
          icon: 'mdi:monitor-dashboard',
          title: $t('system.workflow.instance.title'),
          authority: [PermissionCodes.WorkflowMonitor],
        },
        component: () => import('#/views/workflow/instance/list.vue'),
      },
      {
        path: '/workflow/instance/:id',
        name: 'WorkflowInstanceDetail',
        meta: {
          hideInMenu: true,
          title: $t('system.workflow.instance.detail'),
        },
        component: () => import('#/views/workflow/instance/detail.vue'),
      },
    ],
  },
];

export default routes;
