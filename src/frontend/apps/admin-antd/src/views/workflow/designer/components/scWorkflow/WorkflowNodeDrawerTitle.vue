<script lang="ts" setup>
import { computed, nextTick, ref } from 'vue';

import { IconifyIcon } from '@vben/icons';

import { Input } from 'ant-design-vue';

const props = defineProps<{
  /** 是否禁用标题编辑。 */
  disabled?: boolean;
  /** 节点标题。 */
  name?: string;
}>();

const emit = defineEmits<{
  'update:name': [value: string];
}>();

const editing = ref(false);
const inputRef = ref<InstanceType<typeof Input> | null>(null);

// 标题通过 v-model:name 回写到节点，抽屉标题和画布节点共用同一份名称。
const title = computed({
  get: () => props.name ?? '',
  set: (value) => emit('update:name', value),
});

/** 进入标题编辑状态，并在输入框渲染后自动聚焦。 */
function beginEdit() {
  if (props.disabled) return;
  editing.value = true;
  nextTick(() => {
    const input = inputRef.value as unknown as { focus?: () => void };
    input?.focus?.();
  });
}

/** 退出标题编辑状态，名称已由 computed setter 实时同步。 */
function finishEdit() {
  editing.value = false;
}
</script>

<template>
  <div class="node-wrap-drawer__title">
    <label v-if="!editing" @click="beginEdit">
      {{ title }}
      <IconifyIcon v-if="!disabled" icon="lucide:pencil" class="node-wrap-drawer__title-edit" />
    </label>
    <Input
      v-else
      ref="inputRef"
      v-model:value="title"
      allow-clear
      @blur="finishEdit"
      @press-enter="finishEdit"
    />
  </div>
</template>

<style scoped>
.node-wrap-drawer__title label {
  cursor: pointer;
}

.node-wrap-drawer__title label:hover {
  border-bottom: 1px dashed hsl(var(--primary));
}

.node-wrap-drawer__title-edit {
  margin-left: 8px;
  color: hsl(var(--primary));
}
</style>
