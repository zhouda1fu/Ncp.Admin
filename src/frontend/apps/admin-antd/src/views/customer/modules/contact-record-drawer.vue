<script lang="ts" setup>
import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm, z } from '#/adapter/form';
import { addCustomerContactRecord } from '#/api/system/customer';
import { $t } from '#/locales';

const props = defineProps<{ customerId: string }>();
const emit = defineEmits(['success']);

const recordTypeOptions = () => [
  { label: $t('customer.recordTypePhone'), value: '电话' },
  { label: $t('customer.recordTypeVisit'), value: '上门拜访' },
  { label: $t('customer.recordTypeWechat'), value: '微信' },
  { label: $t('customer.recordTypeOther'), value: '其他' },
];

const recordFormSchema = computed(() => [
  {
    component: 'DatePicker',
    componentProps: {
      class: 'w-full',
      showTime: true,
      valueFormat: 'YYYY-MM-DDTHH:mm:ss',
      placeholder: $t('customer.recordAt'),
    },
    fieldName: 'recordAt',
    label: $t('customer.recordAt'),
    rules: z.any().refine(
      (val) => val != null && String(val).trim() !== '',
      $t('ui.formRules.required', [$t('customer.recordAt')]),
    ),
  },
  {
    component: 'Select',
    componentProps: {
      class: 'w-full',
      options: recordTypeOptions(),
      placeholder: $t('customer.recordType'),
    },
    fieldName: 'recordType',
    label: $t('customer.recordType'),
    rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.recordType')])),
  },
  {
    component: 'Input',
    componentProps: {
      class: 'w-full',
      placeholder: $t('customer.recordContentPlaceholder'),
      type: 'textarea',
      rows: 4,
    },
    fieldName: 'content',
    label: $t('customer.recordContent'),
  },
]);

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: recordFormSchema as any,
  showDefaultActions: false,
  wrapperClass: 'grid-cols-1 gap-y-4',
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    drawerApi.lock();
    try {
      const data = await formApi.getValues();
      const recordAtVal = data.recordAt;
      const recordAtStr =
        typeof recordAtVal === 'string'
          ? recordAtVal
          : (recordAtVal as { format?: (f: string) => string })?.format?.('YYYY-MM-DDTHH:mm:ss') ?? new Date().toISOString();
      await addCustomerContactRecord(props.customerId, {
        recordAt: recordAtStr,
        recordType: String(data.recordType ?? ''),
        content: String(data.content ?? ''),
      });
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const now = new Date();
      const defaultRecordAt = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}T${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:00`;
      formApi.setValues({
        recordAt: defaultRecordAt,
        recordType: '电话',
        content: '',
      });
    }
  },
});

function open() {
  drawerApi.open();
}

defineExpose({ open });
</script>

<template>
  <Drawer :title="$t('customer.addRecordTitle')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <Button type="primary" danger @click="formApi.resetForm">
        {{ $t('common.reset') }}
      </Button>
    </template>
  </Drawer>
</template>
