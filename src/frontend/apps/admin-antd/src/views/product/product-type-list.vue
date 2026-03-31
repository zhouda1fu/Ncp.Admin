<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { ProductTypeItem } from '#/api/system/product';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteProductType, getProductTypeList } from '#/api/system/product';
import { $t } from '#/locales';

import { useProductTypeColumns } from './data';
import ProductTypeForm from './modules/product-type-form.vue';
import type { ProductTypeFormData } from './modules/product-type-form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: ProductTypeForm,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<ProductTypeItem>({
  gridOptions: {
    columns: useProductTypeColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async () => {
          const list = await getProductTypeList(true);
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
      zoom: true,
    },
  },
});

function onActionClick(e: OnActionClickParams<ProductTypeItem>) {
  switch (e.code) {
    case 'edit':
      formDrawerApi.setData<ProductTypeFormData>({ id: e.row.id }).open();
      break;
    case 'delete':
      onDelete(e.row);
      break;
  }
}

function onDelete(row: ProductTypeItem) {
  Modal.confirm({
    title: $t('product.confirmDeleteProductType'),
    onOk: async () => {
      await deleteProductType(row.id);
      message.success($t('common.success'));
      onRefresh();
    },
  });
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData<ProductTypeFormData>({}).open();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid :table-title="$t('product.productTypeList')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('product.productTypeAdd') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
