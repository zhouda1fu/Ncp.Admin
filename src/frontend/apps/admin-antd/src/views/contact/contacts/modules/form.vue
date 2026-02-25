<script lang="ts" setup>
import type { ContactApi } from '#/api/system/contact';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createContact, updateContact } from '#/api/system/contact';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<Partial<ContactApi.ContactItem> & { id?: string }>();
const getTitle = computed(() =>
  formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('contact.contact.name')])
    : $t('ui.actionTitle.create', [$t('contact.contact.name')]),
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
          await updateContact(formData.value.id, {
            name: String(data.name),
            phone: data.phone ? String(data.phone) : undefined,
            email: data.email ? String(data.email) : undefined,
            company: data.company ? String(data.company) : undefined,
            groupId: data.groupId ? String(data.groupId) : undefined,
          });
        } else {
          await createContact({
            name: String(data.name),
            phone: data.phone ? String(data.phone) : undefined,
            email: data.email ? String(data.email) : undefined,
            company: data.company ? String(data.company) : undefined,
            groupId: data.groupId ? String(data.groupId) : undefined,
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
      const data = drawerApi.getData<Partial<ContactApi.ContactItem> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        name: data?.name ?? '',
        phone: data?.phone ?? '',
        email: data?.email ?? '',
        company: data?.company ?? '',
        groupId: data?.groupId ?? undefined,
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
