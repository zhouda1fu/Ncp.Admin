<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { AssetApi } from '#/api/system/asset';

import { Page } from '@vben/common-ui';

import { message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getAssetAllocationList, returnAsset } from '#/api/system/asset';
import { $t } from '#/locales';

import { useColumns } from './data';

const [Grid, gridApi] = useVbenVxeGrid<AssetApi.AllocationItem>({
  formOptions: {
    schema: [
      { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'userId', label: $t('asset.userId') },
      {
        component: 'Select',
        componentProps: {
          allowClear: true,
          options: [
            { label: () => $t('common.all'), value: undefined },
            { label: () => $t('asset.statusAllocated'), value: false },
            { label: () => $t('asset.returnedAt'), value: true },
          ],
          class: 'w-full',
        },
        fieldName: 'returned',
        label: $t('asset.returnedAt'),
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
            assetId: formValues.assetId,
            userId: formValues.userId,
            returned: formValues.returned,
          };
          const result = await getAssetAllocationList(params);
          return { items: result.items ?? [], total: result.total ?? 0 };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, search: true, zoom: true },
  },
});

function onActionClick(e: OnActionClickParams<AssetApi.AllocationItem>) {
  if (e.code === 'return') onReturn(e.row);
}

function onReturn(row: AssetApi.AllocationItem) {
  if (row.returnedAt) return;
  const key = 'alloc_return';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  returnAsset(row.id)
    .then(() => {
      message.success({ content: $t('common.success'), key });
      gridApi.query();
    })
    .finally(() => hide());
}
</script>

<template>
  <Page auto-content-height>
    <Grid :table-title="$t('asset.allocationList')" />
  </Page>
</template>
