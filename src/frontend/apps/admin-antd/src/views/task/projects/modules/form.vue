<script lang="ts" setup>
import type { ProjectApi } from '#/api/system/project';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createProject, updateProject } from '#/api/system/project';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<ProjectApi.ProjectItem> & { id?: string }>();
const getTitle = computed(() =>
  formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('task.project.name')])
    : $t('ui.actionTitle.create', [$t('task.project.name')]),
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
        if (formData.value?.id) {
          await updateProject(formData.value.id, {
            name: String(data.name),
            description: data.description ? String(data.description) : undefined,
          });
        } else {
          await createProject({
            name: String(data.name),
            description: data.description ? String(data.description) : undefined,
          });
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
      const data = drawerApi.getData<Partial<ProjectApi.ProjectItem> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        name: data?.name ?? '',
        description: data?.description ?? '',
      });
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
