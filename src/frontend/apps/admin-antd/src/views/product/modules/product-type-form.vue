<script lang="ts" setup>
import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createProductType,
  getProductType,
  updateProductType,
} from '#/api/system/product';
import { $t } from '#/locales';

import { useProductTypeFormSchema } from '../product-type-form-schema';

const emit = defineEmits(['success']);

export interface ProductTypeFormData {
  id?: string;
  name?: string;
  sortOrder?: number;
  visible?: boolean;
}

const formData = ref<ProductTypeFormData>({});

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useProductTypeFormSchema()),
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
        await updateProductType(formData.value.id, {
          name: String(data.name ?? '').trim(),
          sortOrder: Number(data.sortOrder ?? 0),
          visible: Boolean(data.visible ?? true),
        });
      } else {
        await createProductType({
          name: String(data.name ?? '').trim(),
          sortOrder: Number(data.sortOrder ?? 0),
          visible: Boolean(data.visible ?? true),
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
      const data = drawerApi.getData<ProductTypeFormData>();
      formData.value = data ?? {};
      if (formData.value?.id) {
        const item = await getProductType(formData.value.id);
        formApi.setValues({
          name: item.name ?? '',
          sortOrder: item.sortOrder ?? 0,
          visible: item.visible ?? true,
        });
      } else {
        formApi.setValues({
          name: '',
          sortOrder: 0,
          visible: true,
        });
      }
    }
  },
});
</script>

<template>
  <Drawer
    :title="
      formData?.id ? $t('product.productTypeEdit') : $t('product.productTypeAdd')
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
