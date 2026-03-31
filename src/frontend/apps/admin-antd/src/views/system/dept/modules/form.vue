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
        // 部门主管由用户管理页维护，新建传 0，编辑沿用当前部门的主管 ID
        const submitData: {
          name: string;
          remark?: string;
          parentId?: string;
          status: 0 | 1;
          managerId: string;
        } = {
          name: data.name,
          remark: data.remark || '',
          status: data.status ?? 1,
          managerId: formData.value?.id ? String(formData.value.managerId ?? '0') : '0',
          parentId:
            data.parentId === '0' || data.parentId === '' || !data.parentId
              ? undefined
              : data.parentId,
        };
        await (formData.value?.id
          ? updateDept(formData.value.id, submitData)
          : createDept(submitData));
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
        if (data.managerId != null) {
          data.managerId = String(data.managerId);
        }
        formData.value = data;
        // 仅设置表单中展示的字段（部门主管由用户管理页维护，不在此展示）
        formApi.setValues({
          name: data.name,
          parentId: data.parentId,
          status: data.status,
          remark: data.remark ?? '',
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
