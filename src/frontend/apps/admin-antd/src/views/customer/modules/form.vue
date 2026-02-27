<script lang="ts" setup>
import type { CustomerApi } from '#/api/system/customer';

import { computed, onMounted, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createCustomer, updateCustomer } from '#/api/system/customer';
import { getCustomerSourceList } from '#/api/system/customerSource';
import { getIndustryList } from '#/api/system/industry';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const industryOptions = ref<{ label: string; value: string }[]>([]);
const customerSourceOptions = ref<{ label: string; value: string }[]>([]);
const formData = ref<Partial<CustomerApi.CustomerDetail> & { id?: string }>();

onMounted(() => {
  Promise.all([
    getIndustryList().then((res) => {
      const list = Array.isArray(res) ? res : (res as any)?.data ?? [];
      industryOptions.value = list.map((x: { id: string; name: string }) => ({ label: x.name, value: x.id }));
    }),
    getCustomerSourceList().then((list) => {
      customerSourceOptions.value = list.map((x) => ({ label: x.name, value: x.id }));
    }),
  ]);
});

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: computed(() => useSchema(industryOptions.value, customerSourceOptions.value)),
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
    const payload = {
      fullName: String(data.fullName ?? ''),
      shortName: data.shortName ? String(data.shortName) : undefined,
      customerSourceId: String(data.customerSourceId ?? ''),
      statusId: Number(data.statusId) ?? 0,
      ownerId: data.ownerId != null && data.ownerId !== '' ? Number(data.ownerId) : undefined,
      deptId: data.deptId != null && data.deptId !== '' ? Number(data.deptId) : undefined,
      mainContactName: data.mainContactName ? String(data.mainContactName) : undefined,
      mainContactPhone: data.mainContactPhone ? String(data.mainContactPhone) : undefined,
      wechatStatus: data.wechatStatus ? String(data.wechatStatus) : undefined,
      remark: data.remark ? String(data.remark) : undefined,
      isKeyAccount: Boolean(data.isKeyAccount),
      isHidden: Boolean(data.isHidden),
      industryIds: Array.isArray(data.industryIds) ? data.industryIds : undefined,
    };
    try {
      if (formData.value?.id) {
        await updateCustomer(formData.value.id, payload);
      } else {
        await createCustomer(payload);
      }
      drawerApi.close();
      emit('success');
    } finally {
      drawerApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<Partial<CustomerApi.CustomerDetail> & { id?: string }>();
      formData.value = data;
      formApi.setValues({
        fullName: data?.fullName ?? '',
        shortName: data?.shortName ?? '',
        customerSourceId: data?.customerSourceId ?? '',
        statusId: data?.statusId ?? 0,
        ownerId: data?.ownerId ?? '',
        deptId: data?.deptId ?? '',
        mainContactName: data?.mainContactName ?? '',
        mainContactPhone: data?.mainContactPhone ?? '',
        wechatStatus: data?.wechatStatus ?? '',
        remark: data?.remark ?? '',
        isKeyAccount: data?.isKeyAccount ?? false,
        isHidden: data?.isHidden ?? false,
        industryIds: data?.industryIds ?? [],
      });
    }
  },
});
</script>

<template>
  <Drawer :title="formData?.id ? $t('customer.edit') : $t('customer.create')">
    <Form class="mx-4" />
    <template #prepend-footer>
      <Button type="primary" danger @click="resetForm">{{ $t('common.reset') }}</Button>
    </template>
  </Drawer>
</template>
