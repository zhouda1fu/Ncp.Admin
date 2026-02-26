<script lang="ts" setup>
import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { z } from '#/adapter/form';
import { allocateAsset } from '#/api/system/asset';
import { $t } from '#/locales';

const emit = defineEmits(['success']);
const formData = ref<{ assetId: string; assetName: string }>({ assetId: '', assetName: '' });

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: [
    {
      component: 'InputNumber',
      componentProps: { min: 1, class: 'w-full' },
      fieldName: 'userId',
      label: $t('asset.userId'),
      rules: z.number().min(1, $t('ui.formRules.required', [$t('asset.userId')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'note',
      label: $t('asset.note'),
    },
  ],
  showDefaultActions: false,
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    const data = await formApi.getValues();
    try {
      await allocateAsset(formData.value!.assetId, {
        userId: Number(data.userId),
        note: data.note ? String(data.note) : undefined,
      });
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<{ assetId: string; assetName: string }>();
      formData.value = data ?? { assetId: '', assetName: '' };
      formApi.setValues({ userId: undefined, note: '' });
    }
  },
});
</script>

<template>
  <Drawer :title="$t('asset.allocate') + ' - ' + (formData?.assetName ?? '')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <Button type="primary" danger @click="() => formApi.resetForm()">{{ $t('common.reset') }}</Button>
    </template>
  </Drawer>
</template>
