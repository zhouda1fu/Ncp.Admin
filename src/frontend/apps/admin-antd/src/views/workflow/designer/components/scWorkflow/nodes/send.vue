<template>
  <div class="node-wrap">
    <div class="node-wrap-box" @click="show">
      <div class="title title--primary">
        <IconifyIcon icon="lucide:send" class="icon" />
        <span>{{ nodeConfig.nodeName }}</span>
        <X class="close" @click.stop="delNode()" />
      </div>
      <div class="content">
        <span v-if="toText(nodeConfig)">{{ toText(nodeConfig) }}</span>
        <span v-else class="placeholder">请选择人员</span>
      </div>
    </div>
    <add-node v-model="nodeConfig.childNode" />
    <a-drawer
      v-model:open="drawer"
      title="抄送人设置"
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
              placeholder="如：抄送财务、抄送人事"
            />
          </a-form-item>
          <a-form-item label="选择要抄送的人员">
            <a-button type="primary" @click="selectHandle(1, form.nodeAssigneeList)">
              <template #icon><Plus class="size-4" /></template>
              选择人员
            </a-button>
            <div class="tags-list">
              <a-tag
                v-for="(user, index) in form.nodeAssigneeList"
                :key="user.id"
                closable
                @close="delUser(index)">
                {{ user.name }}
              </a-tag>
            </div>
          </a-form-item>
          <a-form-item>
            <a-checkbox v-model:checked="form.userSelectFlag">允许发起人自选抄送人</a-checkbox>
          </a-form-item>
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
import { Plus, X } from '@vben/icons';
import { Icon as IconifyIcon } from '@iconify/vue';
import { Button, Checkbox, Drawer, Form, Input, Tag } from 'ant-design-vue';
import addNode from './addNode.vue';

export default {
  name: 'SendNode',
  components: { addNode, AButton: Button, ACheckbox: Checkbox, ADrawer: Drawer, AForm: Form, AFormItem: Form.Item, AInput: Input, ATag: Tag, IconifyIcon, Plus, X },
  inject: ['select'],
  props: {
    modelValue: { type: Object, default: () => ({}) },
    viewOnly: { type: Boolean, default: false },
  },
  data() { return { nodeConfig: {}, drawer: false, isEditTitle: false, form: {} }; },
  watch: { modelValue() { this.nodeConfig = this.modelValue; } },
  mounted() { this.nodeConfig = this.modelValue; },
  methods: {
    show() { this.form = JSON.parse(JSON.stringify(this.nodeConfig)); this.drawer = true; },
    editTitle() { this.isEditTitle = true; this.$nextTick(() => this.$refs.nodeTitleRef?.focus()); },
    saveTitle() { this.isEditTitle = false; },
    save() { this.$emit('update:modelValue', this.form); this.drawer = false; },
    delNode() { this.$emit('update:modelValue', this.nodeConfig.childNode); },
    delUser(index) { this.form.nodeAssigneeList.splice(index, 1); },
    selectHandle(type, data) { this.select(type, data); },
    toText(nodeConfig) {
      if (nodeConfig.nodeAssigneeList?.length > 0)
        return nodeConfig.nodeAssigneeList.map((item) => item.name).join('、');
      if (nodeConfig.userSelectFlag) return '发起人自选';
      return false;
    },
  },
};
</script>

<style scoped>
.drawer-body { padding: 0 20px 20px; }
.tags-list { margin-top: 8px; }
.node-wrap .title .close { position: absolute; top: 50%; right: 10px; transform: translateY(-50%); cursor: pointer; display: none; font-size: 14px; }
.node-wrap-box:hover .close { display: block; }
.node-wrap-drawer__title label { cursor: pointer; }
.node-wrap-drawer__title label:hover { border-bottom: 1px dashed hsl(var(--primary)); }
.node-wrap-drawer__title-edit { margin-left: 8px; color: hsl(var(--primary)); }
</style>
