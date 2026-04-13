<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { IconifyIcon } from '@vben/icons';
import { Button, Input, message, Select } from 'ant-design-vue';

import {
  createDefinition,
  getDefinition,
  publishDefinition,
  updateDefinition,
} from '#/api/system/workflow';
import { $t } from '#/locales';

import { useCategoryOptions } from '#/views/workflow/definition/data';

import WorkFlow from './components/workFlow.vue';

const route = useRoute();
const router = useRouter();
const id = computed(() => route.params.id as string | undefined);
const isEdit = computed(() => !!id.value);
const viewOnly = computed(() => route.query.view === '1');

const name = ref('');
const category = ref<string>('');
const description = ref('');
const nodeConfig = ref<Recordable<any> | null>(null);
const loading = ref(false);
const saving = ref(false);

const categoryOptions = useCategoryOptions();

async function loadDefinition() {
  if (!id.value) return;
  loading.value = true;
  try {
    const detail = await getDefinition(id.value);
    name.value = detail.name ?? '';
    category.value = detail.category ?? '';
    description.value = detail.description ?? '';
    if (detail.definitionJson) {
      try {
        nodeConfig.value = JSON.parse(detail.definitionJson);
      } catch {
        nodeConfig.value = null;
      }
    } else {
      nodeConfig.value = null;
    }
  } finally {
    loading.value = false;
  }
}

async function onSave() {
  if (!name.value?.trim()) {
    message.warning($t('system.workflow.definition.flowName') + '不能为空');
    return;
  }
  const definitionJson =
    nodeConfig.value != null ? JSON.stringify(nodeConfig.value) : '{}';
  saving.value = true;
  try {
    if (isEdit.value && id.value) {
      await updateDefinition({
        id: id.value,
        name: name.value.trim(),
        description: description.value?.trim() ?? '',
        category: category.value || 'CreateUser',
        definitionJson,
      });
      message.success('保存成功');
    } else {
      await createDefinition({
        name: name.value.trim(),
        description: description.value?.trim() ?? '',
        category: category.value || 'CreateUser',
        definitionJson,
      });
      message.success('创建成功');
      router.push('/workflow/definitions');
    }
  } finally {
    saving.value = false;
  }
}

async function onPublish() {
  if (!isEdit.value || !id.value) {
    message.warning('请先保存流程定义后再发布');
    return;
  }
  saving.value = true;
  try {
    await publishDefinition(id.value);
    message.success('发布成功');
    await loadDefinition();
  } finally {
    saving.value = false;
  }
}

function onBack() {
  router.push('/workflow/definitions');
}

function getDefaultNodeConfig() {
  return {
    nodeName: '发起人',
    nodeKey: 'root_' + Date.now(),
    type: 0,
    nodeAssigneeList: [],
    childNode: null,
  };
}

onMounted(() => {
  loadDefinition();
});
</script>

<template>
  <div
    id="flowlong-designer-root"
    class="workflow-designer-root flex h-full w-full flex-col"
  >
    <header
      class="workflow-designer-toolbar shrink-0 border-b border-[hsl(var(--border))] bg-[hsl(var(--card))] px-4 py-3 shadow-[0_1px_0_hsl(var(--border)_/_0.6)]"
    >
      <div class="mx-auto flex max-w-[1600px] flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div class="flex flex-wrap items-center gap-3">
          <div class="flex items-center gap-2">
            <span
              class="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-[hsl(var(--primary)_/_0.1)] text-[hsl(var(--primary))]"
            >
              <IconifyIcon icon="lucide:git-branch" class="size-5" />
            </span>
            <div>
              <h1 class="text-base font-semibold leading-tight text-[hsl(var(--foreground))]">
                {{ viewOnly ? '查看流程' : '流程设计器' }}
              </h1>
              <p class="text-xs text-[hsl(var(--muted-foreground))]">
                {{ viewOnly ? '只读浏览节点与连线' : '拖拽下方节点间的 + 添加审批、抄送与条件分支' }}
              </p>
            </div>
          </div>
        </div>

        <div class="flex min-w-0 flex-1 flex-wrap items-end gap-x-4 gap-y-3 lg:max-w-3xl">
          <div class="flex min-w-[140px] flex-1 flex-col gap-1">
            <span class="text-xs font-medium text-[hsl(var(--muted-foreground))]">流程名称</span>
            <Input
              v-model:value="name"
              placeholder="如：订单标准审批"
              class="w-full"
              allow-clear
              :disabled="viewOnly"
            />
          </div>
          <div class="flex w-full min-w-[120px] max-w-[200px] flex-col gap-1">
            <span class="text-xs font-medium text-[hsl(var(--muted-foreground))]">分类</span>
            <Select
              v-model:value="category"
              placeholder="选择分类"
              class="w-full"
              allow-clear
              :options="categoryOptions"
              :disabled="viewOnly"
            />
          </div>
          <div class="flex min-w-[180px] flex-[2] flex-col gap-1">
            <span class="text-xs font-medium text-[hsl(var(--muted-foreground))]">描述</span>
            <Input
              v-model:value="description"
              placeholder="选填，便于同事识别用途"
              class="w-full"
              allow-clear
              :disabled="viewOnly"
            />
          </div>
        </div>

        <div class="flex shrink-0 flex-wrap items-center gap-2 lg:justify-end">
          <template v-if="!viewOnly">
            <Button
              type="primary"
              class="min-w-[100px]"
              :loading="saving"
              @click="onSave"
            >
              {{ isEdit ? '保存' : '保存草稿' }}
            </Button>
            <Button
              v-if="isEdit"
              type="primary"
              ghost
              class="min-w-[88px]"
              :loading="saving"
              @click="onPublish"
            >
              发布
            </Button>
          </template>
          <Button @click="onBack">返回</Button>
        </div>
      </div>
    </header>

    <div class="workflow-designer-body min-h-0 flex-1 overflow-hidden p-3 sm:p-4">
      <WorkFlow
        v-if="!loading"
        v-model="nodeConfig"
        :category="category"
        :view-only="viewOnly"
        :initial-config="isEdit ? undefined : getDefaultNodeConfig()"
      />
    </div>
  </div>
</template>
