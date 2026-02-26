<script lang="ts" setup>
import type { VehicleApi } from '#/api/system/vehicle';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createVehicle, updateVehicle } from '#/api/system/vehicle';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<VehicleApi.VehicleItem> & { id?: string }>();

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
      plateNumber: String(data.plateNumber),
      model: String(data.model),
      remark: data.remark ? String(data.remark) : undefined,
    };
    try {
      if (formData.value?.id) {
        await updateVehicle(formData.value.id, payload);
      } else {
        await createVehicle(payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<VehicleApi.VehicleItem> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        plateNumber: data?.plateNumber ?? '',
        model: data?.model ?? '',
        remark: data?.remark ?? '',
      });
    }
  },
});
</script>

<template>
  <Drawer :title="formData?.id ? $t('vehicle.edit') : $t('vehicle.create')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <Button type="primary" danger @click="resetForm">{{ $t('common.reset') }}</Button>
    </template>
  </Drawer>
</template>
