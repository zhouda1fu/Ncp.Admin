<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { WorkflowApi } from '#/api/system/workflow';

import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getMyCompletedTasks } from '#/api/system/workflow';
import { $t } from '#/locales';

const router = useRouter();

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
};

const [Grid] = useVbenVxeGrid<WorkflowApi.MyCompletedTask>({
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
      {
        align: 'center',
        field: 'operation',
        fixed: 'right',
        title: $t('system.workflow.task.operation'),
        width: 100,
        slots: { default: 'action' },
      },
    ],
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
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
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

function onViewDetail(row: WorkflowApi.MyCompletedTask) {
  router.push(`/workflow/instance/${row.workflowInstanceId}`);
}
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
