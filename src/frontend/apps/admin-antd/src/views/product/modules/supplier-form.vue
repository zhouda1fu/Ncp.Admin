<script lang="ts" setup>
import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createSupplier,
  getSupplier,
  updateSupplier,
} from '#/api/system/product';
import { $t } from '#/locales';

import { useSupplierFormSchema } from '../supplier-form-schema';

const emit = defineEmits(['success']);

export interface SupplierFormData {
  id?: string;
  fullName?: string;
  shortName?: string;
  contact?: string;
  phone?: string;
  email?: string;
  address?: string;
  remark?: string;
}

const formData = ref<SupplierFormData>({});

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useSupplierFormSchema()),
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
        await updateSupplier(formData.value.id, {
          fullName: String(data.fullName ?? '').trim(),
          shortName: String(data.shortName ?? '').trim(),
          contact: String(data.contact ?? '').trim(),
          phone: String(data.phone ?? '').trim(),
          email: String(data.email ?? '').trim() ?? '',
          address: String(data.address ?? '').trim() ?? '',
          remark: String(data.remark ?? '').trim() ?? '',
        });
      } else {
        await createSupplier({
          fullName: String(data.fullName ?? '').trim(),
          shortName: String(data.shortName ?? '').trim(),
          contact: String(data.contact ?? '').trim(),
          phone: String(data.phone ?? '').trim(),
          email: String(data.email ?? '').trim() ?? '',
          address: String(data.address ?? '').trim() ?? '',
          remark: String(data.remark ?? '').trim() ?? '',
        });
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  async onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<SupplierFormData>();
      formData.value = data ?? {};
      if (formData.value?.id) {
        const item = await getSupplier(formData.value.id);
        formApi.setValues({
          fullName: item.fullName ?? '',
          shortName: item.shortName ?? '',
          contact: item.contact ?? '',
          phone: item.phone ?? '',
          email: item.email ?? '',
          address: item.address ?? '',
          remark: item.remark ?? '',
        });
      } else {
        formApi.setValues({
          fullName: '',
          shortName: '',
          contact: '',
          phone: '',
          email: '',
          address: '',
          remark: '',
        });
      }
    }
  },
});
</script>

<template>
  <Drawer
    :title="
      formData?.id ? $t('product.supplierEdit') : $t('product.supplierCreate')
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
