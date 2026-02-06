<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { WorkflowApi } from '#/api/system/workflow';

import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { approveTask, getMyPendingTasks, rejectTask } from '#/api/system/workflow';
import { $t } from '#/locales';

const router = useRouter();

const taskTypeLabels: Record<number, string> = {
  0: $t('system.workflow.task.taskTypeApproval'),
  1: $t('system.workflow.task.taskTypeNotification'),
  2: $t('system.workflow.task.taskTypeCarbonCopy'),
};

const [Grid, gridApi] = useVbenVxeGrid<WorkflowApi.MyPendingTask>({
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
        formatter: ({ row }: { row: WorkflowApi.MyPendingTask }) =>
          taskTypeLabels[row.taskType] ?? '',
      },
      {
        field: 'createdAt',
        formatter: 'formatDateTime',
        title: $t('system.workflow.task.createdAt'),
        width: 180,
      },
      {
        align: 'center',
        field: 'operation',
        fixed: 'right',
        title: $t('system.workflow.task.operation'),
        width: 240,
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
          const result = await getMyPendingTasks({
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

function onApprove(row: WorkflowApi.MyPendingTask) {
  Modal.confirm({
    content: '确认审批通过该任务吗？',
    title: '审批确认',
    async onOk() {
      await approveTask({
        workflowInstanceId: row.workflowInstanceId,
        taskId: row.taskId,
        comment: '同意',
      });
      message.success('审批通过');
      gridApi.query();
    },
  });
}

function onReject(row: WorkflowApi.MyPendingTask) {
  Modal.confirm({
    content: '确认驳回该任务吗？驳回后流程将终止。',
    title: '驳回确认',
    async onOk() {
      await rejectTask({
        workflowInstanceId: row.workflowInstanceId,
        taskId: row.taskId,
        comment: '不同意',
      });
      message.success('已驳回');
      gridApi.query();
    },
  });
}

function onViewDetail(row: WorkflowApi.MyPendingTask) {
  router.push(`/workflow/instance/${row.workflowInstanceId}`);
}
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.workflow.task.pendingTitle')">
      <template #action="{ row }">
        <Button size="small" type="primary" @click="onApprove(row)">
          {{ $t('system.workflow.task.approve') }}
        </Button>
        <Button danger size="small" class="ml-2" @click="onReject(row)">
          {{ $t('system.workflow.task.reject') }}
        </Button>
        <Button size="small" class="ml-2" @click="onViewDetail(row)">
          {{ $t('system.workflow.instance.detail') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
