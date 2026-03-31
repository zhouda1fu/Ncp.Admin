<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { WorkflowApi } from '#/api/system/workflow';

import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deleteDefinition,
  getDefinitionList,
  createDefinitionNewVersion,
  publishDefinition,
} from '#/api/system/workflow';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const router = useRouter();

const [Grid, gridApi] = useVbenVxeGrid<WorkflowApi.WorkflowDefinition>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          const result = await getDefinitionList({
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

function onActionClick(
  e: OnActionClickParams<WorkflowApi.WorkflowDefinition>,
) {
  switch (e.code) {
    case 'delete': {
      onDelete(e.row);
      break;
    }
    case 'edit': {
      onEdit(e.row);
      break;
    }
    case 'view': {
      onView(e.row);
      break;
    }
    case 'newVersion': {
      onCreateNewVersion(e.row);
      break;
    }
    case 'publish': {
      onPublish(e.row);
      break;
    }
  }
}

function onView(row: WorkflowApi.WorkflowDefinition) {
  router.push({ path: `/workflow/designer/${row.id}`, query: { view: '1' } });
}

function onEdit(row: WorkflowApi.WorkflowDefinition) {
  if (row.status === 1) {
    message.warning($t('system.workflow.definition.cannotEditPublished'));
    return;
  }
  router.push(`/workflow/designer/${row.id}`);
}

function onDelete(row: WorkflowApi.WorkflowDefinition) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  deleteDefinition(row.id)
    .then(() => {
      message.success({
        content: $t('ui.actionMessage.deleteSuccess', [row.name]),
        key: 'action_process_msg',
      });
      onRefresh();
    })
    .catch(() => {
      hideLoading();
    });
}

function onPublish(row: WorkflowApi.WorkflowDefinition) {
  Modal.confirm({
    content: `确认要发布流程定义「${row.name}」吗？发布后将不能修改。`,
    title: '发布确认',
    async onOk() {
      await publishDefinition(row.id);
      message.success(`流程定义「${row.name}」已发布`);
      onRefresh();
    },
  });
}

function onCreateNewVersion(row: WorkflowApi.WorkflowDefinition) {
  Modal.confirm({
    content: `确认要基于流程定义「${row.name}」创建新版本吗？`,
    title: '创建新版本确认',
    async onOk() {
      const newId = await createDefinitionNewVersion(row.id);
      message.success(`已基于「${row.name}」创建新版本`);
      router.push(`/workflow/designer/${newId}`);
    },
  });
}

function onRefresh() {
  gridApi.query();
}

function onDesigner() {
  router.push('/workflow/designer');
}
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.workflow.definition.list')">
      <template #toolbar-tools>
        <Button
          type="primary"
          class="inline-flex items-center gap-1"
          @click="onDesigner"
        >
          <Plus class="size-5 shrink-0" />
          流程设计器
        </Button>
      </template>
    </Grid>
  </Page>
</template>
