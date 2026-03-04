<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { OrderApi } from '#/api/system/order';

import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteOrder, getOrderList } from '#/api/system/order';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const router = useRouter();

const [Grid, gridApi] = useVbenVxeGrid<OrderApi.OrderItem>({
  formOptions: {
    schema: useGridFormSchema() as VbenFormSchema[],
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
            orderNumber: formValues.orderNumber,
            type: formValues.type,
            status: formValues.status,
          };
          const result = await getOrderList(params);
          return { items: result.items ?? [], total: result.total ?? 0 };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, search: true, zoom: true },
  },
});

function onActionClick(e: OnActionClickParams<OrderApi.OrderItem>) {
  if (e.code === 'view') onView(e.row);
  else if (e.code === 'edit') onEdit(e.row);
  else if (e.code === 'delete') onDelete(e.row);
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  router.push('/order/create');
}

function onView(row: OrderApi.OrderItem) {
  router.push(`/order/${row.id}`);
}

function onEdit(row: OrderApi.OrderItem) {
  router.push(`/order/${row.id}/edit`);
}

function onDelete(row: OrderApi.OrderItem) {
  Modal.confirm({
    title: $t('order.confirmDelete'),
    onOk: async () => {
      await deleteOrder(row.id);
      message.success($t('common.success'));
      onRefresh();
    },
  });
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('order.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('order.create') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
