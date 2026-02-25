<script lang="ts" setup>
import type { TaskApi } from '#/api/system/task';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createTask, updateTask } from '#/api/system/task';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<TaskApi.TaskItem> & { id?: string }>();
const getTitle = computed(() =>
  formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('task.task.name')])
    : $t('ui.actionTitle.create', [$t('task.task.name')]),
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
          await updateTask(formData.value.id, {
            title: String(data.title),
            description: data.description ? String(data.description) : undefined,
            assigneeId: data.assigneeId != null ? Number(data.assigneeId) : undefined,
            dueDate: data.dueDate ? String(data.dueDate).slice(0, 10) : undefined,
          });
        } else {
          await createTask({
            projectId: String(data.projectId),
            title: String(data.title),
            description: data.description ? String(data.description) : undefined,
            assigneeId: data.assigneeId != null ? Number(data.assigneeId) : undefined,
            dueDate: data.dueDate ? String(data.dueDate).slice(0, 10) : undefined,
            sortOrder: Number(data.sortOrder) ?? 0,
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
      const data = drawerApi.getData<Partial<TaskApi.TaskItem> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        projectId: data?.projectId ?? undefined,
        title: data?.title ?? '',
        description: data?.description ?? '',
        assigneeId: data?.assigneeId ?? undefined,
        dueDate: data?.dueDate ?? undefined,
        sortOrder: data?.sortOrder ?? 0,
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
