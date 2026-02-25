<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { DocumentApi } from '#/api/system/document';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getDocumentList } from '#/api/system/document';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import DocumentDetail from './modules/document-detail.vue';
import UploadForm from './modules/upload-form.vue';

const [UploadDrawer, uploadDrawerApi] = useVbenDrawer({
  connectedComponent: UploadForm,
  destroyOnClose: true,
});
const [DetailDrawer, detailDrawerApi] = useVbenDrawer({
  connectedComponent: DocumentDetail,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<DocumentApi.DocumentItem>({
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
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          const params: Recordable<any> = {
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            title: formValues.title,
          };
          const result = await getDocumentList(params);
          return {
            items: result.items ?? [],
            total: result.total ?? 0,
          };
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

function onActionClick(e: OnActionClickParams<DocumentApi.DocumentItem>) {
  if (e.code === 'edit') {
    detailDrawerApi.setData(e.row).open();
  }
}

function onRefresh() {
  gridApi.query();
}

function onUpload() {
  uploadDrawerApi.setData({}).open();
}
</script>

<template>
  <Page auto-content-height>
    <UploadDrawer @success="onRefresh" />
    <DetailDrawer @success="onRefresh" />
    <Grid :table-title="$t('document.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onUpload">
          <Plus class="size-5 shrink-0" />
          {{ $t('document.upload') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
