<script lang="ts" setup>
import type { ProjectApi } from '#/api/system/project';
import type { CustomerApi } from '#/api/system/customer';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { useVbenForm, z } from '#/adapter/form';
import {
  addProjectContact,
  updateProjectContact,
} from '#/api/system/project';
import { $t } from '#/locales';

const props = defineProps<{
  projectId: string;
  customerContacts: CustomerApi.CustomerContactItem[];
}>();

const emit = defineEmits(['success']);

const customerContactOptions = computed(() =>
  props.customerContacts.map((c) => ({ label: c.name, value: c.id })),
);

const contactFormSchema = computed(() => [
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: customerContactOptions.value,
      placeholder: $t('task.project.customerContactPlaceholder'),
      allowClear: true,
    },
    fieldName: 'customerContactId',
    label: $t('task.project.customerContact'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('task.project.contactName') },
    fieldName: 'name',
    label: $t('task.project.contactName'),
    rules: z.string().min(1, $t('ui.formRules.required', [$t('task.project.contactName')])),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('task.project.contactPosition') },
    fieldName: 'position',
    label: $t('task.project.contactPosition'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('task.project.contactMobile') },
    fieldName: 'mobile',
    label: $t('task.project.contactMobile'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('task.project.contactOfficePhone') },
    fieldName: 'officePhone',
    label: $t('task.project.contactOfficePhone'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: 'QQ' },
    fieldName: 'qq',
    label: $t('task.project.contactQq'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('task.project.contactWechat') },
    fieldName: 'wechat',
    label: $t('task.project.contactWechat'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('task.project.contactEmail') },
    fieldName: 'email',
    label: $t('task.project.contactEmail'),
  },
  {
    component: 'Switch',
    componentProps: { class: 'w-full' },
    fieldName: 'isPrimary',
    label: $t('task.project.isPrimary'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', type: 'textarea', rows: 3 },
    fieldName: 'remark',
    label: $t('task.project.contactRemark'),
  },
]);

const formData = ref<Partial<ProjectApi.ProjectContactItem> | undefined>();
const isEdit = computed(() => !!formData.value?.id);
const drawerTitle = computed(() =>
  isEdit.value ? $t('task.project.editContact') : $t('task.project.addContact'),
);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: contactFormSchema as any,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4',
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    try {
      const data = await formApi.getValues();
      const payload: ProjectApi.ProjectContactParams = {
        customerContactId: data.customerContactId ? String(data.customerContactId) : undefined,
        name: String(data.name ?? ''),
        position: String(data.position ?? ''),
        mobile: String(data.mobile ?? ''),
        officePhone: String(data.officePhone ?? ''),
        qq: String(data.qq ?? ''),
        wechat: String(data.wechat ?? ''),
        email: String(data.email ?? ''),
        isPrimary: Boolean(data.isPrimary),
        remark: String(data.remark ?? ''),
      };
      if (isEdit.value && formData.value?.id) {
        await updateProjectContact(props.projectId, formData.value.id, payload);
      } else {
        await addProjectContact(props.projectId, payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<ProjectApi.ProjectContactItem>>();
      formData.value = data;
      const contact = data ?? {};
      formApi.setValues({
        customerContactId: contact.customerContactId ?? undefined,
        name: contact.name ?? '',
        position: contact.position ?? '',
        mobile: contact.mobile ?? '',
        officePhone: contact.officePhone ?? '',
        qq: contact.qq ?? '',
        wechat: contact.wechat ?? '',
        email: contact.email ?? '',
        isPrimary: contact.isPrimary ?? false,
        remark: contact.remark ?? '',
      });
    }
  },
});

function open(contact?: Partial<ProjectApi.ProjectContactItem>) {
  drawerApi.setData(contact ?? {}).open();
}

defineExpose({ open });
</script>

<template>
  <Drawer :title="drawerTitle">
    <Form class="mx-4" />
  </Drawer>
</template>
