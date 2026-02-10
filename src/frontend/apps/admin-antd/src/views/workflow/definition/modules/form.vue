<script lang="ts" setup>
import type { WorkflowApi } from '#/api/system/workflow';

import { computed, nextTick, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Divider } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createDefinition,
  getDefinition,
  updateDefinition,
} from '#/api/system/workflow';
import { $t } from '#/locales';

import { useFormSchema } from '../data';
import NodeDesigner from './node-designer.vue';

const emits = defineEmits(['success']);

const formData = ref<WorkflowApi.WorkflowDefinition>();
const nodes = ref<WorkflowApi.WorkflowNode[]>([]);

const [Form, formApi] = useVbenForm({
  schema: useFormSchema(),
  showDefaultActions: false,
});

const id = ref<string>();
const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    modalApi.lock();
    try {
      if (id.value) {
        await updateDefinition({
          id: id.value,
          name: values.name,
          description: values.description || '',
          category: values.category || '',
          definitionJson: '{}',
          nodes: nodes.value,
        });
      } else {
        await createDefinition({
          name: values.name,
          description: values.description || '',
          category: values.category || '',
          definitionJson: '{}',
          nodes: nodes.value,
        });
      }
      emits('success');
      modalApi.close();
    } catch {
      modalApi.unlock();
    }
  },

  async onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<WorkflowApi.WorkflowDefinition>();
      formApi.resetForm();
      nodes.value = [];

      if (data && data.id) {
        id.value = data.id;
        // 加载完整定义（含节点）
        try {
          const detail = await getDefinition(data.id);
          formData.value = detail;
          nodes.value = detail.nodes || [];
          await nextTick();
          formApi.setValues({
            name: detail.name,
            description: detail.description || '',
            category: detail.category || '',
          });
        } catch {
          formData.value = data;
          await nextTick();
          formApi.setValues({
            name: data.name,
            description: data.description || '',
            category: data.category || '',
          });
        }
      } else {
        id.value = undefined;
        formData.value = undefined;
      }
    }
  },
});

const getModalTitle = computed(() => {
  return formData.value?.id
    ? $t('common.edit', [$t('system.workflow.definition.name')])
    : $t('common.create', [$t('system.workflow.definition.name')]);
});

const isPublished = computed(() => formData.value?.status === 1);
</script>
<template>
  <Modal :title="getModalTitle" class="w-[720px]">
    <Form class="mx-4" />
    <Divider>{{ $t('system.workflow.node.title') }}</Divider>
    <NodeDesigner v-model="nodes" :disabled="isPublished" />
  </Modal>
</template>
