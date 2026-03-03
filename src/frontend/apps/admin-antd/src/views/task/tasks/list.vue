<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { TaskApi } from '#/api/system/task';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteTask, getTaskList } from '#/api/system/task';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<TaskApi.TaskItem>({
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
          const params: Recordable<any> = {
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            projectId: formValues.projectId,
            status: formValues.status,
          };
          const result = await getTaskList(params);
          return {
            items: result.items ?? [],
            total: result.total ?? 0,
          };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

async function onActionClick(e: OnActionClickParams<TaskApi.TaskItem>) {
  if (e.code === 'edit') {
    formDrawerApi.setData(e.row).open();
  } else if (e.code === 'delete') {
    Modal.confirm({
      title: $t('ui.deleteConfirm.title'),
      content: $t('task.task.deleteConfirm', [e.row.title]),
      okText: $t('common.confirm'),
      okType: 'danger',
      cancelText: $t('common.cancel'),
      async onOk() {
        await deleteTask(e.row.id);
        message.success($t('ui.actionMessage.operationSuccess'));
        gridApi.query();
      },
    });
  }
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
    <Grid :table-title="$t('task.task.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('task.task.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
