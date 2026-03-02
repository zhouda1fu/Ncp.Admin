<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { CustomerApi } from '#/api/system/customer';

import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getCustomerList, releaseCustomerToSea } from '#/api/system/customer';
import { getCustomerSourceList } from '#/api/system/customerSource';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const router = useRouter();
const customerSourceOptions = ref<{ label: string; value: string }[]>([]);

onMounted(() => {
  getCustomerSourceList().then((list) => {
    customerSourceOptions.value = list.map((x) => ({ label: x.name, value: x.id }));
  });
});

const [Grid, gridApi] = useVbenVxeGrid<CustomerApi.CustomerItem>({
  formOptions: {
    schema: computed(() => useGridFormSchema(customerSourceOptions.value)) as unknown as VbenFormSchema[],
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
            fullName: formValues.fullName,
            customerSourceId: formValues.customerSourceId,
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
  router.push('/customer/create');
}

function onEdit(row: CustomerApi.CustomerItem) {
  router.push(`/customer/${row.id}/edit`);
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
