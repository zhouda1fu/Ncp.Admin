<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { ProjectApi } from '#/api/system/project';

import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  activateProject,
  archiveProject,
  deleteProject,
  getProjectList,
} from '#/api/system/project';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const router = useRouter();

const [Grid, gridApi] = useVbenVxeGrid<ProjectApi.ProjectItem>({
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
            name: formValues.name,
            status: formValues.status,
            customerId: formValues.customerId,
            projectTypeId: formValues.projectTypeId,
            projectStatusOptionId: formValues.projectStatusOptionId,
            projectIndustryId: formValues.projectIndustryId,
          };
          const result = await getProjectList(params);
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

async function onActionClick(e: OnActionClickParams<ProjectApi.ProjectItem>) {
  if (e.code === 'edit') {
    router.push({ name: 'ProjectEdit', params: { id: e.row.id } });
    return;
  }
  if (e.code === 'archive') {
    try {
      await archiveProject(e.row.id);
      message.success($t('ui.actionMessage.operationSuccess'));
      gridApi.query();
    } catch (err) {
      message.error((err as Error)?.message ?? $t('ui.actionMessage.operationFailed'));
    }
  } else if (e.code === 'activate') {
    try {
      await activateProject(e.row.id);
      message.success($t('ui.actionMessage.operationSuccess'));
      gridApi.query();
    } catch (err) {
      message.error((err as Error)?.message ?? $t('ui.actionMessage.operationFailed'));
    }
  } else if (e.code === 'delete') {
    Modal.confirm({
      title: $t('ui.deleteConfirm.title'),
      content: $t('task.project.deleteConfirm', [e.row.name]),
      okText: $t('common.confirm'),
      okType: 'danger',
      cancelText: $t('common.cancel'),
      async onOk() {
        await deleteProject(e.row.id);
        message.success($t('ui.actionMessage.operationSuccess'));
        gridApi.query();
      },
    });
  }
}

function onCreate() {
  router.push({ name: 'ProjectCreate' });
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('task.project.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('task.project.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
