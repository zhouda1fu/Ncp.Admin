<script lang="ts" setup>
import type { LeaveApi } from '#/api/system/leave';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { setLeaveBalance } from '#/api/system/leave';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<LeaveApi.LeaveBalanceItem> & { id?: string }>();
const getTitle = computed(() => $t('leave.balance.setBalance'));

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
      await setLeaveBalance({
        userId: String(data.userId),
        year: Number(data.year),
        leaveType: Number(data.leaveType) as LeaveApi.LeaveType,
        totalDays: Number(data.totalDays) ?? 0,
      });
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<LeaveApi.LeaveBalanceItem> & { id?: string }>();
      if (data && (data.userId || data.id)) {
        formData.value = data;
        formApi.setValues({
          userId: data.userId ?? '',
          year: data.year ?? new Date().getFullYear(),
          leaveType: data.leaveType ?? 0,
          totalDays: data.totalDays ?? 0,
        });
      } else {
        formData.value = {};
        formApi.setValues({
          userId: undefined,
          year: new Date().getFullYear(),
          leaveType: 0,
          totalDays: 0,
        });
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
