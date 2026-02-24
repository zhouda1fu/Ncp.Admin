<script lang="ts" setup>
import type { AnnouncementApi } from '#/api/system/announcement';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createAnnouncement, updateAnnouncement } from '#/api/system/announcement';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<AnnouncementApi.AnnouncementItem> & { id?: string }>();
const getTitle = computed(() =>
  formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('announcement.name')])
    : $t('ui.actionTitle.create', [$t('announcement.name')]),
);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: useSchema(),
  showDefaultActions: false,
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (valid) {
      drawerApi.lock();
      const data = await formApi.getValues();
      try {
        const payload = { title: String(data.title), content: String(data.content) };
        if (formData.value?.id) {
          await updateAnnouncement(String(formData.value.id), payload);
        } else {
          await createAnnouncement(payload);
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
      const data = drawerApi.getData<Partial<AnnouncementApi.AnnouncementItem> & { id?: string }>();
      if (data) {
        formData.value = data;
        formApi.setValues({
          title: data.title ?? '',
          content: data.content ?? '',
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
    <Form />
    <template #footer>
      <Button @click="drawerApi.close()">{{ $t('common.cancel') }}</Button>
      <Button type="primary" @click="drawerApi.submit()">{{ $t('common.confirm') }}</Button>
    </template>
  </Drawer>
</template>
