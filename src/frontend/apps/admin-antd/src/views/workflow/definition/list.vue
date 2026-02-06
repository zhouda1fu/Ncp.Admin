<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { WorkflowApi } from '#/api/system/workflow';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deleteDefinition,
  getDefinitionList,
  publishDefinition,
} from '#/api/system/workflow';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

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
    case 'publish': {
      onPublish(e.row);
      break;
    }
  }
}

function onEdit(row: WorkflowApi.WorkflowDefinition) {
  formDrawerApi.setData(row).open();
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

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}
</script>
<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid :table-title="$t('system.workflow.definition.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{
            $t('ui.actionTitle.create', [
              $t('system.workflow.definition.name'),
            ])
          }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
