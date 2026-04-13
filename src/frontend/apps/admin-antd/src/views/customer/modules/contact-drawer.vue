<script lang="ts" setup>
import type { CustomerApi } from '#/api/system/customer';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button, message } from 'ant-design-vue';

import { useVbenForm, z } from '#/adapter/form';
import { addCustomerContact, updateCustomerContact } from '#/api/system/customer';
import { $t } from '#/locales';
import {
  hasAtLeastOneContactChannel,
  isValidMobileIfPresent,
  isValidQqIfPresent,
} from '#/utils/customer-contact-validation';

const props = defineProps<{ customerId: string }>();
const emit = defineEmits(['success']);

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
    componentProps: { class: 'w-full', placeholder: $t('customer.contactPositionPlaceholder') },
    fieldName: 'position',
    label: $t('customer.contactPosition'),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: $t('customer.contactMobilePlaceholder') },
    fieldName: 'mobile',
    label: $t('customer.contactMobile'),
    rules: z.any().refine(isValidMobileIfPresent, $t('customer.contactMobileInvalid')),
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
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: 'QQ' },
    fieldName: 'qq',
    label: 'QQ',
    rules: z.any().refine(isValidQqIfPresent, $t('customer.contactQqInvalid')),
  },
  {
    component: 'Input',
    componentProps: { class: 'w-full', placeholder: '微信' },
    fieldName: 'wechat',
    label: '微信',
  },
  {
    component: 'Switch',
    componentProps: {
      checkedChildren: $t('common.yes'),
      unCheckedChildren: $t('common.no'),
    },
    fieldName: 'isWechatAdded',
    label: '微信添加',
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
    const data = await formApi.getValues();
    if (
      !hasAtLeastOneContactChannel(data.mobile, data.phone, data.qq, data.wechat)
    ) {
      message.warning($t('customer.contactChannelAtLeastOne'));
      return;
    }
    drawerApi.lock();
    try {
      const existing = formData.value;
      const birthdayVal = isEdit.value && existing?.birthday
        ? String(existing.birthday)
        : new Date().toISOString();
      const payload = {
        name: String(data.name ?? ''),
        contactType: isEdit.value ? String(existing?.contactType ?? '') : '',
        gender: isEdit.value ? Number(existing?.gender ?? 0) : 0,
        birthday: birthdayVal,
        position: String(data.position ?? ''),
        mobile: String(data.mobile ?? ''),
        phone: String(data.phone ?? ''),
        email: String(data.email ?? ''),
        qq: String(data.qq ?? ''),
        wechat: String(data.wechat ?? ''),
        isWechatAdded: Boolean(data.isWechatAdded),
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
        position: contact.position ?? '',
        mobile: contact.mobile ?? '',
        phone: contact.phone ?? '',
        email: contact.email ?? '',
        qq: (contact as any).qq ?? '',
        wechat: (contact as any).wechat ?? '',
        isWechatAdded: Boolean((contact as any).isWechatAdded ?? false),
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
