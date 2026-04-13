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
    <add-node v-model="nodeConfig.childNode" />
    <a-drawer
      v-model:open="drawer"
      title="发起人"
      :width="500"
      destroy-on-close
      get-container="body">
      <template #title>
        <div class="node-wrap-drawer__title">
          <label v-if="!isEditTitle" @click="editTitle">
            {{ form.nodeName }}
            <IconifyIcon icon="lucide:pencil" class="node-wrap-drawer__title-edit" />
          </label>
          <a-input
            v-else
            ref="nodeTitleRef"
            v-model:value="form.nodeName"
            allow-clear
            @blur="saveTitle"
            @press-enter="saveTitle" />
        </div>
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
      <template #footer>
        <a-button v-if="!viewOnly" type="primary" @click="save">保存</a-button>
        <a-button @click="drawer = false">取消</a-button>
      </template>
    </a-drawer>
  </div>
</template>

<script>
import { IconifyIcon, Plus } from '@vben/icons';
import { Alert, Button, Drawer, Form, Input, Tag } from 'ant-design-vue';
import addNode from './addNode.vue';

export default {
  name: 'PromoterNode',
  components: { addNode, AAlert: Alert, AButton: Button, ADrawer: Drawer, AForm: Form, AFormItem: Form.Item, AInput: Input, ATag: Tag, IconifyIcon, Plus },
  inject: ['select'],
  props: {
    modelValue: { type: Object, default: () => ({}) },
    viewOnly: { type: Boolean, default: false },
  },
  data() {
    return { nodeConfig: {}, drawer: false, isEditTitle: false, form: {} };
  },
  watch: { modelValue() { this.nodeConfig = this.modelValue; } },
  mounted() { this.nodeConfig = this.modelValue; },
  methods: {
    show() {
      this.form = JSON.parse(JSON.stringify(this.nodeConfig));
      this.isEditTitle = false;
      this.drawer = true;
    },
    editTitle() {
      this.isEditTitle = true;
      this.$nextTick(() => this.$refs.nodeTitleRef?.focus());
    },
    saveTitle() { this.isEditTitle = false; },
    selectHandle(type, data) { this.select(type, data); },
    delRole(index) { this.form.nodeAssigneeList.splice(index, 1); },
    save() {
      this.$emit('update:modelValue', this.form);
      this.drawer = false;
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
.node-wrap-drawer__title label { cursor: pointer; }
.node-wrap-drawer__title label:hover { border-bottom: 1px dashed hsl(var(--primary)); }
.node-wrap-drawer__title-edit { margin-left: 8px; color: hsl(var(--primary)); }
</style>
