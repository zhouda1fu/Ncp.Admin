<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { AnnouncementApi } from '#/api/system/announcement';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getAnnouncementList,
  markAnnouncementRead,
  publishAnnouncement,
} from '#/api/system/announcement';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';
import ViewModal from './modules/view-modal.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [ViewModalRef, viewModalApi] = useVbenModal({
  connectedComponent: ViewModal,
});

const [Grid, gridApi] = useVbenVxeGrid<AnnouncementApi.AnnouncementItem>({
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
            title: formValues.title,
            status: formValues.status,
          };
          const result = await getAnnouncementList(params);
          return {
            items: result.items ?? [],
            total: result.total ?? 0,
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

function onActionClick(e: OnActionClickParams<AnnouncementApi.AnnouncementItem>) {
  switch (e.code) {
    case 'view':
      onView(e.row);
      break;
    case 'edit':
      onEdit(e.row);
      break;
    case 'publish':
      onPublish(e.row);
      break;
    case 'read':
      onMarkRead(e.row);
      break;
  }
}

function onView(row: AnnouncementApi.AnnouncementItem) {
  viewModalApi.setData(row).open();
}

function onEdit(row: AnnouncementApi.AnnouncementItem) {
  formDrawerApi.setData(row).open();
}

function onPublish(row: AnnouncementApi.AnnouncementItem) {
  const key = 'announcement_publish';
  const hide = message.loading({ content: $t('announcement.publishing'), duration: 0, key });
  publishAnnouncement(row.id)
    .then(() => {
      message.success({ content: $t('common.success'), key });
      onRefresh();
    })
    .finally(() => hide());
}

function onMarkRead(row: AnnouncementApi.AnnouncementItem) {
  markAnnouncementRead(row.id).then(() => onRefresh());
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
    <ViewModalRef @read="onRefresh" />
    <Grid :table-title="$t('announcement.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('announcement.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
