<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { VehicleApi } from '#/api/system/vehicle';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getVehicleBookingList,
  createVehicleBooking,
  cancelVehicleBooking,
  completeVehicleBooking,
} from '#/api/system/vehicle';
import { $t } from '#/locales';

import { useColumns } from './data';
import BookingForm from './modules/booking-form.vue';

const [BookingDrawer, bookingDrawerApi] = useVbenDrawer({
  connectedComponent: BookingForm,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<VehicleApi.BookingItem>({
  formOptions: {
    schema: [
      { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'vehicleId', label: $t('vehicle.plateNumber') },
      {
        component: 'Select',
        componentProps: {
          allowClear: true,
          options: [
            { label: () => $t('vehicle.statusBooked'), value: 0 },
            { label: () => $t('vehicle.statusCancelled'), value: 1 },
            { label: () => $t('vehicle.statusCompleted'), value: 2 },
          ],
          class: 'w-full',
        },
        fieldName: 'status',
        label: $t('vehicle.bookingStatus'),
      },
    ],
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
            vehicleId: formValues.vehicleId,
            status: formValues.status,
          };
          const result = await getVehicleBookingList(params);
          return { items: result.items ?? [], total: result.total ?? 0 };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, search: true, zoom: true },
  },
});

function onActionClick(e: OnActionClickParams<VehicleApi.BookingItem>) {
  if (e.code === 'cancel') onCancel(e.row);
  else if (e.code === 'complete') onComplete(e.row);
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  bookingDrawerApi.setData({}).open();
}

async function onCancel(row: VehicleApi.BookingItem) {
  const key = 'vb_cancel';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await cancelVehicleBooking(row.id);
    message.success({ content: $t('common.success'), key });
    onRefresh();
  } finally {
    hide();
  }
}

async function onComplete(row: VehicleApi.BookingItem) {
  const key = 'vb_complete';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await completeVehicleBooking(row.id);
    message.success({ content: $t('common.success'), key });
    onRefresh();
  } finally {
    hide();
  }
}
</script>

<template>
  <Page auto-content-height>
    <BookingDrawer @success="onRefresh" />
    <Grid :table-title="$t('vehicle.bookingList')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('vehicle.bookingList') }} - {{ $t('vehicle.create') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
