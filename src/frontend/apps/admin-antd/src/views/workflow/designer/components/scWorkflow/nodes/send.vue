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
        <span v-else class="placeholder">请选择抄送对象</span>
      </div>
    </div>
    <add-node
      :model-value="nodeConfig.childNode"
      @update:model-value="updateChildNode"
    />
    <workflow-node-config-drawer
      v-model:open="drawer"
      title="抄送人设置"
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
              placeholder="如：抄送财务、抄送人事"
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
              复制节点名称和抄送人配置；套用时不覆盖后续流程。
            </div>
          </a-form-item>
          <a-form-item v-if="!viewOnly">
            <workflow-fragment-copy-actions
              :node="form"
              @apply="applyWorkflowFragment"
            />
          </a-form-item>
          <a-form-item label="设置抄送人">
            <div class="copy-config-list">
              <div
                v-for="(config, configIndex) in form.copyConfigs"
                :key="configIndex"
                class="copy-config-card"
              >
                <div class="copy-config-card__header">
                  <span>抄送人 {{ configIndex + 1 }}</span>
                  <a-button
                    v-if="form.copyConfigs.length > 1"
                    type="text"
                    danger
                    size="small"
                    @click="removeCopyConfig(configIndex)"
                  >
                    <template #icon><X class="size-4" /></template>
                  </a-button>
                </div>
                <a-radio-group
                  v-model:value="config.setType"
                  class="copy-type-grid"
                  @change="changeCopyConfigType(config)"
                >
                  <a-radio :value="1">指定成员</a-radio>
                  <a-radio :value="2">部门负责人</a-radio>
                  <a-radio :value="6">部门负责人链</a-radio>
                  <a-radio :value="3">角色</a-radio>
                  <a-radio :value="5">流程发起人</a-radio>
                </a-radio-group>
                <div v-if="config.setType === 2" class="copy-config-extra">
                  上一审批节点处理人的第
                  <a-input-number v-model:value="config.examineLevel" :min="1" />
                  级部门负责人
                  <div class="form-tip">路径上首个审批节点前无上一环节时，按流程发起人部门取负责人。</div>
                </div>
                <div v-if="config.setType === 6" class="copy-config-extra">
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
                      v-for="(user, index) in config.excludeAssigneeList"
                      :key="user.id"
                      closable
                      @close="delExcludeAssignee(config, index)">
                      {{ user.name }}
                    </a-tag>
                  </div>
                  <div v-if="config.extraAssigneeList?.length" class="tags-list">
                    <span class="tag-prefix">额外成员：</span>
                    <a-tag
                      v-for="(user, index) in config.extraAssigneeList"
                      :key="user.id"
                      closable
                      @close="delExtraAssignee(config, index)">
                      {{ user.name }}
                    </a-tag>
                  </div>
                </div>
                <div v-if="config.setType === 1" class="copy-config-extra">
                  <a-button type="primary" @click="selectHandle(1, config.nodeAssigneeList)">
                    <template #icon><Plus class="size-4" /></template>
                    选择人员
                  </a-button>
                  <div class="tags-list">
                    <a-tag
                      v-for="(user, index) in config.nodeAssigneeList"
                      :key="user.id"
                      closable
                      @close="delConfigAssignee(config, index)">
                      {{ user.name }}
                    </a-tag>
                  </div>
                </div>
                <div v-if="config.setType === 3" class="copy-config-extra">
                  <a-button type="primary" @click="selectHandle(2, config.nodeAssigneeList)">
                    <template #icon><Plus class="size-4" /></template>
                    选择角色
                  </a-button>
                  <div class="tags-list">
                    <a-tag
                      v-for="(role, index) in config.nodeAssigneeList"
                      :key="role.id"
                      closable
                      @close="delConfigAssignee(config, index)">
                      {{ role.name }}
                    </a-tag>
                  </div>
                </div>
              </div>
            </div>
            <a-button type="link" class="add-copy-config" @click="addCopyConfig">
              <template #icon><Plus class="size-4" /></template>
              添加抄送人
            </a-button>
          </a-form-item>
          <a-form-item v-if="form.setType === 1">
            <a-checkbox v-model:checked="form.userSelectFlag">允许发起人自选抄送人</a-checkbox>
          </a-form-item>
        </a-form>
      </div>
    </workflow-node-config-drawer>
  </div>
</template>

<script>
import { IconifyIcon, Plus, X } from '@vben/icons';
import { Button, Checkbox, Form, Input, InputNumber, message, Radio, Tag } from 'ant-design-vue';
import addNode from './addNode.vue';
import WorkflowFragmentCopyActions from '../WorkflowFragmentCopyActions.vue';
import WorkflowNodeConfigDrawer from '../WorkflowNodeConfigDrawer.vue';
import WorkflowNodeDrawerTitle from '../WorkflowNodeDrawerTitle.vue';

export default {
  name: 'SendNode',
  components: {
    addNode,
    AButton: Button,
    ACheckbox: Checkbox,
    AForm: Form,
    AFormItem: Form.Item,
    AInput: Input,
    AInputNumber: InputNumber,
    ARadio: Radio,
    ARadioGroup: Radio.Group,
    ATag: Tag,
    IconifyIcon,
    Plus,
    WorkflowFragmentCopyActions,
    X,
    WorkflowNodeConfigDrawer,
    WorkflowNodeDrawerTitle,
  },
  inject: ['select', 'copyWorkflowNodeConfig', 'getWorkflowNodeConfigClipboard'],
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
    delConfigAssignee(config, index) { config.nodeAssigneeList.splice(index, 1); },
    // 移除部门负责人链中不参与抄送的成员。
    delExcludeAssignee(config, index) { config.excludeAssigneeList.splice(index, 1); },
    // 移除部门负责人链中额外追加的抄送成员。
    delExtraAssignee(config, index) { config.extraAssigneeList.splice(index, 1); },
    selectHandle(type, data) { this.select(type, data); },
    updateChildNode(childNode) {
      this.nodeConfig.childNode = childNode;
      this.$emit('update:modelValue', this.nodeConfig);
    },
    copyCurrentConfig() {
      this.normalizeForm();
      this.copyWorkflowNodeConfig?.({
        type: 'copy',
        label: this.form.nodeName || '抄送节点',
        patch: this.buildConfigPatch(),
      });
      message.success('已复制抄送配置');
    },
    buildConfigPatch() {
      return JSON.parse(JSON.stringify({
        nodeName: this.form.nodeName,
        copyConfigs: this.form.copyConfigs,
        setType: this.form.setType,
        nodeAssigneeList: this.form.nodeAssigneeList,
        examineLevel: this.form.examineLevel,
        userSelectFlag: this.form.userSelectFlag,
      }));
    },
    applyCopiedConfig() {
      const clipboard = this.getWorkflowNodeConfigClipboard?.();
      if (!clipboard) {
        message.warning('请先复制一个抄送节点配置');
        return;
      }
      if (clipboard.type !== 'copy') {
        message.warning('已复制的是审批配置，不能套用到抄送节点');
        return;
      }
      Object.assign(this.form, JSON.parse(JSON.stringify(clipboard.patch)));
      this.normalizeForm();
      message.success('已套用抄送配置');
    },
    applyWorkflowFragment(fragment) {
      this.form.childNode = fragment;
    },
    normalizeForm() {
      const hasCopyConfigs = Array.isArray(this.form.copyConfigs) && this.form.copyConfigs.length > 0;
      const hasTreeAssignees = Array.isArray(this.form.nodeAssigneeList) && this.form.nodeAssigneeList.length > 0;
      if (this.form.userSelectFlag && !hasCopyConfigs && !hasTreeAssignees) {
        this.form.copyConfigs = [];
        return;
      }
      if (!Array.isArray(this.form.copyConfigs) || this.form.copyConfigs.length === 0) {
        this.form.copyConfigs = [this.createCopyConfig(this.form.setType || 1)];
        this.form.copyConfigs[0].nodeAssigneeList = Array.isArray(this.form.nodeAssigneeList)
          ? this.form.nodeAssigneeList
          : [];
        this.form.copyConfigs[0].examineLevel = this.form.examineLevel || 1;
      }
      this.form.copyConfigs.forEach((config) => {
        if (!Array.isArray(config.nodeAssigneeList)) config.nodeAssigneeList = [];
        if (config.setType === 2 && (!config.examineLevel || config.examineLevel < 1)) config.examineLevel = 1;
        // 旧流程没有部门负责人链专属字段，打开配置时补齐，避免选择成员时报错。
        if (!Array.isArray(config.excludeAssigneeList)) config.excludeAssigneeList = [];
        if (!Array.isArray(config.extraAssigneeList)) config.extraAssigneeList = [];
      });
      const first = this.form.copyConfigs[0] || this.createCopyConfig(1);
      this.form.setType = first.setType;
      this.form.nodeAssigneeList = first.nodeAssigneeList;
      this.form.examineLevel = first.examineLevel || 1;
      if (this.form.copyConfigs.length > 0) {
        this.form.userSelectFlag = false;
      }
    },
    createCopyConfig(setType = 1) {
      return {
        setType,
        nodeAssigneeList: [],
        examineLevel: 1,
        excludeAssigneeList: [],
        extraAssigneeList: [],
      };
    },
    addCopyConfig() {
      this.form.userSelectFlag = false;
      this.form.copyConfigs.push(this.createCopyConfig(1));
    },
    removeCopyConfig(index) {
      this.form.copyConfigs.splice(index, 1);
    },
    changeCopyConfigType(config) {
      config.nodeAssigneeList = [];
      if (config.setType === 2 && (!config.examineLevel || config.examineLevel < 1)) config.examineLevel = 1;
      if (config.setType !== 6) {
        // 切走「部门负责人链」时清理专属的排除和追加成员配置。
        config.excludeAssigneeList = [];
        config.extraAssigneeList = [];
      }
    },
    toText(nodeConfig) {
      if (nodeConfig.userSelectFlag) return '发起人自选';
      const configs = Array.isArray(nodeConfig.copyConfigs) && nodeConfig.copyConfigs.length > 0
        ? nodeConfig.copyConfigs
        : [{ setType: nodeConfig.setType, nodeAssigneeList: nodeConfig.nodeAssigneeList, examineLevel: nodeConfig.examineLevel }];
      const parts = configs.map((config) => {
        if (config.setType === 2) {
          const lv = config.examineLevel >= 1 ? config.examineLevel : 1;
          return lv === 1 ? '本部门负责人' : `第${lv}级部门负责人`;
        }
        if (config.setType === 6) {
          const suffix = [
            config.excludeAssigneeList?.length ? `排除${config.excludeAssigneeList.length}人` : '',
            config.extraAssigneeList?.length ? `追加${config.extraAssigneeList.length}人` : '',
          ].filter(Boolean).join('，');
          return suffix ? `部门负责人链（${suffix}）` : '部门负责人链';
        }
        if (config.setType === 1) return config.nodeAssigneeList?.length > 0 ? `指定成员${config.nodeAssigneeList.length}人` : '指定成员';
        if (config.setType === 3) return config.nodeAssigneeList?.length > 0 ? `角色-${config.nodeAssigneeList.map((item) => item.name).join('、')}` : '角色';
        if (config.setType === 5) return '流程发起人';
        return '';
      }).filter(Boolean);
      return parts.length > 0 ? `抄送-${parts.join(' + ')}` : false;
    },
  },
};
</script>

<style scoped>
.drawer-body { padding: 0 20px 20px; }
.form-tip { margin-top: 6px; color: hsl(var(--muted-foreground)); font-size: 12px; line-height: 1.5; }
.tags-list { margin-top: 8px; }
.tag-prefix { margin-right: 6px; color: hsl(var(--muted-foreground)); font-size: 12px; }
.copy-config-list { display: flex; flex-direction: column; gap: 10px; }
.copy-config-card { border: 1px solid hsl(var(--border)); border-radius: 6px; overflow: hidden; background: hsl(var(--card)); }
.copy-config-card__header { display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; background: hsl(var(--muted) / 0.45); font-weight: 600; }
.copy-type-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 8px 12px; padding: 12px; }
.copy-config-extra { padding: 0 12px 12px; }
.manager-chain-actions { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 10px; }
.add-copy-config { margin-top: 8px; padding-left: 0; }
.config-copy-actions { display: flex; flex-wrap: wrap; gap: 8px; }
.node-wrap .title .close { position: absolute; top: 50%; right: 10px; transform: translateY(-50%); cursor: pointer; display: none; font-size: 14px; }
.node-wrap-box:hover .close { display: block; }
</style>
