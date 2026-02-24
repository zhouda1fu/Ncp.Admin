<script lang="ts" setup>
import type { ExpenseApi } from '#/api/system/expense';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button, Input, InputNumber, Select } from 'ant-design-vue';

import { createExpenseClaim } from '#/api/system/expense';
import { $t } from '#/locales';

import { TYPE_OPTIONS } from '../data';

const typeOptions = TYPE_OPTIONS.map((o) => ({
  label: typeof o.label === 'function' ? o.label() : String(o.label),
  value: o.value,
}));

const emit = defineEmits(['success']);

interface ItemRow {
  type: number;
  amount: number;
  description: string;
  invoiceUrl?: string;
}

const items = ref<ItemRow[]>([{ type: 0, amount: 0, description: '', invoiceUrl: '' }]);
const getTitle = $t('ui.actionTitle.create', [$t('expense.claim.name')]);

function addItem() {
  items.value.push({ type: 0, amount: 0, description: '', invoiceUrl: '' });
}

function removeItem(index: number) {
  if (items.value.length <= 1) return;
  items.value.splice(index, 1);
}

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const valid = items.value.every(
      (i) => i.type >= 0 && i.amount > 0 && i.description.trim().length > 0,
    );
    if (!valid) {
      return;
    }
    drawerApi.lock();
    try {
      await createExpenseClaim({
        items: items.value.map((i) => ({
          type: i.type,
          amount: Number(i.amount),
          description: i.description.trim(),
          invoiceUrl: i.invoiceUrl?.trim() || undefined,
        })),
      });
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      items.value = [{ type: 0, amount: 0, description: '', invoiceUrl: '' }];
    }
  },
});
</script>

<template>
  <Drawer :title="getTitle">
    <div class="mx-4 space-y-4">
      <div class="flex items-center justify-between">
        <span class="font-medium">{{ $t('expense.claim.items') }}</span>
        <Button type="dashed" size="small" @click="addItem">
          {{ $t('expense.claim.addItem') }}
        </Button>
      </div>
      <div
        v-for="(item, index) in items"
        :key="index"
        class="flex flex-wrap items-start gap-2 rounded border p-3"
      >
        <Select
          v-model:value="item.type"
          :options="typeOptions"
          class="w-28 shrink-0"
          :placeholder="$t('expense.claim.type')"
        />
        <InputNumber
          v-model:value="item.amount"
          :min="0.01"
          :precision="2"
          class="w-28"
          placeholder="$t('expense.claim.amount')"
        />
        <Input
          v-model:value="item.description"
          class="min-w-40 flex-1"
          :placeholder="$t('expense.claim.description')"
        />
        <Input
          v-model:value="item.invoiceUrl"
          class="min-w-32 flex-1"
          :placeholder="$t('expense.claim.invoiceUrl')"
        />
        <Button
          v-if="items.length > 1"
          danger
          size="small"
          type="text"
          @click="removeItem(index)"
        >
          {{ $t('common.delete') }}
        </Button>
      </div>
    </div>
  </Drawer>
</template>
