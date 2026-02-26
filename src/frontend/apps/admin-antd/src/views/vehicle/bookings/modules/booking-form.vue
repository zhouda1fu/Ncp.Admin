<script lang="ts" setup>
import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { z } from '#/adapter/form';
import { createVehicleBooking, getVehicleList } from '#/api/system/vehicle';
import { $t } from '#/locales';

const emit = defineEmits(['success']);
const vehicleOptions = ref<{ label: string; value: string }[]>([]);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: [
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        options: vehicleOptions,
        fieldNames: { label: 'label', value: 'value' },
      },
      fieldName: 'vehicleId',
      label: $t('vehicle.plateNumber'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('vehicle.plateNumber')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'purpose',
      label: $t('vehicle.purpose'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('vehicle.purpose')])),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', showTime: true, valueFormat: 'YYYY-MM-DDTHH:mm:ss' },
      fieldName: 'startAt',
      label: $t('vehicle.startAt'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('vehicle.startAt')])),
    },
    {
      component: 'DatePicker',
      componentProps: { class: 'w-full', showTime: true, valueFormat: 'YYYY-MM-DDTHH:mm:ss' },
      fieldName: 'endAt',
      label: $t('vehicle.endAt'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('vehicle.endAt')])),
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
    const startAt = data.startAt ? new Date(data.startAt).toISOString() : '';
    const endAt = data.endAt ? new Date(data.endAt).toISOString() : '';
    try {
      await createVehicleBooking({
        vehicleId: String(data.vehicleId),
        purpose: String(data.purpose),
        startAt,
        endAt,
      });
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  async onOpenChange(isOpen) {
    if (isOpen) {
      formApi.setValues({ vehicleId: undefined, purpose: '', startAt: '', endAt: '' });
      const res = await getVehicleList({ pageIndex: 1, pageSize: 500 });
      const list = res.items ?? [];
      vehicleOptions.value = list.map((v) => ({ label: `${v.plateNumber} - ${v.model}`, value: v.id }));
    }
  },
});
</script>

<template>
  <Drawer :title="$t('vehicle.bookingList') + ' - ' + $t('vehicle.create')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <Button type="primary" danger @click="() => formApi.resetForm()">{{ $t('common.reset') }}</Button>
    </template>
  </Drawer>
</template>
