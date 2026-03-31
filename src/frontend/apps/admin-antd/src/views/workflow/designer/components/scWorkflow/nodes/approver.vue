<template>
  <div class="node-wrap">
    <div class="node-wrap-box" @click="show">
      <div class="title approver-title">
        <IconifyIcon icon="lucide:user" class="icon" />
        <span>{{ nodeConfig.nodeName }}</span>
        <X class="close" @click.stop="delNode()" />
      </div>
      <div class="content">
        <span v-if="toText(nodeConfig)">{{ toText(nodeConfig) }}</span>
        <span v-else class="placeholder">请选择</span>
      </div>
    </div>
    <add-node v-model="nodeConfig.childNode" />
    <a-drawer
      v-model:open="drawer"
      title="审批人设置"
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
              placeholder="如：部门主管审批、财务复核"
            />
          </a-form-item>
          <a-form-item label="审批人员类型">
            <a-select v-model:value="form.setType" @change="changeSetType" style="width: 100%">
              <a-select-option :value="1">指定成员</a-select-option>
              <a-select-option :value="2">主管</a-select-option>
              <a-select-option :value="3">角色</a-select-option>
            </a-select>
          </a-form-item>
          <a-form-item v-if="form.setType === 1" label="选择成员">
            <a-button type="primary" @click="selectHandle(1, form.nodeAssigneeList)">
              <template #icon><Plus class="size-4" /></template>
              选择人员
            </a-button>
            <div class="tags-list">
              <a-tag v-for="(user, index) in form.nodeAssigneeList" :key="user.id" closable @close="delUser(index)">{{ user.name }}</a-tag>
            </div>
          </a-form-item>
          <a-form-item v-if="form.setType === 2" label="指定主管">
            发起人的第 <a-input-number v-model:value="form.examineLevel" :min="1" /> 级主管
          </a-form-item>
          <a-form-item v-if="form.setType === 3" label="选择角色">
            <a-button type="primary" @click="selectHandle(2, form.nodeAssigneeList)">
              <template #icon><Plus class="size-4" /></template>
              选择角色
            </a-button>
            <div class="tags-list">
              <a-tag v-for="(role, index) in form.nodeAssigneeList" :key="role.id" closable @close="delRole(index)">{{ role.name }}</a-tag>
            </div>
          </a-form-item>
          <a-divider />
          <a-form-item>
            <a-checkbox v-model:checked="form.termAuto">超时自动审批</a-checkbox>
          </a-form-item>
          <template v-if="form.termAuto">
            <a-form-item label="审批期限（为 0 则不生效）">
              <a-input-number v-model:value="form.term" :min="0" /> 小时
            </a-form-item>
            <a-form-item label="审批期限超时后执行">
              <a-radio-group v-model:value="form.termMode">
                <a-radio :value="0">自动通过</a-radio>
                <a-radio :value="1">自动拒绝</a-radio>
              </a-radio-group>
            </a-form-item>
          </template>
          <a-divider />
          <a-form-item label="多人审批时审批方式">
            <a-radio-group v-model:value="form.examineMode">
              <a-radio :value="1">按顺序依次审批</a-radio>
              <a-radio :value="2">会签 (可同时审批，每个人必须审批通过)</a-radio>
              <a-radio :value="3">或签 (有一人审批通过即可)</a-radio>
            </a-radio-group>
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
import {
  Button, Checkbox, Divider, Drawer, Form, Input, InputNumber, Radio, Select, Tag,
} from 'ant-design-vue';
import addNode from './addNode.vue';

export default {
  name: 'ApproverNode',
  components: {
    addNode,
    AButton: Button,
    ACheckbox: Checkbox,
    ADivider: Divider,
    ADrawer: Drawer,
    AForm: Form,
    AFormItem: Form.Item,
    AInput: Input,
    AInputNumber: InputNumber,
    ARadio: Radio,
    ARadioGroup: Radio.Group,
    ASelect: Select,
    ASelectOption: Select.Option,
    ATag: Tag,
    IconifyIcon,
    Plus,
    X,
  },
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
    delRole(index) { this.form.nodeAssigneeList.splice(index, 1); },
    selectHandle(type, data) { this.select(type, data); },
    changeSetType() { this.form.nodeAssigneeList = []; },
    toText(nodeConfig) {
      if (nodeConfig.setType === 1) {
        if (nodeConfig.nodeAssigneeList?.length > 0)
          return nodeConfig.nodeAssigneeList.map((item) => item.name).join('、');
        return false;
      }
      if (nodeConfig.setType === 2)
        return nodeConfig.examineLevel === 1 ? '直接主管' : `发起人的第${nodeConfig.examineLevel}级主管`;
      if (nodeConfig.setType === 3) {
        if (nodeConfig.nodeAssigneeList?.length > 0)
          return '角色-' + nodeConfig.nodeAssigneeList.map((item) => item.name).join('、');
        return false;
      }
      return false;
    },
  },
};
</script>

<style scoped>
.approver-title { background: #ff943e; }
.drawer-body { padding: 0 20px 20px; }
.tags-list { margin-top: 8px; }
.node-wrap .title .close { position: absolute; top: 50%; right: 10px; transform: translateY(-50%); cursor: pointer; display: none; font-size: 14px; }
.node-wrap-box:hover .close { display: block; }
.node-wrap-drawer__title label { cursor: pointer; }
.node-wrap-drawer__title label:hover { border-bottom: 1px dashed hsl(var(--primary)); }
.node-wrap-drawer__title-edit { margin-left: 8px; color: hsl(var(--primary)); }
</style>
