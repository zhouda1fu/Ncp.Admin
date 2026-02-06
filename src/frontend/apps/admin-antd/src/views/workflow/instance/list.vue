<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { WorkflowApi } from '#/api/system/workflow';

import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getInstanceList } from '#/api/system/workflow';
import { $t } from '#/locales';

const router = useRouter();

const statusLabels: Record<
  number,
  { color: string; label: string }
> = {
  0: {
    color: 'processing',
    label: $t('system.workflow.instance.statusRunning'),
  },
  1: {
    color: 'warning',
    label: $t('system.workflow.instance.statusSuspended'),
  },
  2: {
    color: 'success',
    label: $t('system.workflow.instance.statusCompleted'),
  },
  3: {
    color: 'error',
    label: $t('system.workflow.instance.statusRejected'),
  },
  4: {
    color: 'default',
    label: $t('system.workflow.instance.statusCancelled'),
  },
  5: {
    color: 'error',
    label: $t('system.workflow.instance.statusFaulted'),
  },
};

const [Grid] = useVbenVxeGrid<WorkflowApi.WorkflowInstance>({
  formOptions: {
    schema: [
      {
        component: 'Input',
        fieldName: 'title',
        label: $t('system.workflow.instance.flowTitle'),
      },
      {
        component: 'Input',
        fieldName: 'businessType',
        label: $t('system.workflow.instance.businessType'),
      },
      {
        component: 'Select',
        componentProps: {
          allowClear: true,
          options: Object.entries(statusLabels).map(([value, info]) => ({
            label: info.label,
            value: Number(value),
          })),
        },
        fieldName: 'status',
        label: $t('system.workflow.instance.status'),
      },
    ],
    submitOnChange: true,
  },
  gridOptions: {
    columns: [
      {
        field: 'title',
        title: $t('system.workflow.instance.flowTitle'),
        minWidth: 200,
      },
      {
        field: 'workflowDefinitionName',
        title: $t('system.workflow.instance.definitionName'),
        width: 150,
      },
      {
        field: 'initiatorName',
        title: $t('system.workflow.instance.initiator'),
        width: 120,
      },
      {
        field: 'status',
        title: $t('system.workflow.instance.status'),
        width: 100,
        slots: { default: 'status' },
      },
      {
        field: 'currentNodeName',
        title: $t('system.workflow.instance.currentNode'),
        width: 150,
      },
      {
        field: 'businessKey',
        title: $t('system.workflow.instance.businessKey'),
        width: 150,
      },
      {
        field: 'startedAt',
        formatter: 'formatDateTime',
        title: $t('system.workflow.instance.startedAt'),
        width: 180,
      },
      {
        field: 'completedAt',
        formatter: 'formatDateTime',
        title: $t('system.workflow.instance.completedAt'),
        width: 180,
      },
      {
        align: 'center',
        field: 'operation',
        fixed: 'right',
        title: $t('system.workflow.instance.operation'),
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
          const result = await getInstanceList({
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
      keyField: 'id',
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

function onViewDetail(row: WorkflowApi.WorkflowInstance) {
  router.push(`/workflow/instance/${row.id}`);
}
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.workflow.instance.list')">
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
