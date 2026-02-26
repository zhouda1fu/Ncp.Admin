<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { ContractApi } from '#/api/system/contract';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getContractList,
  getContract,
  submitContract,
  approveContract,
  archiveContract,
} from '#/api/system/contract';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<ContractApi.ContractItem>({
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
            code: formValues.code,
            title: formValues.title,
            status: formValues.status,
          };
          const result = await getContractList(params);
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

function onActionClick(e: OnActionClickParams<ContractApi.ContractItem>) {
  if (e.code === 'edit') {
    onEdit(e.row);
  } else if (e.code === 'submit') {
    onSubmit(e.row);
  } else if (e.code === 'approve') {
    onApprove(e.row);
  } else if (e.code === 'archive') {
    onArchive(e.row);
  }
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}

async function onEdit(row: ContractApi.ContractItem) {
  const detail = await getContract(row.id);
  formDrawerApi.setData(detail).open();
}

async function onSubmit(row: ContractApi.ContractItem) {
  const key = 'contract_submit';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await submitContract(row.id);
    message.success({ content: $t('common.success'), key });
    onRefresh();
  } finally {
    hide();
  }
}

async function onApprove(row: ContractApi.ContractItem) {
  const key = 'contract_approve';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await approveContract(row.id);
    message.success({ content: $t('common.success'), key });
    onRefresh();
  } finally {
    hide();
  }
}

async function onArchive(row: ContractApi.ContractItem) {
  const key = 'contract_archive';
  const hide = message.loading({ content: $t('common.loading'), duration: 0, key });
  try {
    await archiveContract(row.id);
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
    <Grid :table-title="$t('contract.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('contract.create') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
