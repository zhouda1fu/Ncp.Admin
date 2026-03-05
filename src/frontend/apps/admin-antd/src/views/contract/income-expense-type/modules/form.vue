<script lang="ts" setup>
import type { IncomeExpenseTypeOptionApi } from '#/api/system/income-expense-type';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createIncomeExpenseTypeOption,
  updateIncomeExpenseTypeOption,
} from '#/api/system/income-expense-type';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<
  Partial<IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem> & { id?: string }
>();

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: useSchema(),
  showDefaultActions: false,
});

function resetForm() {
  formApi.resetForm();
  formApi.setValues(formData.value || {});
}

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    const data = await formApi.getValues();
    try {
      if (formData.value?.id) {
        await updateIncomeExpenseTypeOption(formData.value.id, {
          name: String(data.name ?? ''),
          typeValue: Number(data.typeValue) ?? 0,
          sortOrder: Number(data.sortOrder) ?? 0,
        });
      } else {
        await createIncomeExpenseTypeOption({
          name: String(data.name ?? ''),
          typeValue: Number(data.typeValue) ?? 0,
          sortOrder: Number(data.sortOrder) ?? 0,
        });
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<
        Partial<IncomeExpenseTypeOptionApi.IncomeExpenseTypeOptionItem> & { id?: string }
      >();
      formData.value = data;
      formApi.setValues({
        name: data?.name ?? '',
        typeValue: data?.typeValue ?? 0,
        sortOrder: data?.sortOrder ?? 0,
      });
    }
  },
});
</script>

<template>
  <Drawer
    :title="
      formData?.id
        ? $t('ui.actionTitle.edit', [$t('contract.incomeExpenseTypeList')])
        : $t('ui.actionTitle.create', [$t('contract.incomeExpenseTypeList')])
    "
  >
    <Form class="mx-4" />
    <template #prepend-footer>
      <Button type="primary" danger @click="resetForm">
        {{ $t('common.reset') }}
      </Button>
    </template>
  </Drawer>
</template>
