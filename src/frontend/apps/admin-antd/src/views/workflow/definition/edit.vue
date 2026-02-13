<script lang="ts" setup>
import type { WorkflowApi } from '#/api/system/workflow';

import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { ArrowLeft } from '@vben/icons';

import { Button, message, Steps } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import {
  createDefinition,
  getDefinition,
  updateDefinition,
} from '#/api/system/workflow';
import { $t } from '#/locales';

import { useFormSchema } from './data';
import NodeDesigner from './modules/node-designer.vue';

const route = useRoute();
const router = useRouter();

const id = computed(() => route.params.id as string | undefined);
const isNew = computed(() => !id.value);

const currentStep = ref(0);
const formData = ref<WorkflowApi.WorkflowDefinition>();
const nodes = ref<WorkflowApi.WorkflowNode[]>([]);

const [Form, formApi] = useVbenForm({
  schema: useFormSchema(),
  showDefaultActions: false,
});

const isPublished = computed(() => formData.value?.status === 1);

async function loadDefinition() {
  if (!id.value) return;
  try {
    const detail = await getDefinition(id.value);
    formData.value = detail;
    nodes.value = detail.nodes ? [...detail.nodes] : [];
    formApi.setValues({
      name: detail.name,
      description: detail.description || '',
      category: detail.category || '',
    });
  } catch {
    message.error($t('ui.actionMessage.loadFailed'));
  }
}

watch(
  id,
  (v) => {
    if (v) loadDefinition();
    else {
      formData.value = undefined;
      nodes.value = [];
      formApi.resetForm();
      currentStep.value = 0;
    }
  },
  { immediate: true },
);

function goBack() {
  router.push('/workflow/definitions');
}

async function onStep1Next() {
  const { valid } = await formApi.validate();
  if (!valid) return;
  if (isNew.value) {
    currentStep.value = 1;
    return;
  }
  currentStep.value = 1;
}

async function onSave() {
  const values = await formApi.getValues();
  const name = values.name as string;
  const description = (values.description as string) || '';
  const category = (values.category as string) || '';
  try {
    if (id.value) {
      await updateDefinition({
        id: id.value,
        name,
        description,
        category,
        definitionJson: '{}',
        nodes: nodes.value,
      });
      message.success($t('ui.actionMessage.updateSuccess'));
    } else {
      await createDefinition({
        name,
        description,
        category,
        definitionJson: '{}',
        nodes: nodes.value,
      });
      message.success($t('ui.actionMessage.createSuccess'));
    }
    goBack();
  } catch {
    // error handled by request
  }
}

</script>

<template>
  <Page auto-content-height content-class="flex flex-col">
    <div class="mb-4 flex items-center gap-2">
      <Button type="text" @click="goBack">
        <ArrowLeft class="size-4" />
      </Button>
      <span class="text-lg font-medium">
        {{
          isNew
            ? $t('common.create', [$t('system.workflow.definition.name')])
            : $t('common.edit', [$t('system.workflow.definition.name')])
        }}
      </span>
    </div>

    <div class="workflow-edit-content mx-auto w-full max-w-2xl flex-1">
      <Steps v-model:current="currentStep" class="mb-4">
        <Steps.Step :title="$t('system.workflow.definition.stepBasicInfo')" />
        <Steps.Step :title="$t('system.workflow.definition.stepDesign')" />
      </Steps>
      <div v-show="currentStep === 0" class="workflow-step-body">
        <div class="workflow-form-wrap">
          <Form />
          <div class="mt-4 flex justify-center gap-2">
            <Button type="primary" @click="onStep1Next">
              {{ $t('system.workflow.definition.nextStep') }}
            </Button>
            <Button @click="goBack">{{ $t('system.workflow.definition.cancel') }}</Button>
          </div>
        </div>
      </div>
      <div v-show="currentStep === 1" class="workflow-step-body flex flex-col">
        <NodeDesigner v-model="nodes" :disabled="isPublished" />
        <div class="mt-4 flex justify-center gap-2">
          <Button @click="currentStep = 0">{{ $t('system.workflow.definition.prevStep') }}</Button>
          <Button type="primary" @click="onSave">
            {{ $t('system.workflow.definition.save') }}
          </Button>
          <Button @click="goBack">{{ $t('system.workflow.definition.cancel') }}</Button>
        </div>
      </div>
    </div>
  </Page>
</template>

<style scoped>
.workflow-edit-content {
  padding-top: 0.25rem;
}
.workflow-step-body {
  padding-top: 0.5rem;
}
.workflow-form-wrap {
  max-width: 28rem;
  margin-left: auto;
  margin-right: auto;
}
.workflow-form-wrap :deep(.ant-form-item) {
  margin-bottom: 1rem;
}
</style>
