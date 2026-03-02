<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { IndustryApi } from '#/api/system/industry';

import { ref } from 'vue';
import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteIndustry, getIndustryList } from '#/api/system/industry';
import { $t } from '#/locales';

import { flatToTree, useColumns, useGridFormSchema } from './data';
import type { IndustryTreeItem } from './data';
import Form from './modules/form.vue';

const industryList = ref<IndustryApi.IndustryItem[]>([]);

async function onActionClick(e: OnActionClickParams<IndustryApi.IndustryItem>) {
  if (e.code === 'edit') {
    formDrawerApi.setData(e.row).open();
  } else if (e.code === 'delete') {
    await deleteIndustry(e.row.id);
    message.success($t('ui.actionMessage.operationSuccess'));
    await onRefresh();
  }
}

// VxeGrid 要求 columns 为数组，不能用 computed ref；在 query 里随 industryList 更新
const gridColumns = ref(useColumns(onActionClick, []));

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<IndustryApi.IndustryItem>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: gridColumns.value,
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          _params: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<{ name?: string }>,
        ) => {
          const list = await getIndustryList();
          industryList.value = list;
          gridColumns.value = useColumns(onActionClick, list);
          gridApi.setGridOptions({ columns: gridColumns.value });
          let items: IndustryApi.IndustryItem[] = list;
          if (formValues?.name) {
            const kw = String(formValues.name).trim().toLowerCase();
            items = items.filter((x) => x.name.toLowerCase().includes(kw));
          }
          const tree = flatToTree(items) as IndustryTreeItem[];
          return { items: tree, total: items.length };
        },
      },
    },
    rowConfig: { keyField: 'id' },
    treeConfig: {
      childrenField: 'children',
      rowField: 'id',
      transform: false,
    },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

async function onRefresh() {
  // 清空搜索条件，避免新增/编辑后被过滤掉
  gridApi.formApi?.setValues?.({ name: '' });
  const list = await getIndustryList();
  industryList.value = list;
  await gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid :table-title="$t('customer.industryList')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('customer.industry')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
