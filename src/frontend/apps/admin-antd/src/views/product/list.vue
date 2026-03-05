<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { ProductApi } from '#/api/system/product';

import { ref } from 'vue';
import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deleteProduct,
  getProductList,
} from '#/api/system/product';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onActionClick(e: OnActionClickParams<ProductApi.ProductItem>) {
  if (e.code === 'edit') {
    formDrawerApi.setData(e.row).open();
  } else if (e.code === 'delete') {
    Modal.confirm({
      title: $t('product.confirmDelete'),
      onOk: async () => {
        await deleteProduct(e.row.id);
        message.success($t('common.success'));
        onRefresh();
      },
    });
  }
}

const [Grid, gridApi] = useVbenVxeGrid<ProductApi.ProductItem>({
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
            keyword: formValues?.keyword,
          };
          const result = await getProductList(params);
          return { items: result.items ?? [], total: result.total ?? 0 };
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
    <Grid :table-title="$t('product.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('product.create') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
