<script lang="ts" setup>
import { computed, watch } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

const props = withDefaults(
  defineProps<{
    /** 确认按钮文案。 */
    confirmText?: string;
    /** 抽屉是否打开，由节点配置组件通过 v-model:open 控制。 */
    open?: boolean;
    /** 是否展示底部确认按钮，查看模式下会隐藏。 */
    showConfirmButton?: boolean;
    /** 默认标题；传入 title slot 时由 slot 接管。 */
    title?: string;
    /** 抽屉目标宽度，当前用于区分标准和宽版样式。 */
    width?: number | string;
  }>(),
  {
    confirmText: '保存',
    open: false,
    showConfirmButton: true,
    title: '',
    width: 500,
  },
);

const emit = defineEmits<{
  confirm: [];
  'update:open': [value: boolean];
}>();

// 统一封装 Vben 抽屉，保留旧节点配置组件的 v-model:open 使用方式。
const [Drawer, drawerApi] = useVbenDrawer({
  onConfirm() {
    emit('confirm');
  },
  onOpenChange(isOpen) {
    if (props.open !== isOpen) {
      emit('update:open', isOpen);
    }
  },
});

// 当前抽屉只提供标准宽度和宽版两档，避免各节点重复维护样式。
const drawerClass = computed(() =>
  Number(props.width) >= 600
    ? 'workflow-node-config-drawer workflow-node-config-drawer--wide'
    : 'workflow-node-config-drawer',
);

watch(
  () => props.open,
  (isOpen) => {
    // 外层仍通过 open 控制显隐，这里负责同步到 Vben Drawer 的命令式 API。
    if (isOpen) {
      drawerApi.open();
    } else {
      drawerApi.close();
    }
  },
  { immediate: true },
);
</script>

<template>
  <Drawer
    :title="title"
    :class="drawerClass"
    :confirm-text="confirmText"
    :show-confirm-button="showConfirmButton"
    destroy-on-close
  >
    <template v-if="$slots.title" #title>
      <slot name="title" />
    </template>
    <slot />
  </Drawer>
</template>

<style>
.workflow-node-config-drawer {
  width: min(500px, calc(100vw - 32px)) !important;
}

.workflow-node-config-drawer--wide {
  width: min(600px, calc(100vw - 32px)) !important;
}
</style>
