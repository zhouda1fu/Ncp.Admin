<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { SupplierItem } from '#/api/system/product';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteSupplier, getSupplierList } from '#/api/system/product';
import { $t } from '#/locales';

import { useSupplierColumns, useSupplierGridFormSchema } from './data';
import SupplierForm from './modules/supplier-form.vue';
import type { SupplierFormData } from './modules/supplier-form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: SupplierForm,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<SupplierItem>({
  formOptions: {
    schema: useSupplierGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: useSupplierColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          _params: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<{ keyword?: string }>,
        ) => {
          const keyword = formValues?.keyword as string | undefined;
          const list = await getSupplierList(keyword);
          const items = Array.isArray(list) ? list : [];
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

function onActionClick(e: OnActionClickParams<SupplierItem>) {
  switch (e.code) {
    case 'edit':
      formDrawerApi.setData<SupplierFormData>({ id: e.row.id }).open();
      break;
    case 'delete':
      onDelete(e.row);
      break;
  }
}

function onDelete(row: SupplierItem) {
  Modal.confirm({
    title: $t('product.confirmDeleteSupplier'),
    onOk: async () => {
      await deleteSupplier(row.id);
      message.success($t('common.success'));
      onRefresh();
    },
  });
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData<SupplierFormData>({}).open();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid :table-title="$t('product.supplierList')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('product.supplierCreate') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
