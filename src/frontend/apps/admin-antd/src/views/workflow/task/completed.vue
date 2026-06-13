<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { WorkflowApi } from '#/api/system/workflow';

import { onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useListReturnState } from '#/composables/use-list-return-state';
import { getMyCompletedTasks } from '#/api/system/workflow';
import { $t } from '#/locales';
import { handleVxeCellDblclick } from '#/utils/vxe-row-navigation';

const LIST_PATH = '/workflow/completed';
const SEARCH_KEYS = ['title'] as const;

const router = useRouter();
const route = useRoute();

const { shouldDeferGridAutoLoad, pagerConfig, trackPage, buildReturnQuery, restoreOnMount } =
  useListReturnState({
    route,
    listPath: LIST_PATH,
    searchKeys: SEARCH_KEYS,
  });

const taskTypeLabels: Record<number, string> = {
  0: $t('system.workflow.task.taskTypeApproval'),
  1: $t('system.workflow.task.taskTypeNotification'),
  2: $t('system.workflow.task.taskTypeCarbonCopy'),
};

const statusLabels: Record<
  number,
  { color: string; label: string }
> = {
  0: {
    color: 'processing',
    label: $t('system.workflow.task.statusPending'),
  },
  1: {
    color: 'success',
    label: $t('system.workflow.task.statusApproved'),
  },
  2: { color: 'error', label: $t('system.workflow.task.statusRejected') },
  3: {
    color: 'warning',
    label: $t('system.workflow.task.statusTransferred'),
  },
  4: {
    color: 'default',
    label: $t('system.workflow.task.statusCancelled'),
  },
  5: {
    color: 'warning',
    label: $t('system.workflow.task.statusDelegated'),
  },
  6: {
    color: 'success',
    label: $t('system.workflow.task.statusRead'),
  },
  7: {
    color: 'success',
    label: $t('system.workflow.task.statusTaskCompleted'),
  },
  8: {
    color: 'default',
    label: $t('system.workflow.task.statusAutoSkipped'),
  },
};

const [Grid, gridApi] = useVbenVxeGrid<WorkflowApi.MyCompletedTask>({
  formOptions: {
    schema: [
      {
        component: 'Input',
        fieldName: 'title',
        label: $t('system.workflow.task.flowTitle'),
      },
    ],
    submitOnChange: true,
  },
  gridEvents: {
    'cell-dblclick': (event: any) => handleVxeCellDblclick(event, onViewDetail),
  } as any,
  gridOptions: {
    columns: [
      {
        field: 'workflowTitle',
        title: $t('system.workflow.task.flowTitle'),
        minWidth: 200,
      },
      {
        field: 'workflowDefinitionName',
        title: $t('system.workflow.task.definitionName'),
        width: 150,
      },
      {
        field: 'initiatorName',
        title: $t('system.workflow.task.initiator'),
        width: 120,
      },
      {
        field: 'nodeName',
        title: $t('system.workflow.task.nodeName'),
        width: 150,
      },
      {
        field: 'taskType',
        title: $t('system.workflow.task.taskType'),
        width: 100,
        formatter: ({ row }: { row: WorkflowApi.MyCompletedTask }) =>
          taskTypeLabels[row.taskType] ?? '',
      },
      {
        field: 'status',
        title: $t('system.workflow.task.status'),
        width: 100,
        slots: { default: 'status' },
      },
      {
        field: 'comment',
        title: $t('system.workflow.task.comment'),
        width: 150,
      },
      {
        field: 'completedAt',
        formatter: 'formatDateTime',
        title: $t('system.workflow.task.completedAt'),
        width: 180,
      },
      { field: '_flex', minWidth: 1, title: '' },
      {
        align: 'center',
        field: 'operation',
        fixed: 'right',
      showOverflow: false,
        title: $t('system.workflow.task.operation'),
        width: 100,
        slots: { default: 'action' },
      },
    ],
    height: 'auto',
    keepSource: true,
    pagerConfig,
    proxyConfig: {
      autoLoad: !shouldDeferGridAutoLoad,
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          trackPage(page);
          const result = await getMyCompletedTasks({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            countTotal: true,
            ...formValues,
          });
          return {
            items: result.items,
            total: result.total,
          };
        },
      },
    },
    rowConfig: {
      keyField: 'taskId',
    },
    rowClassName: () => 'vxe-row-clickable',
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

async function onViewDetail(row: WorkflowApi.MyCompletedTask) {
  void router.push({
    path: `/workflow/instance/${row.workflowInstanceId}`,
    query: await buildReturnQuery(gridApi),
  });
}

onMounted(async () => {
  await restoreOnMount(gridApi);
});
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.workflow.task.completedTitle')">
      <template #status="{ row }">
        <Tag :color="statusLabels[row.status]?.color ?? 'default'">
          {{ statusLabels[row.status]?.label ?? '' }}
        </Tag>
      </template>
      <template #action="{ row }">
        <Button size="small" @click="onViewDetail(row)">
          {{ $t('system.workflow.instance.detail') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
