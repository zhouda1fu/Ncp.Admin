<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { AssetApi } from '#/api/system/asset';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getAssetList,
  getAsset,
  scrapAsset,
} from '#/api/system/asset';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';
import AllocateForm from './modules/allocate-form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [AllocateDrawer, allocateDrawerApi] = useVbenDrawer({
  connectedComponent: AllocateForm,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<AssetApi.AssetItem>({
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
            code: formValues.code,
            name: formValues.name,
            status: formValues.status,
          };
          const result = await getAssetList(params);
          return { items: result.items ?? [], total: result.total ?? 0 };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    toolbarConfig: { custom: true, export: false, refresh: true, search: true, zoom: true },
  },
});

function onActionClick(e: OnActionClickParams<AssetApi.AssetItem>) {
  if (e.code === 'edit') onEdit(e.row);
  else if (e.code === 'allocate') onAllocate(e.row);
  else if (e.code === 'scrap') onScrap(e.row);
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}

async function onEdit(row: AssetApi.AssetItem) {
  const detail = await getAsset(row.id);
  formDrawerApi.setData(detail).open();
}

function onAllocate(row: AssetApi.AssetItem) {
  allocateDrawerApi.setData({ assetId: row.id, assetName: row.name }).open();
}

async function onScrap(row: AssetApi.AssetItem) {
  const key = 'asset_scrap';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await scrapAsset(row.id);
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
    <AllocateDrawer @success="onRefresh" />
    <Grid :table-title="$t('asset.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('asset.create') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
