<script lang="ts" setup>
import type { ContractApi } from '#/api/system/contract';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createContract,
  updateContract,
} from '#/api/system/contract';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<ContractApi.ContractItem> & { id?: string }>();

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
    const payload = {
      code: String(data.code),
      title: String(data.title),
      partyA: String(data.partyA),
      partyB: String(data.partyB),
      amount: Number(data.amount) ?? 0,
      startDate: data.startDate
        ? new Date(data.startDate).toISOString()
        : new Date().toISOString(),
      endDate: data.endDate
        ? new Date(data.endDate).toISOString()
        : new Date().toISOString(),
      fileStorageKey: data.fileStorageKey ? String(data.fileStorageKey) : undefined,
    };
    try {
      if (formData.value?.id) {
        await updateContract(formData.value.id, payload);
      } else {
        await createContract(payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<ContractApi.ContractItem> & { id?: string }>();
      formData.value = data;
      const start = data?.startDate
        ? (typeof data.startDate === 'string'
            ? data.startDate.slice(0, 10)
            : '')
        : '';
      const end = data?.endDate
        ? (typeof data.endDate === 'string'
            ? data.endDate.slice(0, 10)
            : '')
        : '';
      formApi.setValues({
        code: data?.code ?? '',
        title: data?.title ?? '',
        partyA: data?.partyA ?? '',
        partyB: data?.partyB ?? '',
        amount: data?.amount ?? 0,
        startDate: start,
        endDate: end,
        fileStorageKey: data?.fileStorageKey ?? '',
      });
    }
  },
});
</script>

<template>
  <Drawer :title="formData?.id ? $t('contract.edit') : $t('contract.create')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Drawer>
</template>
