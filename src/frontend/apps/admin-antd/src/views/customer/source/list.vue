<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { CustomerSourceApi } from '#/api/system/customerSource';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getCustomerSourceList } from '#/api/system/customerSource';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<CustomerSourceApi.CustomerSourceItem>({
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
          const list = await getCustomerSourceList();
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

function onActionClick(e: OnActionClickParams<CustomerSourceApi.CustomerSourceItem>) {
  if (e.code === 'edit') {
    formDrawerApi.setData(e.row).open();
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
    <Grid :table-title="$t('customer.sourceList')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('customer.customerSource')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
