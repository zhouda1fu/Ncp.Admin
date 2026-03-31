<script lang="ts" setup>
import type { CustomerApi } from '#/api/system/customer';

import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Button, Input, Table } from 'ant-design-vue';
import type { TableColumnType } from 'ant-design-vue';

import { getCustomerList } from '#/api/system/customer';
import { $t } from '#/locales';

const STATUS_MAP: Record<number, string> = {
  0: 'customer.statusFollowUpUnclear',
  1: 'customer.statusFollowUpInterested',
  2: 'customer.statusCooperating',
  3: 'customer.statusFormerCooperating',
};

function statusLabel(status: number | undefined): string {
  if (status === undefined) return '-';
  return $t(STATUS_MAP[status] ?? '');
}

const [Modal, modalApi] = useVbenModal({
  showConfirmButton: false,
  showCancelButton: true,
  onOpenChange(isOpen) {
    if (isOpen) {
      searchKeyword.value = '';
      currentPage.value = 1;
      fetchList();
    }
  },
});

const list = ref<CustomerApi.CustomerItem[]>([]);
const loading = ref(false);
const total = ref(0);
const pageSize = 10;
const currentPage = ref(1);
const searchKeyword = ref('');
const ownerId = ref<string>('');

const columns: TableColumnType<CustomerApi.CustomerItem>[] = [
  {
    title: () => $t('customer.fullName'),
    dataIndex: 'fullName',
    key: 'fullName',
    width: 180,
    ellipsis: true,
    customRender: ({ record }) =>
      record.shortName ? `${record.fullName}（${record.shortName}）` : record.fullName,
  },
  {
    title: () => $t('customer.mainContactName'),
    dataIndex: 'mainContactName',
    key: 'mainContactName',
    width: 120,
    ellipsis: true,
  },
  {
    title: () => $t('customer.ownerId'),
    dataIndex: 'ownerName',
    key: 'ownerName',
    width: 100,
    ellipsis: true,
  },
  {
    title: () => $t('customer.ownerDept'),
    dataIndex: 'ownerDeptName',
    key: 'ownerDeptName',
    width: 120,
    ellipsis: true,
    customRender: ({ record }) => (record.ownerDeptName ? String(record.ownerDeptName) : '-'),
  },
  {
    title: () => $t('customer.status'),
    dataIndex: 'status',
    key: 'status',
    width: 120,
    customRender: ({ record }) => statusLabel(record.status),
  },
  {
    title: () => $t('task.project.operation'),
    key: 'action',
    width: 100,
    fixed: 'right',
  },
];

const pagination = computed(() => ({
  current: currentPage.value,
  pageSize,
  total: total.value,
  showSizeChanger: false,
  showTotal: (t: number) => $t('ui.pagination.total', [String(t)]),
}));

async function fetchList() {
  loading.value = true;
  try {
    const res = await getCustomerList({
      pageIndex: currentPage.value,
      pageSize,
      fullName: searchKeyword.value.trim() || undefined,
      ownerId: ownerId.value || undefined,
    });
    list.value = res?.items ?? [];
    total.value = res?.total ?? 0;
  } finally {
    loading.value = false;
  }
}

function onSearch() {
  currentPage.value = 1;
  fetchList();
}

function onTableChange(pag: { current?: number }) {
  if (pag?.current) {
    currentPage.value = pag.current;
    fetchList();
  }
}

function handleSelect(record: CustomerApi.CustomerItem) {
  const data = modalApi.getData<{ onSelect?: (row: CustomerApi.CustomerItem) => void; ownerId?: string }>();
  data?.onSelect?.(record);
  modalApi.close();
}

function open(payload?: { onSelect: (row: CustomerApi.CustomerItem) => void; ownerId?: string }) {
  ownerId.value = payload?.ownerId ?? '';
  modalApi.setData(payload ?? {}).open();
}

defineExpose({ open });
</script>

<template>
  <Modal :title="$t('task.project.selectCustomer')" class="!w-[720px]">
    <div class="flex flex-col gap-3">
      <div class="flex items-center gap-2">
        <Input
          v-model:value="searchKeyword"
          :placeholder="$t('customer.fullNamePlaceholder')"
          class="max-w-[240px]"
          allow-clear
          @press-enter="onSearch"
        />
        <Button type="primary" @click="onSearch">
          {{ $t('common.search') }}
        </Button>
      </div>
      <Table
        :columns="columns"
        :data-source="list"
        :loading="loading"
        :pagination="pagination"
        row-key="id"
        size="small"
        :scroll="{ x: 600 }"
        @change="onTableChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'action'">
            <Button type="link" size="small" @click="handleSelect(record as CustomerApi.CustomerItem)">
              {{ $t('customer.select') }}
            </Button>
          </template>
        </template>
      </Table>
    </div>
  </Modal>
</template>
