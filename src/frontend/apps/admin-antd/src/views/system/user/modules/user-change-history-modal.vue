<script lang="ts" setup>
import type { SystemUserApi } from '#/api/system/user';

import { computed, ref, watch } from 'vue';

import { ArrowLeft } from '@vben/icons';
import { Button, Input, Modal, Table, Tag } from 'ant-design-vue';

import { getUserChangeHistory } from '#/api/system/user';
import { $t } from '#/locales';

const props = defineProps<{
  open: boolean;
  row: SystemUserApi.SystemUser | null;
}>();

const emit = defineEmits<{ 'update:open': [boolean] }>();

const loading = ref(false);
const items = ref<SystemUserApi.UserFieldChangeRow[]>([]);
const total = ref(0);
const pageIndex = ref(1);
const pageSize = ref(20);
const keyword = ref('');

const titleText = computed(() => {
  const name = props.row?.name?.trim() || props.row?.realName?.trim() || '';
  return name
    ? `${$t('system.user.changeHistoryTitle')} — ${name}`
    : $t('system.user.changeHistoryTitle');
});

const columns = computed(() => [
  {
    key: 'fieldKey',
    title: $t('system.user.changeColField'),
    dataIndex: 'fieldKey',
    width: 140,
    ellipsis: true,
  },
  {
    key: 'oldDisplay',
    title: $t('system.user.changeColOld'),
    dataIndex: 'oldDisplay',
    width: 220,
    ellipsis: true,
  },
  {
    key: 'newDisplay',
    title: $t('system.user.changeColNew'),
    dataIndex: 'newDisplay',
    width: 220,
    ellipsis: true,
  },
  {
    key: 'operatorUserName',
    title: $t('system.user.changeColOperator'),
    dataIndex: 'operatorUserName',
    width: 120,
    ellipsis: true,
  },
  {
    key: 'changedAt',
    title: $t('system.user.changeColTime'),
    dataIndex: 'changedAt',
    width: 168,
  },
]);

function fieldLabel(fieldKey: string) {
  const path = `system.user.changeHistoryField.${fieldKey}`;
  const t = $t(path);
  return t === path ? fieldKey : t;
}

function formatChangedAt(v?: string) {
  if (!v) return '—';
  const d = new Date(v);
  return Number.isNaN(d.getTime()) ? v : d.toLocaleString('zh-CN', { hour12: false });
}

async function load() {
  if (!props.open || !props.row?.userId) return;
  loading.value = true;
  try {
    const res = await getUserChangeHistory(props.row.userId, {
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
      keyword: keyword.value.trim() || undefined,
    });
    items.value = res.items ?? [];
    total.value = res.total ?? 0;
  } catch {
    items.value = [];
    total.value = 0;
  } finally {
    loading.value = false;
  }
}

watch(
  () => [props.open, props.row?.userId] as const,
  () => {
    if (!props.open) return;
    pageIndex.value = 1;
    keyword.value = '';
    void load();
  },
);

function onPageChange(p: number, ps: number) {
  pageIndex.value = p;
  pageSize.value = ps;
  void load();
}

function onTableChange(pag: { current?: number; pageSize?: number }) {
  onPageChange(pag.current ?? 1, pag.pageSize ?? pageSize.value);
}

function onClose() {
  emit('update:open', false);
}

function onSearch() {
  pageIndex.value = 1;
  void load();
}
</script>

<template>
  <Modal
    :open="open"
    :title="titleText"
    width="960px"
    :footer="null"
    destroy-on-close
    class="user-change-history-modal"
    @cancel="onClose"
  >
    <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
      <div class="flex flex-wrap items-center gap-2">
        <Button class="inline-flex items-center gap-1" @click="onClose">
          <ArrowLeft class="size-4 shrink-0" />
          {{ $t('system.user.changeHistoryBack') }}
        </Button>
      </div>
      <Input.Search
        v-model:value="keyword"
        allow-clear
        class="max-w-xs min-w-[200px]"
        :placeholder="$t('system.user.changeHistorySearchPlaceholder')"
        @search="onSearch"
      />
    </div>
    <Table
      class="user-change-history-table"
      :loading="loading"
      :columns="columns"
      :data-source="items"
      :pagination="{
        current: pageIndex,
        pageSize,
        total,
        showSizeChanger: true,
        pageSizeOptions: ['10', '20', '50'],
        showTotal: (t: number) => $t('system.user.changeHistoryTotal', { count: t }),
      }"
      :row-key="(r: SystemUserApi.UserFieldChangeRow, i?: number) => `${r.fieldKey}-${r.changedAt}-${i ?? 0}`"
      size="small"
      :scroll="{ x: 900 }"
      @change="onTableChange"
    >
      <template #emptyText>
        <span class="text-muted-foreground">{{ $t('system.user.changeHistoryEmpty') }}</span>
      </template>
      <template #bodyCell="{ column, record }: any">
        <template v-if="column.key === 'fieldKey'">
          {{ fieldLabel(record.fieldKey) }}
        </template>
        <template v-else-if="column.key === 'oldDisplay'">
          <Tag color="warning" class="m-0 max-w-full whitespace-normal break-words">
            {{ record.oldDisplay || '—' }}
          </Tag>
        </template>
        <template v-else-if="column.key === 'newDisplay'">
          <Tag color="processing" class="m-0 max-w-full whitespace-normal break-words">
            {{ record.newDisplay || '—' }}
          </Tag>
        </template>
        <template v-else-if="column.key === 'operatorUserName'">
          {{ record.operatorUserName || '—' }}
        </template>
        <template v-else-if="column.key === 'changedAt'">
          {{ formatChangedAt(record.changedAt) }}
        </template>
      </template>
    </Table>
  </Modal>
</template>

<style scoped>
.user-change-history-table :deep(.ant-table-thead > tr > th) {
  color: hsl(var(--primary));
  font-weight: 600;
}
</style>
