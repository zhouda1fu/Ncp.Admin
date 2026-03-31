<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { ProductApi } from '#/api/system/product';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { useRouter } from 'vue-router';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteProduct, getProductList } from '#/api/system/product';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const router = useRouter();

function onActionClick(e: OnActionClickParams<ProductApi.ProductItem>) {
  if (e.code === 'edit') {
    router.push(`/product/form/${e.row.id}`);
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
  router.push('/product/form');
}
</script>

<template>
  <Page auto-content-height>
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
