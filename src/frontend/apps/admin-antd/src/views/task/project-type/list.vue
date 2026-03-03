<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { ProjectTypeApi } from '#/api/system/project-type';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button } from 'ant-design-vue';
import { Modal, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getProjectTypeList,
  deleteProjectType,
} from '#/api/system/project-type';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<ProjectTypeApi.ProjectTypeItem>({
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
          _params: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          const list = await getProjectTypeList();
          let items = list;
          if (formValues?.name) {
            const kw = String(formValues.name).trim().toLowerCase();
            items = list.filter((x) => x.name.toLowerCase().includes(kw));
          }
          return { items, total: items.length };
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

function onActionClick(e: OnActionClickParams<ProjectTypeApi.ProjectTypeItem>) {
  if (e.code === 'edit') {
    formDrawerApi.setData(e.row).open();
  } else if (e.code === 'delete') {
    Modal.confirm({
      title: $t('ui.actionMessage.deleteConfirm', [e.row.name]),
      onOk: async () => {
        await deleteProjectType(e.row.id);
        message.success($t('ui.actionMessage.deleteSuccess', [e.row.name]));
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
    <Grid :table-title="$t('task.projectType.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('task.projectType.title')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
