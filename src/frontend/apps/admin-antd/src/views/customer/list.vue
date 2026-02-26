<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { CustomerApi } from '#/api/system/customer';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getCustomerList,
  getCustomer,
  releaseCustomerToSea,
} from '#/api/system/customer';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<CustomerApi.CustomerItem>({
  formOptions: { schema: useGridFormSchema(), submitOnChange: true },
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
            fullName: formValues.fullName,
            customerSource: formValues.customerSource,
            isInSea: formValues.isInSea,
          };
          const result = await getCustomerList(params);
          return { items: result.items ?? [], total: result.total ?? 0 };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, search: true, zoom: true },
  },
});

function onActionClick(e: OnActionClickParams<CustomerApi.CustomerItem>) {
  if (e.code === 'edit') onEdit(e.row);
  else if (e.code === 'releaseToSea') onReleaseToSea(e.row);
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}

async function onEdit(row: CustomerApi.CustomerItem) {
  const detail = await getCustomer(row.id);
  formDrawerApi.setData(detail).open();
}

async function onReleaseToSea(row: CustomerApi.CustomerItem) {
  const key = 'customer_release';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await releaseCustomerToSea(row.id);
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
    <Grid :table-title="$t('customer.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('customer.create') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
