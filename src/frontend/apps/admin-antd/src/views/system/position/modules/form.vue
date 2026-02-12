<script lang="ts" setup>
import type { SystemPositionApi } from '#/api/system/position';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createPosition, updatePosition } from '#/api/system/position';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<SystemPositionApi.Position> & { id?: string }>();
const getTitle = computed(() =>
  formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('system.position.name')])
    : $t('ui.actionTitle.create', [$t('system.position.name')]),
);

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
    if (valid) {
      drawerApi.lock();
      const data = await formApi.getValues();
      try {
        const payload = {
          name: data.name,
          code: data.code,
          description: data.description ?? '',
          deptId: String(data.deptId),
          sortOrder: Number(data.sortOrder) ?? 0,
          status: Number(data.status) ?? 1,
        };
        if (formData.value?.id) {
          await updatePosition({
            id: String(formData.value.id),
            ...payload,
          });
        } else {
          await createPosition(payload);
        }
        drawerApi.close();
        emit('success');
      } finally {
        drawerApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<SystemPositionApi.Position> & { id?: string }>();
      if (data) {
        formData.value = data;
        formApi.setValues({
          name: data.name,
          code: data.code,
          description: data.description ?? '',
          deptId: data.deptId ?? undefined,
          sortOrder: data.sortOrder ?? 0,
          status: data.status ?? 1,
        });
      } else {
        formData.value = undefined;
        formApi.resetForm();
      }
    }
  },
});
</script>

<template>
  <Drawer :title="getTitle">
    <Form class="mx-4" />
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Drawer>
</template>
