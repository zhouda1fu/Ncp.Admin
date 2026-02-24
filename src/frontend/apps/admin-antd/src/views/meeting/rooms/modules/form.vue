<script lang="ts" setup>
import type { MeetingApi } from '#/api/system/meeting';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createMeetingRoom } from '#/api/system/meeting';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<MeetingApi.MeetingRoomItem> & { id?: string }>();
const getTitle = $t('ui.actionTitle.create', [$t('meeting.room.name')]);

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
        await createMeetingRoom({
          name: String(data.name),
          location: data.location ? String(data.location) : undefined,
          capacity: Number(data.capacity) || 1,
          equipment: data.equipment ? String(data.equipment) : undefined,
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
      const data = drawerApi.getData<Partial<MeetingApi.MeetingRoomItem> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        name: data?.name ?? '',
        location: data?.location ?? '',
        capacity: data?.capacity ?? 1,
        equipment: data?.equipment ?? '',
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
