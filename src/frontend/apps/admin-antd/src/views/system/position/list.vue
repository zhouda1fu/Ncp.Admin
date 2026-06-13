<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { SystemPositionApi } from '#/api/system/position';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deletePosition,
  getPositionList,
} from '#/api/system/position';
import { $t } from '#/locales';
import { handleVxeCellDblclick } from '#/utils/vxe-row-navigation';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<SystemPositionApi.PositionListItem>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridClass: 'w-full min-w-0 [&_.vxe-table]:w-full',
    gridEvents: {
    'cell-dblclick': (event: any) => handleVxeCellDblclick(event, onRowDblclick),
  } as any,

  gridOptions: {
    columns: useColumns(onActionClick),
    customConfig: {
      visibleMethod({ column }: any) {
        const field = (column as { field?: string; type?: string }).field;
        const type = (column as { field?: string; type?: string }).type;
        if (type) return false;
        return ['name', 'code', 'sortOrder', 'status', 'createdAt', 'operation'].includes(
          field ?? '',
        );
      },
    },
    height: 'auto',
    id: 'SystemPositionListGrid-v2',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          const result = await getPositionList({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
          });
          return {
            items: result.items,
            total: result.total,
          };
        },
      },
    },
    rowClassName: () => 'vxe-row-clickable',

    rowConfig: {
      keyField: 'id',
    },
    toolbarConfig: {
      custom: false,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

function onActionClick(e: OnActionClickParams<SystemPositionApi.PositionListItem>) {
  switch (e.code) {
    case 'delete':
      onDelete(e.row);
      break;
    case 'edit':
      onEdit(e.row);
      break;
  }
}

function onEdit(row: SystemPositionApi.PositionListItem) {
  formDrawerApi.setData(row).open();
}

function onDelete(row: SystemPositionApi.PositionListItem) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  deletePosition(String(row.id))
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

function onRefresh() {
  gridApi.query();
}

function onRowDblclick(row: SystemPositionApi.PositionListItem) {
  onEdit(row);
}

function onCreate() {
  formDrawerApi.setData({}).open();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid :table-title="$t('system.position.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('system.position.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
