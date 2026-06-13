<script lang="ts" setup>
import type { SystemRoleApi } from '#/api/system/role';

import { computed, ref, watch } from 'vue';

import { ArrowLeft } from '@vben/icons';
import { Button, Input, Modal, Table, Tag } from 'ant-design-vue';

import { getRoleChangeHistory } from '#/api/system/role';
import { $t } from '#/locales';

export interface RoleFieldChangeRow {
  fieldKey: string;
  oldDisplay: string;
  newDisplay: string;
  operatorUserName: string;
  changedAt: string;
}

const props = defineProps<{
  open: boolean;
  row: SystemRoleApi.SystemRole | null;
}>();

const emit = defineEmits<{ 'update:open': [boolean] }>();

const loading = ref(false);
const items = ref<RoleFieldChangeRow[]>([]);
const total = ref(0);
const pageIndex = ref(1);
const pageSize = ref(20);
const keyword = ref('');

const titleText = computed(() => {
  const name = props.row?.name?.trim() || '';
  return name
    ? `${$t('system.role.changeHistoryTitle')} — ${name}`
    : $t('system.role.changeHistoryTitle');
});

const columns = computed(() => [
  {
    key: 'fieldKey',
    title: $t('system.role.changeColField'),
    dataIndex: 'fieldKey',
    width: 140,
    ellipsis: true,
  },
  {
    key: 'oldDisplay',
    title: $t('system.role.changeColOld'),
    dataIndex: 'oldDisplay',
    width: 220,
    ellipsis: true,
  },
  {
    key: 'newDisplay',
    title: $t('system.role.changeColNew'),
    dataIndex: 'newDisplay',
    width: 220,
    ellipsis: true,
  },
  {
    key: 'operatorUserName',
    title: $t('system.role.changeColOperator'),
    dataIndex: 'operatorUserName',
    width: 120,
    ellipsis: true,
  },
  {
    key: 'changedAt',
    title: $t('system.role.changeColTime'),
    dataIndex: 'changedAt',
    width: 168,
  },
]);

function fieldLabel(fieldKey: string) {
  const path = `system.role.changeHistoryField.${fieldKey}`;
  const t = $t(path);
  return t === path ? fieldKey : t;
}

function formatChangedAt(v?: string) {
  if (!v) return '—';
  const d = new Date(v);
  return Number.isNaN(d.getTime()) ? v : d.toLocaleString('zh-CN', { hour12: false });
}

function resolveRoleId(row: SystemRoleApi.SystemRole | null): string {
  if (!row?.roleId) return '';
  const raw = row.roleId as unknown;
  if (typeof raw === 'string') return raw.trim();
  if (raw && typeof raw === 'object' && 'id' in (raw as Record<string, unknown>)) {
    return String((raw as Record<string, unknown>).id ?? '').trim();
  }
  return String(raw).trim();
}

async function load() {
  const roleId = resolveRoleId(props.row);
  if (!props.open || !roleId) return;
  loading.value = true;
  try {
    const res = await getRoleChangeHistory(roleId, {
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
  () => [props.open, resolveRoleId(props.row)] as const,
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
    class="role-change-history-modal"
    @cancel="onClose"
  >
    <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
      <div class="flex flex-wrap items-center gap-2">
        <Button class="inline-flex items-center gap-1" @click="onClose">
          <ArrowLeft class="size-4 shrink-0" />
          {{ $t('system.role.changeHistoryBack') }}
        </Button>
      </div>
      <Input.Search
        v-model:value="keyword"
        allow-clear
        class="max-w-xs min-w-[200px]"
        :placeholder="$t('system.role.changeHistorySearchPlaceholder')"
        @search="onSearch"
      />
    </div>
    <Table
      class="role-change-history-table"
      :loading="loading"
      :columns="columns"
      :data-source="items"
      :pagination="{
        current: pageIndex,
        pageSize,
        total,
        showSizeChanger: true,
        pageSizeOptions: ['10', '20', '50'],
        showTotal: (t: number) => $t('system.role.changeHistoryTotal', { count: t }),
      }"
      :row-key="(r: RoleFieldChangeRow, i?: number) => `${r.fieldKey}-${r.changedAt}-${i ?? 0}`"
      size="small"
      :scroll="{ x: 900 }"
      @change="onTableChange"
    >
      <template #emptyText>
        <span class="text-muted-foreground">{{ $t('system.role.changeHistoryEmpty') }}</span>
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
.role-change-history-table :deep(.ant-table-thead > tr > th) {
  color: #1677ff;
  font-weight: 600;
}
</style>
