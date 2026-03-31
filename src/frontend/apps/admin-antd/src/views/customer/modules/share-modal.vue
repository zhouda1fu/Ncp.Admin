<script lang="ts" setup>
import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Button, message, Select, Space } from 'ant-design-vue';

import { getCustomerShares, shareCustomer } from '#/api/system/customer';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

const emit = defineEmits<{ (e: 'success'): void }>();

type OpenPayload = { customerId: string; customerName?: string };

const customerId = ref<string>('');
const customerName = ref<string>('');

const userOptions = ref<Array<{ label: string; value: string }>>([]);
const loadingUsers = ref(false);
const submitting = ref(false);
const selectedUserIds = ref<string[]>([]);

const title = computed(() =>
  customerName.value
    ? `${$t('customer.shareTitle')}：${customerName.value}`
    : $t('customer.shareTitle'),
);

async function loadUsers() {
  if (userOptions.value.length > 0) return;
  loadingUsers.value = true;
  try {
    const res = await getUserList({ pageIndex: 1, pageSize: 500, isResigned: false });
    userOptions.value = (res.items ?? []).map((u: any) => ({
      label: u.realName || u.name || u.userId,
      value: String(u.userId),
    }));
  } finally {
    loadingUsers.value = false;
  }
}

async function loadShares() {
  if (!customerId.value) return;
  try {
    const res = await getCustomerShares(customerId.value);
    selectedUserIds.value = Array.isArray((res as any)?.sharedToUserIds)
      ? (res as any).sharedToUserIds.map((x: any) => String(x))
      : [];
  } catch {
    selectedUserIds.value = [];
  }
}

const [Modal, modalApi] = useVbenModal({
  async onOpenChange(isOpen) {
    if (!isOpen) return;
    const data = modalApi.getData<OpenPayload>();
    customerId.value = data?.customerId ?? '';
    customerName.value = data?.customerName ?? '';
    selectedUserIds.value = [];
    await Promise.all([loadUsers(), loadShares()]);
  },
  onClosed() {
    customerId.value = '';
    customerName.value = '';
    selectedUserIds.value = [];
  },
  showConfirmButton: false,
  showCancelButton: false,
});

async function onSubmit() {
  if (!customerId.value) return;
  if (submitting.value) return;
  submitting.value = true;
  try {
    await shareCustomer(customerId.value, selectedUserIds.value);
    message.success($t('common.success'));
    emit('success');
    modalApi.close();
  } finally {
    submitting.value = false;
  }
}

function open(payload: OpenPayload) {
  modalApi.setData(payload).open();
}

defineExpose({ open });
</script>

<template>
  <Modal :title="title" class="!w-[640px]">
    <div class="flex flex-col gap-3">
      <div class="text-sm text-muted-foreground">
        {{ $t('customer.shareHint') }}
      </div>
      <Select
        v-model:value="selectedUserIds"
        mode="multiple"
        allow-clear
        show-search
        class="w-full"
        :options="userOptions"
        :loading="loadingUsers"
        :placeholder="$t('customer.sharePlaceholder')"
        option-filter-prop="label"
      />
      <div class="flex justify-end">
        <Space>
          <Button @click="modalApi.close()">{{ $t('common.cancel') }}</Button>
          <Button type="primary" :loading="submitting" @click="onSubmit">
            {{ $t('common.confirm') }}
          </Button>
        </Space>
      </div>
    </div>
  </Modal>
</template>

