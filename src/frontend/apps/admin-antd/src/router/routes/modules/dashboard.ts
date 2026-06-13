import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

import { workflowRoute } from './workflow';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'lucide:layout-dashboard',
      order: -1,
      title: $t('page.dashboard.title'),
    },
    name: 'Dashboard',
    path: '/dashboard',
    children: [
      {
        name: 'Analytics',
        path: '/analytics',
        component: () => import('#/views/dashboard/analytics/index.vue'),
        meta: {
          affixTab: true,
          icon: 'lucide:area-chart',
          order: 0,
          title: $t('page.dashboard.analytics'),
        },
      },
      workflowRoute,
      // {
      //   name: 'Workspace',
      //   path: '/workspace',
      //   component: () => import('#/views/dashboard/workspace/index.vue'),
      //   meta: {
      //     icon: 'carbon:workspace',
      //     title: $t('page.dashboard.workspace'),
      //   },
      // },
    ],
  },
];

export default routes;
