<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { WorkflowApi } from '#/api/system/workflow';

import { onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useListReturnState } from '#/composables/use-list-return-state';
import { collectRouteStringParams, parseNumberQuery, readStringQuery } from '#/utils/list-return-state';
import { getMyWorkflows } from '#/api/system/workflow';
import { $t } from '#/locales';
import { handleVxeCellDblclick } from '#/utils/vxe-row-navigation';

const LIST_PATH = '/workflow/my-workflows';
const SEARCH_KEYS = ['title', 'businessType', 'status'] as const;

const router = useRouter();
const route = useRoute();

const { shouldDeferGridAutoLoad, pagerConfig, trackPage, buildReturnQuery, restoreOnMount } =
  useListReturnState({
    route,
    listPath: LIST_PATH,
    searchKeys: SEARCH_KEYS,
    parseRouteToSearchValues: (query) => {
      const values: Record<string, unknown> = collectRouteStringParams(query, SEARCH_KEYS);
      const statusNum = parseNumberQuery(readStringQuery(query, 'status'));
      if (statusNum !== undefined) values.status = statusNum;
      return values;
    },
  });

const statusLabels: Record<number, { color: string; label: string }> = {
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

const [Grid, gridApi] = useVbenVxeGrid<WorkflowApi.WorkflowInstance>({
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
  gridEvents: {
    'cell-dblclick': (event: any) => handleVxeCellDblclick(event, onViewDetail),
  } as any,
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
        field: 'businessType',
        title: $t('system.workflow.instance.businessType'),
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
      { field: '_flex', minWidth: 1, title: '' },
      {
        align: 'center',
        field: 'operation',
        fixed: 'right',
      showOverflow: false,
        title: $t('system.workflow.instance.operation'),
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
          const result = await getMyWorkflows({
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

async function onViewDetail(row: WorkflowApi.WorkflowInstance) {
  void router.push({
    path: `/workflow/instance/${row.id}`,
    query: await buildReturnQuery(gridApi),
  });
}

onMounted(async () => {
  await restoreOnMount(gridApi);
});
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.workflow.task.myWorkflows')">
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
