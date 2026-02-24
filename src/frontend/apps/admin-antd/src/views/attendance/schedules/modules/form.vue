<script lang="ts" setup>
import type { AttendanceApi } from '#/api/system/attendance';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createSchedule, updateSchedule } from '#/api/system/attendance';
import { $t } from '#/locales';

import { useEditSchema, useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<AttendanceApi.ScheduleItem> & { id?: string }>();
const isEdit = computed(() => !!formData.value?.id);
const getTitle = computed(() =>
  isEdit.value
    ? $t('ui.actionTitle.edit', [$t('attendance.schedule.name')])
    : $t('ui.actionTitle.create', [$t('attendance.schedule.name')]),
);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => (isEdit.value ? useEditSchema() : useSchema())),
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
          await updateSchedule(formData.value.id, {
            startTime: String(data.startTime),
            endTime: String(data.endTime),
            shiftName: data.shiftName,
          });
        } else {
          await createSchedule({
            userId: Number(data.userId),
            workDate: String(data.workDate),
            startTime: String(data.startTime),
            endTime: String(data.endTime),
            shiftName: data.shiftName,
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
      const data = drawerApi.getData<Partial<AttendanceApi.ScheduleItem> & { id?: string }>();
      if (data) {
        formData.value = data;
        formApi.setValues({
          userId: data.userId,
          workDate: data.workDate,
          startTime: data.startTime,
          endTime: data.endTime,
          shiftName: data.shiftName,
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
