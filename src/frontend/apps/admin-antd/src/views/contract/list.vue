<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type {
  OnActionClickParams,
  VxeTableGridOptions,
} from '#/adapter/vxe-table';
import type { ContractApi } from '#/api/system/contract';
import type {
  ContractTypeOptionItem,
  IncomeExpenseTypeOptionItem,
} from './data';

import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getContractList,
  getContract,
  deleteContract,
  submitContract,
  approveContract,
  archiveContract,
} from '#/api/system/contract';
import type { ContractTypeOptionApi } from '#/api/system/contract-type';
import type { IncomeExpenseTypeOptionApi } from '#/api/system/income-expense-type';
import { getContractTypeOptionList } from '#/api/system/contract-type';
import { getIncomeExpenseTypeOptionList } from '#/api/system/income-expense-type';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';

const route = useRoute();
const router = useRouter();
const orderIdFromRoute = computed(() => (route.query.orderId as string) ?? '');

const contractTypeOptions = ref<ContractTypeOptionItem[]>([]);
const incomeExpenseTypeOptions = ref<IncomeExpenseTypeOptionItem[]>([]);

const formSchemaRef = ref(useGridFormSchema([], []));
const columnsRef = ref(useColumns(onActionClick, [], []));

onMounted(async () => {
  const [ctList, ieList] = await Promise.all([
    getContractTypeOptionList(),
    getIncomeExpenseTypeOptionList(),
  ]) as unknown as [ContractTypeOptionApi.ContractTypeOptionItem[], IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem[]];
  contractTypeOptions.value = ctList.map((x) => ({ label: x.name, value: x.typeValue }));
  incomeExpenseTypeOptions.value = ieList.map((x) => ({ label: x.name, value: x.typeValue }));
  formSchemaRef.value = useGridFormSchema(contractTypeOptions.value, incomeExpenseTypeOptions.value);
  columnsRef.value = useColumns(onActionClick, contractTypeOptions.value, incomeExpenseTypeOptions.value);
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
  } else if (e.code === 'delete') {
    onDelete(e.row);
  }
}

const [Grid, gridApi] = useVbenVxeGrid<ContractApi.ContractItem>({
  formOptions: {
    schema: formSchemaRef as unknown as import('#/adapter/form').VbenFormSchema[],
    submitOnChange: true,
  },
  gridOptions: {
    columns: columnsRef as unknown as VxeTableGridOptions<ContractApi.ContractItem>['columns'],
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
            code: formValues?.code,
            title: formValues?.title,
            status: formValues?.status,
            orderId: orderIdFromRoute.value || formValues?.orderId,
            customerId: formValues?.customerId,
            contractType: formValues?.contractType,
            incomeExpenseType: formValues?.incomeExpenseType,
            signDateFrom: formValues?.signDateFrom,
            signDateTo: formValues?.signDateTo,
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

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  const path = orderIdFromRoute.value
    ? `/contract/create?orderId=${encodeURIComponent(orderIdFromRoute.value)}`
    : '/contract/create';
  router.push(path);
}

async function onDelete(row: ContractApi.ContractItem) {
  Modal.confirm({
    title: $t('contract.confirmDelete'),
    onOk: async () => {
      await deleteContract(row.id);
      message.success($t('common.success'));
      onRefresh();
    },
  });
}

function onEdit(row: ContractApi.ContractItem) {
  router.push(`/contract/edit/${row.id}`);
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
