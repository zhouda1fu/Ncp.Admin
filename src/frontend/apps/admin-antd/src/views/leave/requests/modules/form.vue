<script lang="ts" setup>
import type { LeaveApi } from '#/api/system/leave';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createLeaveRequest, updateLeaveRequest } from '#/api/system/leave';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<LeaveApi.LeaveRequestItem> & { id?: string }>();
const getTitle = computed(() =>
  formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('leave.request.name')])
    : $t('ui.actionTitle.create', [$t('leave.request.name')]),
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
          leaveType: Number(data.leaveType) as LeaveApi.LeaveType,
          startDate: String(data.startDate),
          endDate: String(data.endDate),
          days: Number(data.days) ?? 0,
          reason: data.reason ?? '',
        };
        if (formData.value?.id) {
          await updateLeaveRequest(String(formData.value.id), payload);
        } else {
          await createLeaveRequest(payload);
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
      const data = drawerApi.getData<Partial<LeaveApi.LeaveRequestItem> & { id?: string }>();
      if (data) {
        formData.value = data;
        formApi.setValues({
          leaveType: data.leaveType ?? 0,
          startDate: data.startDate ?? undefined,
          endDate: data.endDate ?? undefined,
          days: data.days ?? 0,
          reason: data.reason ?? '',
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
