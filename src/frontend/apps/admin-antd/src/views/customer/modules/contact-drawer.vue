<script lang="ts" setup>
import type { CustomerApi } from '#/api/system/customer';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm, z } from '#/adapter/form';
import { addCustomerContact, updateCustomerContact } from '#/api/system/customer';
import { $t } from '#/locales';

const props = defineProps<{ customerId: string }>();
const emit = defineEmits(['success']);

const genderOptions = () => [
  { label: $t('customer.genderMale'), value: 1 },
  { label: $t('customer.genderFemale'), value: 2 },
  { label: $t('customer.genderUnknown'), value: 0 },
];

const contactFormSchema = computed(() => [
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactNamePlaceholder') },
    fieldName: 'name',
    label: $t('customer.contactName'),
    rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.contactName')])),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactTypePlaceholder') },
    fieldName: 'contactType',
    label: $t('customer.contactType'),
  },
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: genderOptions(),
      placeholder: $t('customer.contactGenderPlaceholder'),
    },
    fieldName: 'gender',
    label: $t('customer.contactGender'),
    rules: z.number({
      required_error: $t('ui.formRules.required', [$t('customer.contactGender')]),
    }),
  },
  {
    component: 'DatePicker',
    componentProps: {
      class: 'w-full',
      placeholder: $t('customer.contactBirthdayPlaceholder'),
      valueFormat: 'YYYY-MM-DD',
    },
    fieldName: 'birthday',
    label: $t('customer.contactBirthday'),
    rules: z.any().refine(
      (val) => val != null && String(val).trim() !== '',
      $t('ui.formRules.required', [$t('customer.contactBirthday')]),
    ),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactPositionPlaceholder') },
    fieldName: 'position',
    label: $t('customer.contactPosition'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactMobilePlaceholder') },
    fieldName: 'mobile',
    label: $t('customer.contactMobile'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactPhonePlaceholder') },
    fieldName: 'phone',
    label: $t('customer.contactPhone'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactEmailPlaceholder') },
    fieldName: 'email',
    label: $t('customer.contactEmail'),
  },
  {
    component: 'Switch',
    componentProps: {
      checkedChildren: $t('common.yes'),
      unCheckedChildren: $t('common.no'),
    },
    fieldName: 'isPrimary',
    label: $t('customer.isPrimary'),
  },
]);

const formData = ref<Partial<CustomerApi.CustomerContactItem> | undefined>();
const isEdit = computed(() => !!formData.value?.id);
const drawerTitle = computed(() =>
  isEdit.value ? $t('customer.editContactTitle') : $t('customer.addContactTitle'),
);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: contactFormSchema as any,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 gap-y-4',
});

function resetForm() {
  formApi.resetForm();
  formApi.setValues(formData.value ?? {});
}

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    try {
      const data = await formApi.getValues();
      const b = Array.isArray(data.birthday) ? data.birthday[0] : data.birthday;
      const birthdayVal =
        typeof b === 'string' ? b : (b as { format?: (f: string) => string })?.format?.('YYYY-MM-DD') ?? String(b ?? '');
      const payload = {
        name: String(data.name ?? ''),
        contactType: data.contactType != null ? String(data.contactType) : '',
        gender: Number(data.gender) as number,
        birthday: birthdayVal,
        position: String(data.position ?? ''),
        mobile: String(data.mobile ?? ''),
        phone: String(data.phone ?? ''),
        email: String(data.email ?? ''),
        isPrimary: Boolean(data.isPrimary),
      };
      if (isEdit.value && formData.value?.id) {
        await updateCustomerContact(props.customerId, formData.value.id, payload);
      } else {
        await addCustomerContact(props.customerId, payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<CustomerApi.CustomerContactItem>>();
      formData.value = data;
      const contact = data ?? {};
      const isPrimary = contact.isPrimary ?? (contact as Record<string, unknown>).IsPrimary ?? false;
      const primaryBool = isPrimary === true || isPrimary === 1 || isPrimary === 'true' || isPrimary === '1';
      formApi.setValues({
        name: contact.name ?? '',
        contactType: contact.contactType ?? '',
        gender: contact.gender ?? 0,
        birthday: contact.birthday ?? undefined,
        position: contact.position ?? '',
        mobile: contact.mobile ?? '',
        phone: contact.phone ?? '',
        email: contact.email ?? '',
        isPrimary: primaryBool,
      });
    }
  },
});

function open(contact?: Partial<CustomerApi.CustomerContactItem>) {
  drawerApi.setData(contact ?? {}).open();
}

defineExpose({ open });
</script>

<template>
  <Drawer :title="drawerTitle">
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
