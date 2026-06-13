<template>
  <div class="workflow-fragment-actions">
    <a-button @click="openCopyModal">
      <template #icon><IconifyIcon icon="lucide:copy-plus" /></template>
      复制流程片段
    </a-button>
    <a-button @click="applyCopiedFragment">
      <template #icon><IconifyIcon icon="lucide:clipboard-paste" /></template>
      套用流程片段
    </a-button>
    <div class="workflow-fragment-actions__tip">{{ tip }}</div>

    <a-modal
      v-model:open="copyModalOpen"
      title="复制流程片段"
      ok-text="复制"
      cancel-text="取消"
      :ok-button-props="{ disabled: checkedKeys.length === 0 }"
      @ok="confirmCopy"
    >
      <div class="workflow-fragment-copy-tip">
        {{ copyTip }}
      </div>
      <a-tree
        v-model:checked-keys="checkedKeys"
        :tree-data="treeData"
        :expanded-keys="expandedKeys"
        checkable
        block-node
      />
    </a-modal>
  </div>
</template>

<script>
import { IconifyIcon } from '@vben/icons';
import { Button, message, Modal, Tree } from 'ant-design-vue';

import {
  cloneWorkflowFragmentForInsert,
  collectWorkflowFragmentKeys,
  createWorkflowFragment,
  createWorkflowFragmentTree,
} from './workflow-fragment-utils';

export default {
  name: 'WorkflowFragmentCopyActions',
  components: {
    AButton: Button,
    AModal: Modal,
    ATree: Tree,
    IconifyIcon,
  },
  inject: ['copyWorkflowFragment', 'getWorkflowFragmentClipboard'],
  props: {
    node: { type: Object, default: () => ({}) },
    fragmentNode: { type: Object, default: null },
    tip: {
      type: String,
      default: '复制当前节点及其后续流程；套用时插入到当前节点和下一个节点之间。',
    },
    copyTip: {
      type: String,
      default: '默认选中当前节点及其后续流程，可取消不需要复制的节点。',
    },
  },
  emits: ['apply'],
  data() {
    return {
      checkedKeys: [],
      copyModalOpen: false,
      expandedKeys: [],
      treeData: [],
    };
  },
  methods: {
    openCopyModal() {
      const sourceNode = this.fragmentNode || this.node;
      if (!sourceNode?.nodeKey) {
        message.warning('当前节点不能复制流程片段');
        return;
      }

      this.treeData = createWorkflowFragmentTree(sourceNode);
      this.checkedKeys = collectWorkflowFragmentKeys(sourceNode);
      this.expandedKeys = [...this.checkedKeys];
      this.copyModalOpen = true;
    },
    confirmCopy() {
      const sourceNode = this.fragmentNode || this.node;
      const fragment = createWorkflowFragment(sourceNode, this.checkedKeys);
      if (!fragment) {
        message.warning('请选择要复制的流程节点');
        return;
      }

      this.copyWorkflowFragment?.({
        label: this.node.nodeName || sourceNode.nodeName || '流程片段',
        fragment,
      });
      this.copyModalOpen = false;
      message.success('已复制流程片段');
    },
    applyCopiedFragment() {
      const clipboard = this.getWorkflowFragmentClipboard?.();
      if (!clipboard?.fragment) {
        message.warning('请先复制一个流程片段');
        return;
      }

      const inserted = cloneWorkflowFragmentForInsert(clipboard.fragment, this.node.childNode);
      if (!inserted) {
        message.warning('复制的流程片段为空');
        return;
      }

      this.$emit('apply', inserted);
      message.success('已套用流程片段');
    },
  },
};
</script>

<style scoped>
.workflow-fragment-actions { display: flex; flex-wrap: wrap; gap: 8px; }
.workflow-fragment-actions__tip,
.workflow-fragment-copy-tip {
  flex-basis: 100%;
  color: hsl(var(--muted-foreground));
  font-size: 12px;
  line-height: 1.5;
}
.workflow-fragment-copy-tip { margin-bottom: 10px; }
</style>
