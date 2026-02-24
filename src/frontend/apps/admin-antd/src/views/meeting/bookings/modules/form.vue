<script lang="ts" setup>
import type { MeetingApi } from '#/api/system/meeting';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createMeetingBooking } from '#/api/system/meeting';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<MeetingApi.MeetingBookingItem> & { id?: string }>();
const getTitle = $t('ui.actionTitle.create', [$t('meeting.booking.name')]);

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
        await createMeetingBooking({
          meetingRoomId: String(data.meetingRoomId),
          title: String(data.title),
          startAt: String(data.startAt),
          endAt: String(data.endAt),
        });
        drawerApi.close();
        emit('success');
      } finally {
        drawerApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<MeetingApi.MeetingBookingItem> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        meetingRoomId: data?.meetingRoomId ?? undefined,
        title: data?.title ?? '',
        startAt: data?.startAt ?? undefined,
        endAt: data?.endAt ?? undefined,
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
