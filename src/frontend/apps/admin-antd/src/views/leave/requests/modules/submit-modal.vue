<script lang="ts" setup>
import type { LeaveApi } from '#/api/system/leave';

import { useVbenModal } from '@vben/common-ui';

import { useVbenForm } from '#/adapter/form';
import { submitLeaveRequest } from '#/api/system/leave';
import { $t } from '#/locales';

export interface SubmitModalData {
  leaveRequestId: string;
  remark?: string;
  onSuccess?: () => void;
}

const emit = defineEmits(['success']);

const schema = [
  {
    component: 'Textarea',
    componentProps: {
      maxLength: 200,
      rows: 2,
      showCount: true,
      class: 'w-full',
    },
    fieldName: 'remark',
    label: $t('leave.request.submitRemark'),
  },
];

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema,
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    const data = modalApi.getData<SubmitModalData>();
    if (!data?.leaveRequestId) return;
    const values = await formApi.getValues();
    const remark = values?.remark ?? data?.remark ?? '';
    modalApi.lock();
    try {
      await submitLeaveRequest(data.leaveRequestId, remark);
      modalApi.close();
      data?.onSuccess?.();
      emit('success');
    } finally {
      modalApi.lock(false);
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<SubmitModalData>();
      if (data) {
        formApi.setValues({ remark: data.remark ?? '' });
      }
    }
  },
});
</script>

<template>
  <Modal :title="$t('leave.request.submit')">
    <Form class="mx-4" />
  </Modal>
</template>
