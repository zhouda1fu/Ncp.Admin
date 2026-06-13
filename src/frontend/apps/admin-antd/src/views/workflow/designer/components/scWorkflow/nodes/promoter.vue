<template>
  <div class="node-wrap">
    <div class="node-wrap-box start-node" @click="show">
      <div class="title promoter-title">
        <IconifyIcon icon="lucide:user" class="icon" />
        <span>{{ nodeConfig.nodeName }}</span>
      </div>
      <div class="content">
        <span>{{ toText(nodeConfig) }}</span>
      </div>
    </div>
    <add-node
      :model-value="nodeConfig.childNode"
      @update:model-value="updateChildNode"
    />
    <workflow-node-config-drawer
      v-model:open="drawer"
      title="发起人"
      :width="500"
      :show-confirm-button="!viewOnly"
      @confirm="save">
      <template #title>
        <workflow-node-drawer-title v-model:name="form.nodeName" :disabled="viewOnly" />
      </template>
      <div class="drawer-body">
        <a-form layout="vertical">
          <a-form-item label="节点名称">
            <a-input
              v-model:value="form.nodeName"
              allow-clear
              placeholder="如：发起申请、提交审批"
            />
          </a-form-item>
          <a-form-item label="谁可以发起此审批">
            <a-button type="primary" @click="selectHandle(2, form.nodeAssigneeList)">
              <template #icon><Plus class="size-4" /></template>
              选择角色
            </a-button>
            <div class="tags-list">
              <a-tag
                v-for="(role, index) in form.nodeAssigneeList"
                :key="role.id"
                closable
                @close="delRole(index)">
                {{ role.name }}
              </a-tag>
            </div>
          </a-form-item>
          <a-alert
            v-if="form.nodeAssigneeList && form.nodeAssigneeList.length === 0"
            message="不指定则默认所有人都可发起此审批"
            type="info"
            show-icon />
        </a-form>
      </div>
    </workflow-node-config-drawer>
  </div>
</template>

<script>
import { IconifyIcon, Plus } from '@vben/icons';
import { Alert, Button, Form, Input, Tag } from 'ant-design-vue';
import addNode from './addNode.vue';
import WorkflowNodeConfigDrawer from '../WorkflowNodeConfigDrawer.vue';
import WorkflowNodeDrawerTitle from '../WorkflowNodeDrawerTitle.vue';

export default {
  name: 'PromoterNode',
  components: { addNode, AAlert: Alert, AButton: Button, AForm: Form, AFormItem: Form.Item, AInput: Input, ATag: Tag, IconifyIcon, Plus, WorkflowNodeConfigDrawer, WorkflowNodeDrawerTitle },
  inject: ['select'],
  props: {
    modelValue: { type: Object, default: () => ({}) },
    viewOnly: { type: Boolean, default: false },
  },
  data() {
    return { nodeConfig: {}, drawer: false, form: {} };
  },
  watch: { modelValue() { this.nodeConfig = this.modelValue; } },
  mounted() { this.nodeConfig = this.modelValue; },
  methods: {
    show() {
      this.form = JSON.parse(JSON.stringify(this.nodeConfig));
      this.normalizeForm();
      this.drawer = true;
    },
    selectHandle(type, data) {
      if (!Array.isArray(data)) {
        this.form.nodeAssigneeList = [];
        data = this.form.nodeAssigneeList;
      }
      this.select(type, data);
    },
    delRole(index) { this.form.nodeAssigneeList.splice(index, 1); },
    save() {
      this.normalizeForm();
      this.nodeConfig = this.form;
      this.$emit('update:modelValue', this.nodeConfig);
      this.drawer = false;
    },
    normalizeForm() {
      if (!Array.isArray(this.form.nodeAssigneeList)) this.form.nodeAssigneeList = [];
    },
    updateChildNode(childNode) {
      this.nodeConfig.childNode = childNode;
      this.$emit('update:modelValue', this.nodeConfig);
    },
    toText(nodeConfig) {
      if (nodeConfig.nodeAssigneeList?.length > 0)
        return nodeConfig.nodeAssigneeList.map((item) => item.name).join('、');
      return '所有人';
    },
  },
};
</script>

<style scoped>
.promoter-title { background: #576a95; }
.drawer-body { padding: 0 20px 20px; }
.tags-list { margin-top: 8px; }
</style>
