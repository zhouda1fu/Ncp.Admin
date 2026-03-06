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

const isInitializing = ref(false);

function fillByCustomerContactId(customerContactId?: string) {
  if (isInitializing.value) return;
  if (!customerContactId) return;
  const c = props.customerContacts.find((x) => x.id === customerContactId);
  if (!c) return;
  // 客户联系人信息回填到项目联系人表单（保持 isPrimary/remark 不被覆盖）
  formApi.setValues({
    customerContactId,
    name: c.name ?? '',
    position: c.position ?? '',
    mobile: c.mobile ?? '',
    officePhone: c.phone ?? '',
    email: c.email ?? '',
  });
}

const contactFormSchema = computed(() => [
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: customerContactOptions.value,
      placeholder: $t('task.project.customerContactPlaceholder'),
      allowClear: true,
      onChange: (val: unknown) => fillByCustomerContactId(val ? String(val) : undefined),
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
    componentProps: {
      checkedChildren: $t('common.yes'),
      unCheckedChildren: $t('common.no'),
    },
    fieldName: 'isPrimary',
    label: $t('task.project.isPrimary'),
    help: $t('task.project.isPrimaryHint'),
    formItemClass: 'md:col-span-2',
  },
  {
    component: 'Textarea',
    componentProps: { class: 'w-full', rows: 3 },
    fieldName: 'remark',
    label: $t('task.project.contactRemark'),
    formItemClass: 'md:col-span-2',
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
  wrapperClass: 'grid grid-cols-1 gap-y-5 md:grid-cols-2 md:gap-x-8 md:gap-y-5',
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
      isInitializing.value = true;
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
      isInitializing.value = false;
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
    <div class="px-4 pb-6">
      <Form />
    </div>
  </Drawer>
</template>
