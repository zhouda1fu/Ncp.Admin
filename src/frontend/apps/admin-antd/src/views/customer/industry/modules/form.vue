<script lang="ts" setup>
import type { IndustryApi } from '#/api/system/industry';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createIndustry,
  getIndustryList,
  updateIndustry,
} from '#/api/system/industry';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<IndustryApi.IndustryItem> & { id?: string }>();
const industryList = ref<IndustryApi.IndustryItem[]>([]);

const parentOptions = computed(() =>
  industryList.value
    .filter((x) => x.id !== formData.value?.id)
    .map((x) => ({ label: x.name, value: x.id })),
);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useSchema(parentOptions.value)),
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
        await updateIndustry(formData.value.id, {
          name: String(data.name ?? ''),
          sortOrder: Number(data.sortOrder) ?? 0,
          remark: data.remark ? String(data.remark) : undefined,
        });
      } else {
        await createIndustry({
          name: String(data.name ?? ''),
          parentId: data.parentId ? String(data.parentId) : undefined,
          sortOrder: Number(data.sortOrder) ?? 0,
          remark: data.remark ? String(data.remark) : undefined,
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
        Partial<IndustryApi.IndustryItem> & { id?: string }
      >();
      formData.value = data;
      formApi.setValues({
        name: data?.name ?? '',
        parentId: data?.parentId ?? '',
        sortOrder: data?.sortOrder ?? 0,
        remark: data?.remark ?? '',
      });
    }
  },
});

getIndustryList().then((list) => {
  industryList.value = list;
});
</script>

<template>
  <Drawer
    :title="
      formData?.id
        ? $t('ui.actionTitle.edit', [$t('customer.industry')])
        : $t('ui.actionTitle.create', [$t('customer.industry')])
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
