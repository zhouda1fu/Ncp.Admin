<script lang="ts" setup>
import type { AssetApi } from '#/api/system/asset';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createAsset, updateAsset } from '#/api/system/asset';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<AssetApi.AssetItem> & { id?: string }>();

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
      name: String(data.name),
      category: String(data.category),
      code: String(data.code),
      purchaseDate: data.purchaseDate ? new Date(data.purchaseDate).toISOString() : new Date().toISOString(),
      value: Number(data.value) ?? 0,
      remark: data.remark ? String(data.remark) : undefined,
    };
    try {
      if (formData.value?.id) {
        await updateAsset(formData.value.id, payload);
      } else {
        await createAsset(payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<AssetApi.AssetItem> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        name: data?.name ?? '',
        category: data?.category ?? '',
        code: data?.code ?? '',
        purchaseDate: data?.purchaseDate ? String(data.purchaseDate).slice(0, 10) : '',
        value: data?.value ?? 0,
        remark: data?.remark ?? '',
      });
    }
  },
});
</script>

<template>
  <Drawer :title="formData?.id ? $t('asset.edit') : $t('asset.create')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <Button type="primary" danger @click="resetForm">{{ $t('common.reset') }}</Button>
    </template>
  </Drawer>
</template>
