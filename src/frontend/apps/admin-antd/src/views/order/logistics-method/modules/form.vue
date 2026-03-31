<script lang="ts" setup>
import type { OrderLogisticsMethodApi } from '#/api/system/order-logistics-method';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createOrderLogisticsMethod,
  updateOrderLogisticsMethod,
} from '#/api/system/order-logistics-method';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<
  Partial<OrderLogisticsMethodApi.OrderLogisticsMethodItem> & { id?: string }
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
      if (formData.value?.id != null) {
        await updateOrderLogisticsMethod(formData.value.id, {
          name: String(data.name ?? ''),
          typeValue: Number(data.typeValue) ?? 0,
          sort: Number(data.sort) ?? 0,
        });
      } else {
        await createOrderLogisticsMethod({
          name: String(data.name ?? ''),
          typeValue: Number(data.typeValue) ?? 0,
          sort: Number(data.sort) ?? 0,
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
        Partial<OrderLogisticsMethodApi.OrderLogisticsMethodItem> & {
          id?: string;
        }
      >();
      formData.value = data;
      formApi.setValues({
        name: data?.name ?? '',
        typeValue: data?.typeValue ?? 0,
        sort: data?.sort ?? 0,
      });
    }
  },
});
</script>

<template>
  <Drawer
    :title="
      formData?.id != null
        ? $t('ui.actionTitle.edit', [$t('order.logisticsMethodList')])
        : $t('ui.actionTitle.create', [$t('order.logisticsMethodList')])
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
