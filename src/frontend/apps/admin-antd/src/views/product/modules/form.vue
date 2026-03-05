<script lang="ts" setup>
import type { ProductApi } from '#/api/system/product';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createProduct,
  getProduct,
  updateProduct,
} from '#/api/system/product';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<ProductApi.ProductItem> & { id?: string }>();

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
        await updateProduct(formData.value.id, {
          name: String(data.name ?? ''),
          code: String(data.code ?? ''),
          model: String(data.model ?? ''),
          unit: String(data.unit ?? ''),
        });
      } else {
        await createProduct({
          name: String(data.name ?? ''),
          code: String(data.code ?? ''),
          model: String(data.model ?? ''),
          unit: String(data.unit ?? ''),
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
      const data = drawerApi.getData<
        Partial<ProductApi.ProductItem> & { id?: string }
      >();
      formData.value = data;
      if (data?.id) {
        const detail = await getProduct(data.id);
        formApi.setValues({
          name: detail?.name ?? '',
          code: detail?.code ?? '',
          model: detail?.model ?? '',
          unit: detail?.unit ?? '',
        });
      } else {
        formApi.setValues({
          name: data?.name ?? '',
          code: data?.code ?? '',
          model: data?.model ?? '',
          unit: data?.unit ?? '',
        });
      }
    }
  },
});
</script>

<template>
  <Drawer
    :title="
      formData?.id
        ? $t('product.edit')
        : $t('product.create')
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
