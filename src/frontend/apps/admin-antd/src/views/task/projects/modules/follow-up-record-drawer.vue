<script lang="ts" setup>
import type { ProjectApi } from '#/api/system/project';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { useVbenForm, z } from '#/adapter/form';
import {
  addProjectFollowUpRecord,
  updateProjectFollowUpRecord,
} from '#/api/system/project';
import { $t } from '#/locales';

const props = defineProps<{ projectId: string }>();
const emit = defineEmits(['success']);

const reminderIntervalOptions = [
  { label: $t('task.project.followUpReminderDays', ['1']), value: 1 },
  { label: $t('task.project.followUpReminderDays', ['3']), value: 3 },
  { label: $t('task.project.followUpReminderDays', ['7']), value: 7 },
  { label: $t('task.project.followUpReminderDays', ['15']), value: 15 },
  { label: $t('task.project.followUpReminderDays', ['30']), value: 30 },
  { label: $t('task.project.followUpReminderDays', ['0']), value: 0 },
];

const recordFormSchema = computed(() => [
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('task.project.followUpTitle') },
    fieldName: 'title',
    label: $t('task.project.followUpTitle'),
    rules: z.string().min(1, $t('ui.formRules.required', [$t('task.project.followUpTitle')])),
  },
  {
    component: 'DatePicker',
    componentProps: {
      class: 'w-full',
      placeholder: $t('task.project.followUpVisitDate'),
      valueFormat: 'YYYY-MM-DD',
    },
    fieldName: 'visitDate',
    label: $t('task.project.followUpVisitDate'),
  },
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: reminderIntervalOptions,
      placeholder: $t('task.project.followUpReminderFrequency'),
    },
    fieldName: 'reminderIntervalDays',
    label: $t('task.project.followUpReminderFrequency'),
  },
  {
    component: 'Textarea',
    componentProps: { class: 'w-full', rows: 6 },
    fieldName: 'content',
    label: $t('task.project.followUpContent'),
  },
]);

const formData = ref<Partial<ProjectApi.ProjectFollowUpRecordItem> | undefined>();
const isEdit = computed(() => !!formData.value?.id);
const drawerTitle = computed(() =>
  isEdit.value ? $t('task.project.editFollowUpRecord') : $t('task.project.addFollowUpRecord'),
);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: recordFormSchema as any,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 gap-y-4',
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    try {
      const data = await formApi.getValues();
      const payload: ProjectApi.ProjectFollowUpRecordParams = {
        title: String(data.title ?? ''),
        visitDate: data.visitDate ? String(data.visitDate) : undefined,
        reminderIntervalDays: Number(data.reminderIntervalDays) ?? 0,
        content: String(data.content ?? ''),
      };
      if (isEdit.value && formData.value?.id) {
        await updateProjectFollowUpRecord(props.projectId, formData.value.id, payload);
      } else {
        await addProjectFollowUpRecord(props.projectId, payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<ProjectApi.ProjectFollowUpRecordItem>>();
      formData.value = data;
      const record = data ?? {};
      formApi.setValues({
        title: record.title ?? '',
        visitDate: record.visitDate ?? undefined,
        reminderIntervalDays: record.reminderIntervalDays ?? 0,
        content: record.content ?? '',
      });
    }
  },
});

function open(record?: Partial<ProjectApi.ProjectFollowUpRecordItem>) {
  drawerApi.setData(record ?? {}).open();
}

defineExpose({ open });
</script>

<template>
  <Drawer :title="drawerTitle">
    <Form class="mx-4" />
  </Drawer>
</template>
