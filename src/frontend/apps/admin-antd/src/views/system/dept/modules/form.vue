<script lang="ts" setup>
import type { SystemDeptApi } from '#/api/system/dept';

import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Button } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createDept, updateDept } from '#/api/system/dept';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<SystemDeptApi.SystemDept>();
const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('system.dept.name')])
    : $t('ui.actionTitle.create', [$t('system.dept.name')]);
});

// 编辑时从上级部门选项中排除当前部门及其子部门，避免选自己为上级
const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: useSchema(() => formData.value?.id),
  showDefaultActions: false,
});

function resetForm() {
  formApi.resetForm();
  if (formData.value) {
    formApi.setValues({
      name: formData.value.name,
      parentId: formData.value.parentId,
      status: formData.value.status,
      remark: formData.value.remark ?? '',
      responsibleUserIds: formData.value.responsibleUsers?.map((x) => String(x.userId)) ?? [],
      defaultResponsibleUserId: formData.value.responsibleUsers?.find((x) => x.isDefault)?.userId,
    });
  }
}

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (valid) {
      modalApi.lock();
      const data = await formApi.getValues();
      try {
        // 处理 parentId：如果是 '0' 或空字符串，设置为 undefined
        const submitData: {
          name: string;
          remark?: string;
          parentId?: string;
          status: 0 | 1;
          sortOrder?: number;
          responsibleUserIds?: string[];
          defaultResponsibleUserId?: string;
        } = {
          name: data.name,
          remark: data.remark || '',
          status: data.status ?? 1,
          responsibleUserIds: data.responsibleUserIds ?? [],
          defaultResponsibleUserId: data.defaultResponsibleUserId || undefined,
          parentId:
            data.parentId === '0' || data.parentId === '' || !data.parentId
              ? undefined
              : data.parentId,
        };
        if (formData.value?.id) {
          submitData.sortOrder = formData.value.sortOrder ?? 0;
          await updateDept(formData.value.id, submitData);
        } else {
          await createDept(submitData);
        }
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<SystemDeptApi.SystemDept>();
      if (data) {
        // 处理 parentId：如果是 '0' 或空字符串，设置为 undefined
        if (data.parentId === '0' || data.parentId === '') {
          data.parentId = undefined;
        }
        formData.value = data;
        formApi.setValues({
          name: data.name,
          parentId: data.parentId,
          status: data.status,
          remark: data.remark ?? '',
          responsibleUserIds: data.responsibleUsers?.map((x) => String(x.userId)) ?? [],
          defaultResponsibleUserId: data.responsibleUsers?.find((x) => x.isDefault)?.userId,
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
  <Modal :title="getTitle">
    <Form class="mx-4" />
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Modal>
</template>
