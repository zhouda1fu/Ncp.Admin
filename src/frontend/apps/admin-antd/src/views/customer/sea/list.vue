<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { CustomerApi } from '#/api/system/customer';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getCustomerList,
  getCustomer,
  claimCustomerFromSea,
  voidCustomer,
  deleteCustomer,
} from '#/api/system/customer';
import { $t } from '#/locales';

import SeaForm from './modules/form.vue';
import SeaDetail from './modules/detail.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: SeaForm,
  destroyOnClose: true,
});

const [DetailDrawer, detailDrawerApi] = useVbenDrawer({
  connectedComponent: SeaDetail,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<CustomerApi.CustomerItem>({
  gridOptions: {
    columns: [
      { field: 'customerSourceName', title: $t('customer.customerSource'), width: 100 },
      { field: 'mainContactName', title: $t('customer.mainContactName'), width: 100 },
      { field: 'mainContactPhone', title: $t('customer.mainContactPhone'), width: 120 },
      {
        field: 'phoneRegion',
        title: $t('customer.phoneRegion'),
        width: 160,
        formatter: ({ row }: { row: CustomerApi.CustomerItem }) =>
          [row.phoneProvinceName, row.phoneCityName, row.phoneDistrictName]
            .filter(Boolean)
            .join(' ') || '-',
      },
      {
        field: 'projectRegion',
        title: $t('customer.projectRegion'),
        width: 160,
        formatter: ({ row }: { row: CustomerApi.CustomerItem }) =>
          [row.provinceName, row.cityName, row.districtName]
            .filter(Boolean)
            .join(' ') || '-',
      },
      {
        field: 'consultationContent',
        title: $t('customer.consultationContent'),
        minWidth: 140,
        showOverflow: 'tooltip',
      },
      { field: 'creatorName', title: $t('customer.creatorName'), width: 100 },
      { field: 'ownerName', title: $t('customer.claimUserName'), width: 100 },
      {
        field: 'claimedAt',
        title: $t('customer.claimTime'),
        width: 170,
        formatter: 'formatDateTime',
      },
      {
        field: 'claimStatus',
        title: $t('customer.claimStatus'),
        width: 100,
        slots: { default: 'claimStatus' },
      },
      {
        formatter: 'formatDateTime',
        field: 'createdAt',
        title: $t('customer.createdAt'),
        width: 170,
      },
      {
        align: 'right',
        cellRender: {
          attrs: { nameField: 'mainContactName', nameTitle: $t('customer.mainContactName'), onClick: onActionClick },
          name: 'CellOperation',
          options: [
            { code: 'view', text: $t('customer.view') },
            { code: 'claim', text: $t('customer.claim'), show: (row: CustomerApi.CustomerItem) => row.isInSea },
            { code: 'edit', text: $t('customer.edit'), show: (row: CustomerApi.CustomerItem) => row.isInSea },
            { code: 'void', text: $t('customer.void'), show: (row: CustomerApi.CustomerItem) => row.isInSea },
            { code: 'delete', text: $t('customer.delete'), danger: true, show: (row: CustomerApi.CustomerItem) => row.isInSea },
          ],
        },
        field: 'operation',
        fixed: 'right',
        headerAlign: 'center',
        showOverflow: false,
        title: $t('customer.operation'),
        width: 220,
      },
    ],
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async ({
          page,
        }: {
          page: { currentPage: number; pageSize: number };
        }) => {
          const params: Recordable<any> = {
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            isInSea: true,
          };
          const result = await getCustomerList(params);
          return { items: result.items ?? [], total: result.total ?? 0 };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, search: false, zoom: true },
  },
});

function onActionClick(e: OnActionClickParams<CustomerApi.CustomerItem>) {
  if (e.code === 'view') onView(e.row);
  else if (e.code === 'claim') onClaim(e.row);
  else if (e.code === 'edit') onEdit(e.row);
  else if (e.code === 'void') onVoid(e.row);
  else if (e.code === 'delete') onDelete(e.row);
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}

async function onView(row: CustomerApi.CustomerItem) {
  const detail = await getCustomer(row.id);
  detailDrawerApi.setData(detail).open();
}

async function onEdit(row: CustomerApi.CustomerItem) {
  const detail = await getCustomer(row.id);
  formDrawerApi.setData(detail).open();
}

async function onClaim(row: CustomerApi.CustomerItem) {
  const key = 'customer_claim';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await claimCustomerFromSea(row.id);
    message.success({ content: $t('common.success'), key });
    onRefresh();
  } finally {
    hide();
  }
}

function onVoid(row: CustomerApi.CustomerItem) {
  Modal.confirm({
    title: $t('customer.void'),
    content: `确定要作废客户「${row.mainContactName ?? '该客户'}」吗？`,
    okText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    onOk: async () => {
      const key = 'customer_void';
      const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
      try {
        await voidCustomer(row.id);
        message.success({ content: $t('common.success'), key });
        onRefresh();
      } finally {
        hide();
      }
    },
  });
}

async function onDelete(row: CustomerApi.CustomerItem) {
  const key = 'customer_delete';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await deleteCustomer(row.id);
    message.success({ content: $t('common.success'), key });
    onRefresh();
  } finally {
    hide();
  }
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <DetailDrawer />
    <Grid :table-title="$t('customer.sea')">
      <template #claimStatus="{ row }">
        <Tag :color="row.isInSea ? 'warning' : 'success'">
          {{ row.isInSea ? $t('customer.claimStatusUnclaimed') : $t('customer.claimStatusClaimed') }}
        </Tag>
      </template>
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('customer.seaCreate') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
