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
    <add-node
      :model-value="nodeConfig.childNode"
      @update:model-value="updateChildNode"
    />
    <workflow-node-config-drawer
      v-model:open="drawer"
      title="审批人设置"
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
              placeholder="如：部门负责人审批、财务复核"
            />
          </a-form-item>
          <a-form-item v-if="!viewOnly">
            <div class="config-copy-actions">
              <a-button @click="copyCurrentConfig">
                <template #icon><IconifyIcon icon="lucide:copy" /></template>
                复制配置
              </a-button>
              <a-button @click="applyCopiedConfig">
                <template #icon><IconifyIcon icon="lucide:clipboard-check" /></template>
                套用配置
              </a-button>
            </div>
            <div class="form-tip">
              复制节点名称、审批人、空审批人、自审和多人审批方式；套用时不覆盖后续流程。
            </div>
          </a-form-item>
          <a-form-item v-if="!viewOnly">
            <workflow-fragment-copy-actions
              :node="form"
              @apply="applyWorkflowFragment"
            />
          </a-form-item>
          <a-form-item label="设置审批人">
              <div class="approver-config-list">
                <div
                  v-for="(config, index) in form.approverConfigs"
                  :key="index"
                  class="approver-config-card"
                >
                  <div class="approver-config-card__header">
                    <span>审批人 {{ index + 1 }}</span>
                    <a-button
                      v-if="form.approverConfigs.length > 1"
                      type="text"
                      danger
                      size="small"
                      @click="removeApproverConfig(index)"
                    >
                      <template #icon><X class="size-4" /></template>
                    </a-button>
                  </div>
                  <a-radio-group
                    v-model:value="config.setType"
                    class="approver-type-grid"
                    @change="changeApproverConfigType(config)"
                  >
                    <a-radio :value="2">部门负责人</a-radio>
                    <a-radio :value="6">部门负责人链</a-radio>
                    <a-radio :value="1">指定成员</a-radio>
                    <a-radio :value="3">角色</a-radio>
                    <a-radio :value="5">流程发起人</a-radio>
                  </a-radio-group>
                  <div v-if="config.setType === 2" class="approver-config-extra">
                    上一审批人/提交人的第
                    <a-input-number v-model:value="config.examineLevel" :min="1" />
                    级部门负责人
                  </div>
                  <div v-if="config.setType === 6" class="approver-config-extra">
                    <div class="form-tip">
                      从上一审批人/提交人所在部门开始，沿部门父链依次收集所有已配置的部门负责人。
                    </div>
                    <div class="manager-chain-actions">
                      <a-button @click="selectHandle(1, config.excludeAssigneeList)">
                        <template #icon><Plus class="size-4" /></template>
                        选择不参与成员
                      </a-button>
                      <a-button @click="selectHandle(1, config.extraAssigneeList)">
                        <template #icon><Plus class="size-4" /></template>
                        选择额外成员
                      </a-button>
                    </div>
                    <div v-if="config.excludeAssigneeList?.length" class="tags-list">
                      <span class="tag-prefix">不参与：</span>
                      <a-tag
                        v-for="(user, userIndex) in config.excludeAssigneeList"
                        :key="user.id"
                        closable
                        @close="delExcludeAssignee(config, userIndex)"
                      >{{ user.name }}</a-tag>
                    </div>
                    <div v-if="config.extraAssigneeList?.length" class="tags-list">
                      <span class="tag-prefix">额外成员：</span>
                      <a-tag
                        v-for="(user, userIndex) in config.extraAssigneeList"
                        :key="user.id"
                        closable
                        @close="delExtraAssignee(config, userIndex)"
                      >{{ user.name }}</a-tag>
                    </div>
                  </div>
                  <div v-if="config.setType === 1" class="approver-config-extra">
                    <a-button type="primary" @click="selectHandle(1, config.nodeAssigneeList)">
                      <template #icon><Plus class="size-4" /></template>
                      选择人员
                    </a-button>
                    <div class="tags-list">
                      <a-tag
                        v-for="(user, userIndex) in config.nodeAssigneeList"
                        :key="user.id"
                        closable
                        @close="delConfigAssignee(config, userIndex)"
                      >{{ user.name }}</a-tag>
                    </div>
                  </div>
                  <div v-if="config.setType === 3" class="approver-config-extra">
                    <a-button type="primary" @click="selectHandle(2, config.nodeAssigneeList)">
                      <template #icon><Plus class="size-4" /></template>
                      选择角色
                    </a-button>
                    <div class="tags-list">
                      <a-tag
                        v-for="(role, roleIndex) in config.nodeAssigneeList"
                        :key="role.id"
                        closable
                        @close="delConfigAssignee(config, roleIndex)"
                      >{{ role.name }}</a-tag>
                    </div>
                  </div>
                  <div v-if="config.setType === 3" class="approver-config-extra">
                    <div class="role-scope-block">
                      <div class="role-scope-block__label">发起部门范围</div>
                      <a-radio-group
                        v-model:value="config.initiatorDeptScopeMode"
                        class="role-scope-grid"
                        @change="changeInitiatorDeptScopeMode(config)"
                      >
                        <a-radio :value="0">沿用数据权限</a-radio>
                        <a-radio :value="2">数据权限 + 额外部门</a-radio>
                        <a-radio :value="1">不限部门</a-radio>
                      </a-radio-group>
                      <div v-if="config.initiatorDeptScopeMode === 2" class="dept-scope-select">
                        <a-button @click="selectHandle(3, config.initiatorDeptList)">
                          <template #icon><Plus class="size-4" /></template>
                          选择额外部门
                        </a-button>
                        <div class="tags-list">
                          <a-tag
                            v-for="(dept, deptIndex) in config.initiatorDeptList"
                            :key="dept.id"
                            closable
                            @close="delInitiatorDept(config, deptIndex)"
                          >{{ dept.name }}</a-tag>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <a-button type="link" class="add-approver-config" @click="addApproverConfig">
                <template #icon><Plus class="size-4" /></template>
                添加审批人
              </a-button>
            </a-form-item>
          <a-divider />
          <a-form-item label="审批人为空时">
            <a-radio-group v-model:value="form.emptyApproverPolicy" class="policy-grid">
              <a-radio :value="1">自动通过</a-radio>
              <a-radio :value="2">指定人员审批</a-radio>
              <a-radio :value="3">转交流程管理员</a-radio>
            </a-radio-group>
            <div v-if="form.emptyApproverPolicy === 2" class="approver-config-extra">
              <a-button type="primary" @click="selectHandle(1, form.emptyApproverAssigneeList)">
                <template #icon><Plus class="size-4" /></template>
                选择兜底人员
              </a-button>
              <div class="tags-list">
                <a-tag
                  v-for="(user, index) in form.emptyApproverAssigneeList"
                  :key="user.id"
                  closable
                  @close="delEmptyApprover(index)"
                >{{ user.name }}</a-tag>
              </div>
            </div>
          </a-form-item>
          <a-form-item label="提交人与审批人为同一人时">
            <a-radio-group v-model:value="form.selfApprovalPolicy" class="policy-grid">
              <a-radio :value="1">由提交人自己审批</a-radio>
              <a-radio :value="2">自动跳过</a-radio>
            <a-radio :value="3">转交部门负责人</a-radio>
              <a-radio :value="4">转交部门负责人</a-radio>
            </a-radio-group>
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
    </workflow-node-config-drawer>
  </div>
</template>

<script>
import { IconifyIcon, Plus, X } from '@vben/icons';
import {
  Button, Checkbox, Divider, Form, Input, InputNumber, message, Radio, Select, Tag,
} from 'ant-design-vue';


import addNode from './addNode.vue';
import WorkflowFragmentCopyActions from '../WorkflowFragmentCopyActions.vue';
import WorkflowNodeConfigDrawer from '../WorkflowNodeConfigDrawer.vue';
import WorkflowNodeDrawerTitle from '../WorkflowNodeDrawerTitle.vue';

export default {
  name: 'ApproverNode',
  components: {
    addNode,
    AButton: Button,
    ACheckbox: Checkbox,
    ADivider: Divider,
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
    WorkflowFragmentCopyActions,
    WorkflowNodeConfigDrawer,
    WorkflowNodeDrawerTitle,
    X,
  },
  inject: [
    'select',
    'copyWorkflowNodeConfig',
    'getWorkflowNodeConfigClipboard',
  ],
  props: {
    modelValue: { type: Object, default: () => ({}) },
    viewOnly: { type: Boolean, default: false },
  },
  data() {
    return {
      nodeConfig: {},
      drawer: false,
      form: {},
    };
  },
  watch: { modelValue() { this.nodeConfig = this.modelValue; } },
  mounted() { this.nodeConfig = this.modelValue; },
  methods: {
    show() {
      this.form = JSON.parse(JSON.stringify(this.nodeConfig));
      this.normalizeForm();
      this.drawer = true;
    },
    save() {
      this.normalizeForm();
      this.nodeConfig = this.form;
      this.$emit('update:modelValue', this.nodeConfig);
      this.drawer = false;
    },
    delNode() { this.$emit('update:modelValue', this.nodeConfig.childNode); },
    delUser(index) { this.form.nodeAssigneeList.splice(index, 1); },
    delRole(index) { this.form.nodeAssigneeList.splice(index, 1); },
    delConfigAssignee(config, index) { config.nodeAssigneeList.splice(index, 1); },
    delInitiatorDept(config, index) { config.initiatorDeptList.splice(index, 1); },
    // 移除部门负责人链中不参与审批的成员。
    delExcludeAssignee(config, index) { config.excludeAssigneeList.splice(index, 1); },
    // 移除部门负责人链中额外追加的审批成员。
    delExtraAssignee(config, index) { config.extraAssigneeList.splice(index, 1); },
    delEmptyApprover(index) { this.form.emptyApproverAssigneeList.splice(index, 1); },
    selectHandle(type, data) { this.select(type, data); },
    changeSetType() { this.form.nodeAssigneeList = []; },
    updateChildNode(childNode) {
      this.nodeConfig.childNode = childNode;
      this.$emit('update:modelValue', this.nodeConfig);
    },
    copyCurrentConfig() {
      this.normalizeForm();
      this.copyWorkflowNodeConfig?.({
        type: 'approver',
        label: this.form.nodeName || '审批节点',
        patch: this.buildConfigPatch(),
      });
      message.success('已复制审批配置');
    },
    buildConfigPatch() {
      return JSON.parse(JSON.stringify({
        nodeName: this.form.nodeName,
        approverConfigs: this.form.approverConfigs,
        emptyApproverPolicy: this.form.emptyApproverPolicy,
        emptyApproverAssigneeList: this.form.emptyApproverAssigneeList,
        selfApprovalPolicy: this.form.selfApprovalPolicy,
        examineMode: this.form.examineMode,
      }));
    },
    applyCopiedConfig() {
      const clipboard = this.getWorkflowNodeConfigClipboard?.();
      if (!clipboard) {
        message.warning('请先复制一个审批节点配置');
        return;
      }
      if (clipboard.type !== 'approver') {
        message.warning('已复制的是抄送配置，不能套用到审批节点');
        return;
      }
      Object.assign(this.form, JSON.parse(JSON.stringify(clipboard.patch)));
      this.normalizeForm();
      message.success('已套用审批配置');
    },
    applyWorkflowFragment(fragment) {
      this.form.childNode = fragment;
    },
    normalizeForm() {
      if (!Array.isArray(this.form.approverConfigs) || this.form.approverConfigs.length === 0) {
        this.form.approverConfigs = [this.createApproverConfig(2)];
      }
      this.form.approverConfigs.forEach((config) => {
        if (!Array.isArray(config.nodeAssigneeList)) config.nodeAssigneeList = [];
        if (config.setType === 2 && (!config.examineLevel || config.examineLevel < 1)) config.examineLevel = 1;
        if (config.setType === 7) config.setType = 1;
        if (!Array.isArray(config.initiatorDeptList)) config.initiatorDeptList = [];
        if (![0, 1, 2].includes(config.initiatorDeptScopeMode)) config.initiatorDeptScopeMode = 0;
        // 旧流程可能没有部门负责人链扩展字段，打开抽屉时补齐默认数组。
        if (!Array.isArray(config.excludeAssigneeList)) config.excludeAssigneeList = [];
        if (!Array.isArray(config.extraAssigneeList)) config.extraAssigneeList = [];
      });
      if (!this.form.emptyApproverPolicy) this.form.emptyApproverPolicy = 1;
      if (!Array.isArray(this.form.emptyApproverAssigneeList)) this.form.emptyApproverAssigneeList = [];
      if (!this.form.selfApprovalPolicy) this.form.selfApprovalPolicy = 1;
    },
    createApproverConfig(setType = 2) {
      return {
        setType,
        nodeAssigneeList: [],
        examineLevel: 1,
        initiatorDeptScopeMode: 0,
        initiatorDeptList: [],
        excludeAssigneeList: [],
        extraAssigneeList: [],
      };
    },
    addApproverConfig() {
      this.form.approverConfigs.push(this.createApproverConfig(1));
    },
    removeApproverConfig(index) {
      this.form.approverConfigs.splice(index, 1);
    },
    changeApproverConfigType(config) {
      config.nodeAssigneeList = [];
      if (config.setType === 2 && (!config.examineLevel || config.examineLevel < 1)) config.examineLevel = 1;
      if (config.setType !== 6) {
        // 切走「部门负责人链」时清理专属的排除和追加成员配置。
        config.excludeAssigneeList = [];
        config.extraAssigneeList = [];
      }
      if (config.setType !== 3) {
        config.initiatorDeptScopeMode = 0;
        config.initiatorDeptList = [];
      }
    },
    changeInitiatorDeptScopeMode(config) {
      if (!Array.isArray(config.initiatorDeptList)) config.initiatorDeptList = [];
      if (config.initiatorDeptScopeMode !== 2) config.initiatorDeptList = [];
    },
    toText(nodeConfig) {
      const configs = Array.isArray(nodeConfig.approverConfigs) ? nodeConfig.approverConfigs : [];
      const parts = configs.map((config) => {
        if (config.setType === 2) {
          const level = config.examineLevel >= 1 ? config.examineLevel : 1;
          return level === 1 ? '本部门负责人' : `第${level}级部门负责人`;
        }
        if (config.setType === 1) return config.nodeAssigneeList?.length > 0 ? `指定成员${config.nodeAssigneeList.length}人` : '指定成员';
        if (config.setType === 6) {
          const excludeCount = config.excludeAssigneeList?.length || 0;
          const extraCount = config.extraAssigneeList?.length || 0;
          const suffix = [
            excludeCount ? `排除${excludeCount}人` : '',
            extraCount ? `额外${extraCount}人` : '',
          ].filter(Boolean).join('，');
          return suffix ? `部门负责人链（${suffix}）` : '部门负责人链';
        }
        if (config.setType === 3) {
          const roleText = config.nodeAssigneeList?.length > 0 ? `角色-${config.nodeAssigneeList.map((item) => item.name).join('、')}` : '角色';
          if (config.initiatorDeptScopeMode === 1) return `${roleText}（不限部门）`;
          if (config.initiatorDeptScopeMode === 2) {
            const count = config.initiatorDeptList?.length || 0;
            return `${roleText}（额外部门${count}个）`;
          }
          return roleText;
        }
        if (config.setType === 5) return '流程发起人';
        return '';
      }).filter(Boolean);
      return parts.length > 0 ? parts.join(' + ') : false;
    },
  },
};
</script>

<style scoped>
.approver-title { background: #ff943e; }
.drawer-body { padding: 0 20px 20px; }
.form-tip { margin-top: 6px; color: hsl(var(--muted-foreground)); font-size: 12px; line-height: 1.5; }
.tags-list { margin-top: 8px; }
.approver-config-list { display: flex; flex-direction: column; gap: 10px; }
.approver-config-card { border: 1px solid hsl(var(--border)); border-radius: 6px; overflow: hidden; background: hsl(var(--card)); }
.approver-config-card__header { display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; background: hsl(var(--muted) / 0.45); font-weight: 600; }
.approver-type-grid,
.policy-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 8px 12px; padding: 12px; }
.policy-grid { padding: 0; }
.approver-config-extra { padding: 0 12px 12px; }
.manager-chain-actions { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 10px; }
.tag-prefix { margin-right: 6px; color: hsl(var(--muted-foreground)); font-size: 12px; }
.role-scope-block { margin-top: 12px; padding-top: 10px; border-top: 1px dashed hsl(var(--border)); }
.role-scope-block--compact { margin-top: 0; }
.role-scope-block__label { margin-bottom: 8px; color: hsl(var(--muted-foreground)); font-size: 12px; }
.role-scope-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 8px 12px; }
.dept-scope-select { margin-top: 10px; }
.add-approver-config { margin-top: 8px; padding-left: 0; }
.config-copy-actions { display: flex; flex-wrap: wrap; gap: 8px; }
.node-wrap .title .close { position: absolute; top: 50%; right: 10px; transform: translateY(-50%); cursor: pointer; display: none; font-size: 14px; }
.node-wrap-box:hover .close { display: block; }
</style>
