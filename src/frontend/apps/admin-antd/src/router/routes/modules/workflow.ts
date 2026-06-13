import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';
import { listReturnListRouteMeta } from '#/router/list-return-route-meta';

/** 流程侧栏入口：拥有任一子菜单相关权限即显示；WorkflowManagement 为权限树分组码，通常不会单独勾选。 */
const workflowMenuAccess = [
  PermissionCodes.WorkflowManagement,
  PermissionCodes.WorkflowInstanceView,
  PermissionCodes.WorkflowDefinitionView,
  PermissionCodes.WorkflowMonitor,
  PermissionCodes.WorkflowTaskApprove,
];

export const workflowRoute: RouteRecordRaw = {
  meta: {
    icon: 'mdi:workflow',
    order: 1,
    title: $t('system.workflow.title'),
    authority: workflowMenuAccess,
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
          ...listReturnListRouteMeta,
          authority: [PermissionCodes.WorkflowInstanceView],
        },
        component: () => import('#/views/workflow/task/pending.vue'),
      },
      {
        path: '/workflow/completed',
        name: 'WorkflowCompleted',
        meta: {
          icon: 'mdi:clipboard-check',
          title: $t('system.workflow.task.completedTitle'),
          ...listReturnListRouteMeta,
          authority: [PermissionCodes.WorkflowInstanceView],
        },
        component: () => import('#/views/workflow/task/completed.vue'),
      },
      {
        path: '/workflow/my-workflows',
        name: 'WorkflowMyWorkflows',
        meta: {
          icon: 'mdi:clipboard-account',
          title: $t('system.workflow.task.myWorkflows'),
          ...listReturnListRouteMeta,
          authority: [PermissionCodes.WorkflowInstanceView],
        },
        component: () => import('#/views/workflow/instance/my-started.vue'),
      },
      {
        path: '/workflow/definitions',
        name: 'WorkflowDefinitions',
        meta: {
          icon: 'mdi:file-tree',
          title: $t('system.workflow.definition.title'),
          ...listReturnListRouteMeta,
          authority: [PermissionCodes.WorkflowDefinitionView],
        },
        component: () => import('#/views/workflow/definition/list.vue'),
      },
      {
        path: '/workflow/designer/:id?',
        name: 'WorkflowDesigner',
        meta: {
          hideInMenu: true,
          title: '流程设计器',
          activePath: '/workflow/definitions',
        },
        component: () => import('#/views/workflow/designer/index.vue'),
      },
      {
        path: '/workflow/monitor',
        name: 'WorkflowMonitor',
        meta: {
          icon: 'mdi:monitor-dashboard',
          title: $t('system.workflow.instance.title'),
          ...listReturnListRouteMeta,
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
          activePath: '/workflow/pending',
        },
        component: () => import('#/views/workflow/instance/detail.vue'),
      },
    ],
};

const routes: RouteRecordRaw[] = [];

export default routes;
