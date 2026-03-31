<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { OrderApi } from '#/api/system/order';

import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal, Tag } from 'ant-design-vue';

import { useAccessStore } from '@vben/stores';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteOrder, getOrderList, OrderStatusEnum, submitOrder } from '#/api/system/order';
import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const router = useRouter();
const accessStore = useAccessStore();
const getCanSubmitOrder = () =>
  accessStore.accessCodes?.includes(PermissionCodes.OrderSubmit) ?? false;

const hasPermission = (code: string) =>
  accessStore.accessCodes?.includes(code) ?? false;

const [Grid, gridApi] = useVbenVxeGrid<OrderApi.OrderQueryDto>({
  formOptions: {
    schema: useGridFormSchema() as VbenFormSchema[],
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(onActionClick, getCanSubmitOrder, hasPermission),
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

function onActionClick(e: OnActionClickParams<OrderApi.OrderQueryDto>) {
  if (e.code === 'view') onView(e.row);
  else if (e.code === 'edit') onEdit(e.row);
  else if (e.code === 'delete') onDelete(e.row);
  else if (e.code === 'resubmit') onResubmit(e.row);
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  router.push('/order/create');
}

function onView(row: OrderApi.OrderQueryDto) {
  router.push(`/order/${row.id}`);
}

function onEdit(row: OrderApi.OrderQueryDto) {
  router.push(`/order/${row.id}/edit`);
}

function onDelete(row: OrderApi.OrderQueryDto) {
  Modal.confirm({
    title: $t('order.confirmDelete'),
    onOk: async () => {
      await deleteOrder(row.id);
      message.success($t('common.success'));
      onRefresh();
    },
  });
}

function onResubmit(row: OrderApi.OrderQueryDto) {
  if (
    row.status !== OrderStatusEnum.Draft &&
    row.status !== OrderStatusEnum.Rejected
  )
    return;
  Modal.confirm({
    title: $t('order.submitApprovalConfirmTitle'),
    content: $t('order.submitApprovalConfirmContent'),
    onOk: async () => {
      await submitOrder(row.id);
      message.success($t('order.submitApprovalSuccess'));
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
      <template #status="{ row }">
        <Tag v-if="row.status === 0" color="default">{{ $t('order.statusDraft') }}</Tag>
        <Tag v-else-if="row.status === 1" color="processing">{{ $t('order.statusPendingAudit') }}</Tag>
        <Tag v-else-if="row.status === 2" color="success">{{ $t('order.statusOrdered') }}</Tag>
        <Tag v-else-if="row.status === 3" color="default">{{ $t('order.statusCompleted') }}</Tag>
        <Tag v-else-if="row.status === 4" color="error">{{ $t('order.statusRejected') }}</Tag>
        <Tag v-else-if="row.status === 5" color="warning">{{ $t('order.statusUnpaid') }}</Tag>
      </template>
      <template #operation="{ row }">
        <div class="flex items-center justify-end gap-2">
          <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="onView(row)">{{ $t('order.view') }}</Button>
          <template v-if="row.status === OrderStatusEnum.Draft || row.status === OrderStatusEnum.Rejected">
            <Button type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="onEdit(row)">{{ $t('order.edit') }}</Button>
            <Button v-if="getCanSubmitOrder()" type="link" size="small" class="p-0 min-w-0 h-auto leading-normal" @click="onResubmit(row)">{{ $t('order.submit') }}</Button>
          </template>
          <Button type="link" size="small" danger class="p-0 min-w-0 h-auto leading-normal" @click="onDelete(row)">{{ $t('order.delete') }}</Button>
        </div>
      </template>
    </Grid>
  </Page>
</template>
