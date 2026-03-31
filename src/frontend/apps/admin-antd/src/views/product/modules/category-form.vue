<script lang="ts" setup>
import type { ProductCategoryTreeItem } from '#/api/system/product';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createProductCategory,
  getProductCategory,
  getProductCategoryTree,
  updateProductCategory,
} from '#/api/system/product';
import { $t } from '#/locales';

import { useCategoryFormSchema } from '../category-form-schema';

const emit = defineEmits(['success']);

export interface CategoryFormData {
  id?: string;
  name?: string;
  remark?: string;
  parentId?: string | null;
  sortOrder?: number;
  visible?: boolean;
  isDiscount?: boolean;
}

const formData = ref<CategoryFormData>({});
const treeData = ref<ProductCategoryTreeItem[]>([]);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useCategoryFormSchema(treeData.value)),
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
        await updateProductCategory(formData.value.id, {
          name: String(data.name ?? ''),
          remark: String(data.remark ?? ''),
          parentId: data.parentId != null && data.parentId !== '' ? String(data.parentId) : null,
          sortOrder: Number(data.sortOrder) ?? 0,
          visible: Boolean(data.visible),
          isDiscount: Boolean(data.isDiscount),
        });
      } else {
        await createProductCategory({
          name: String(data.name ?? ''),
          remark: String(data.remark ?? ''),
          parentId: data.parentId != null && data.parentId !== '' ? String(data.parentId) : null,
          sortOrder: Number(data.sortOrder) ?? 0,
          visible: data.visible !== false,
          isDiscount: Boolean(data.isDiscount),
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
      const data = drawerApi.getData<CategoryFormData>();
      formData.value = data ?? {};
      treeData.value = await getProductCategoryTree(true, true);
      if (formData.value?.id) {
        const item = await getProductCategory(formData.value.id);
        formApi.setValues({
          name: item.name,
          remark: item.remark ?? '',
          parentId: item.parentId ?? undefined,
          sortOrder: item.sortOrder ?? 0,
          visible: item.visible ?? true,
          isDiscount: item.isDiscount ?? false,
        });
      } else {
        formApi.setValues({
          name: '',
          remark: '',
          parentId: data?.parentId ?? undefined,
          sortOrder: 0,
          visible: true,
          isDiscount: false,
        });
      }
    }
  },
});
</script>

<template>
  <Drawer
    :title="
      formData?.id ? $t('product.categoryEdit') : $t('product.categoryAdd')
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
