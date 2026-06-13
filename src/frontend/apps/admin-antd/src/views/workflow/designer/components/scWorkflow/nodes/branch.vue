<template>
  <div class="branch-wrap">
    <div class="branch-box-wrap">
      <div class="branch-box">
        <a-button
          class="add-branch"
          type="primary"
          @click="addTerm">
          添加同级条件
        </a-button>
        <div
          class="col-box"
          v-for="(item, index) in nodeConfig.conditionNodes"
          :key="index">
          <div class="condition-node">
            <div class="condition-node-box">
              <div
                class="auto-judge"
                @click="show(index)">
                <div
                  class="sort-left"
                  v-if="index != 0"
                  @click.stop="arrTransfer(index, -1)">
                  <ChevronLeft class="size-4" />
                </div>
                <div class="title">
                  <span class="node-title">{{ item.nodeName }}</span>
                  <span class="priority-title">优先级{{ item.priorityLevel }}</span>
                  <X class="close" @click.stop="delTerm(index)" />
                </div>
                <div class="content">
                  <span v-if="toText(nodeConfig, index)">{{ toText(nodeConfig, index) }}</span>
                  <span v-else class="placeholder">请设置条件</span>
                </div>
                <div
                  class="sort-right"
                  v-if="index != nodeConfig.conditionNodes.length - 1"
                  @click.stop="arrTransfer(index)">
                  <ChevronRight class="size-4" />
                </div>
              </div>
              <add-node
                :model-value="item.childNode"
                @update:model-value="updateConditionChildNode(item, $event)"
              />
            </div>
          </div>
          <slot v-if="item.childNode" :node="item" />
          <div class="top-left-cover-line" v-if="index == 0" />
          <div class="bottom-left-cover-line" v-if="index == 0" />
          <div class="top-right-cover-line" v-if="index == nodeConfig.conditionNodes.length - 1" />
          <div class="bottom-right-cover-line" v-if="index == nodeConfig.conditionNodes.length - 1" />
        </div>
      </div>
      <add-node
        :model-value="nodeConfig.childNode"
        @update:model-value="updateChildNode"
      />
    </div>
    <workflow-node-config-drawer
      v-model:open="drawer"
      title="条件设置"
      :width="600"
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
              placeholder="如：金额>1万、销售部"
            />
          </a-form-item>
          <a-form-item label="与指定优先级互换">
            <a-select
              v-model:value="form.priorityLevel"
              :options="conditionPriorityOptions()"
              placeholder="请选择要互换的优先级"
            />
          </a-form-item>
          <a-form-item v-if="!viewOnly">
            <workflow-fragment-copy-actions
              :node="form"
              :fragment-node="form.childNode"
              tip="复制当前条件分支下面的流程；套用时插入到当前分支已有流程前面。"
              copy-tip="默认选中当前条件分支下面的所有流程节点，可取消不需要复制的节点。"
              @apply="applyWorkflowFragment"
            />
          </a-form-item>
        </a-form>
        <div v-if="isCurrentFallback()" class="top-tips">
          当前分支为“其他情况”兜底分支，无需配置条件；前面条件都不满足时进入此分支。
        </div>
        <div v-else class="top-tips">满足以下条件时进入当前分支</div>
        <template v-if="!isCurrentFallback()" v-for="(conditionGroup, conditionGroupIdx) in form.conditionList" :key="conditionGroupIdx">
          <div class="or-branch-link-tip" v-if="conditionGroupIdx != 0">或满足</div>
          <div class="condition-group-editor">
            <div class="header">
              <span>条件组 {{ conditionGroupIdx + 1 }}</span>
              <span class="branch-delete-icon" @click="deleteConditionGroup(conditionGroupIdx)"><IconifyIcon icon="lucide:trash-2" /></span>
            </div>
            <div class="main-content">
              <div class="condition-content-box cell-box">
                <div>描述</div>
                <div>条件字段</div>
                <div>运算符</div>
                <div>值</div>
              </div>
              <div class="condition-content" v-for="(condition, idx) in conditionGroup" :key="idx">
                <div class="condition-relation">
                  <span>{{ idx == 0 ? '当' : '且' }}</span>
                  <span class="branch-delete-icon" @click="deleteConditionList(conditionGroup, idx)"><IconifyIcon icon="lucide:trash-2" /></span>
                </div>
                <div class="condition-content-box">
                  <a-input v-model:value="condition.label" placeholder="描述" />
                  <a-select
                    v-model:value="condition.field"
                    placeholder="请选择字段"
                    style="min-width: 120px"
                    :options="conditionFieldOptions"
                    allow-clear
                    :loading="conditionFieldsLoading"
                    @change="onConditionFieldChange(condition, $event)" />
                  <a-select
                    v-if="!isPresenceCondition(condition)"
                    v-model:value="condition.operator"
                    placeholder="运算符"
                    style="min-width: 100px">
                    <a-select-option value="==">等于</a-select-option>
                    <a-select-option value="!=">不等于</a-select-option>
                    <a-select-option value=">">大于</a-select-option>
                    <a-select-option value=">=">大于等于</a-select-option>
                    <a-select-option value="<">小于</a-select-option>
                    <a-select-option value="<=">小于等于</a-select-option>
                    <a-select-option value="include">包含</a-select-option>
                    <a-select-option value="notinclude">不包含</a-select-option>
                  </a-select>
                  <span v-else class="text-muted-foreground text-sm">等于</span>
                  <a-select
                    v-if="isEnumMultiSelectCondition(condition) && getValueSelectOptions(condition).length"
                    :value="getMultiSelectValue(condition)"
                    mode="multiple"
                    allow-clear
                    placeholder="请选择（可多选）"
                    style="min-width: 220px"
                    :max-tag-count="3"
                    :options="getValueSelectOptions(condition)"
                    show-search
                    :filter-option="filterSelectOption"
                    @update:value="onMultiSelectValueChange(condition, $event)"
                  />
                  <a-select
                    v-else-if="getValueSelectOptions(condition).length"
                    v-model:value="condition.value"
                    allow-clear
                    placeholder="请选择"
                    style="min-width: 180px"
                    :options="getValueSelectOptions(condition)"
                    show-search
                    :filter-option="filterSelectOption" />
                  <a-input
                    v-else
                    v-model:value="condition.value"
                    placeholder="值" />
                </div>
              </div>
            </div>
            <div class="sub-content">
              <a-button type="link" @click="addConditionList(conditionGroup)">添加条件</a-button>
            </div>
          </div>
        </template>
        <a-button v-if="!isCurrentFallback()" type="dashed" block @click="addConditionGroup">添加条件组</a-button>
      </div>
    </workflow-node-config-drawer>
  </div>
</template>

<script>
import { ChevronLeft, ChevronRight, IconifyIcon, X } from '@vben/icons';
import { Button, Form, Input, Select } from 'ant-design-vue';

import { getConditionFields } from '#/api/system/workflow';

import { createWorkflowNodeKey } from '../../../utils/createWorkflowNodeKey';

import addNode from './addNode.vue';
import WorkflowFragmentCopyActions from '../WorkflowFragmentCopyActions.vue';
import WorkflowNodeConfigDrawer from '../WorkflowNodeConfigDrawer.vue';
import WorkflowNodeDrawerTitle from '../WorkflowNodeDrawerTitle.vue';

export default {
  name: 'BranchNode',
  components: {
    addNode,
    AButton: Button,
    AForm: Form,
    AFormItem: Form.Item,
    AInput: Input,
    ASelect: Select,
    ASelectOption: Select.Option,
    ChevronLeft,
    ChevronRight,
    IconifyIcon,
    WorkflowFragmentCopyActions,
    WorkflowNodeConfigDrawer,
    WorkflowNodeDrawerTitle,
    X,
  },
  props: {
    modelValue: { type: Object, default: () => ({}) },
    category: { type: String, default: '' },
    viewOnly: { type: Boolean, default: false },
  },
  data() {
    return {
      nodeConfig: {},
      drawer: false,
      index: 0,
      form: {},
      conditionFieldOptions: [],
      conditionFieldDefs: [],
      conditionFieldsLoading: false,
    }
  },
  watch: {
    modelValue() {
      this.nodeConfig = this.modelValue
    },
    category: {
      immediate: true,
      handler() {
        this.loadConditionFieldDefs()
      },
    },
  },
  mounted() {
    this.nodeConfig = this.modelValue
  },
  methods: {
    normalizeOperator(op) {
      // 将设计器运算符别名统一为后端支持的标准运算符。
      if (op === '=') return '=='
      if (op === '<>') return '!='
      return op
    },
    getFieldDef(fieldKey) {
      return (this.conditionFieldDefs || []).find((d) => d.key === fieldKey)
    },
    /** 标准化条件字段接口返回的大小写。 */
    normalizeConditionFieldDefs(list) {
      return (list || []).map((raw) => ({
        key: raw.key ?? raw.Key ?? '',
        label: raw.label ?? raw.Label ?? '',
        type: raw.type ?? raw.Type ?? 'string',
        options: (raw.options ?? raw.Options ?? []).map((o) => ({
          value: String(o.value ?? o.Value ?? ''),
          label: String(o.label ?? o.Label ?? ''),
        })),
      }))
    },
    async loadConditionFieldDefs() {
      if (!this.category) {
        this.conditionFieldDefs = []
        return
      }
      this.conditionFieldsLoading = true
      try {
        const list = await getConditionFields(this.category)
        this.conditionFieldDefs = this.normalizeConditionFieldDefs(list)
      } catch {
        this.conditionFieldDefs = []
      } finally {
        this.conditionFieldsLoading = false
      }
    },
    /** 分支摘要、枚举字段显示选项标签而非原始 value（如角色 Guid） */
    getConditionValueDisplay(condition) {
      if (condition == null) return ''
      const raw = condition.value
      if (raw === '' || raw == null) return ''
      const def = this.getFieldDef(condition.field)
      const opts = def?.options
      if (!opts?.length) return String(raw)
      const isMulti = def && String(def.type || '').toLowerCase() === 'enummulti'
      if (isMulti) {
        const ids = String(raw)
          .split(',')
          .map((s) => s.trim())
          .filter(Boolean)
        if (!ids.length) return ''
        return ids
          .map((id) => {
            const v = id.toLowerCase()
            const found = opts.find((o) => String(o.value).trim().toLowerCase() === v)
            return found ? found.label : id
          })
          .join('、')
      }
      const v = String(raw).trim().toLowerCase()
      const found = opts.find((o) => String(o.value).trim().toLowerCase() === v)
      return found ? found.label : String(raw)
    },
    getValueSelectOptions(condition) {
      const def = this.getFieldDef(condition?.field)
      const opts = def?.options
      if (!opts?.length) return []
      return opts.map((o) => ({ value: o.value, label: o.label }))
    },
    filterSelectOption(input, option) {
      const keyword = String(input || '').trim().toLowerCase()
      if (!keyword) return true
      return String(option?.label ?? '').toLowerCase().includes(keyword)
        || String(option?.value ?? '').toLowerCase().includes(keyword)
    },
    isEnumMultiSelectCondition(condition) {
      const d = this.getFieldDef(condition?.field)
      return Boolean(d && String(d.type || '').toLowerCase() === 'enummulti')
    },
    isPresenceCondition(condition) {
      const d = this.getFieldDef(condition?.field)
      return Boolean(d && String(d.type || '').toLowerCase() === 'presence')
    },
    getMultiSelectValue(condition) {
      const raw = (condition?.value ?? '').trim()
      if (!raw) return []
      return raw.split(',').map((s) => s.trim()).filter(Boolean)
    },
    onMultiSelectValueChange(condition, val) {
      const arr = Array.isArray(val) ? val.filter((x) => x != null && String(x).trim() !== '') : []
      condition.value = arr.length ? arr.map((x) => String(x).trim()).join(',') : ''
    },
    onConditionFieldChange(condition, newFieldKey) {
      const d = this.getFieldDef(newFieldKey)
      const type = String(d?.type || '').toLowerCase()
      const multi = type === 'enummulti'
      if (type === 'presence') {
        condition.operator = '=='
        if (!condition.value) condition.value = 'empty'
      }
      if (!multi && condition.value && String(condition.value).includes(',')) {
        const parts = String(condition.value)
          .split(',')
          .map((s) => s.trim())
          .filter(Boolean)
        condition.value = parts[0] || ''
      }
    },
    async show(index) {
      this.index = index
      this.form = {}
      this.form = JSON.parse(JSON.stringify(this.nodeConfig.conditionNodes[index]))

      // 条件编辑器统一使用后端支持的标准运算符。
      const list = this.form?.conditionList || []
      for (const group of list) {
        for (const c of group || []) {
          c.operator = this.normalizeOperator(c.operator)
        }
      }

      this.drawer = true
      await this.loadConditionFieldDefs()
      this.conditionFieldOptions = (this.conditionFieldDefs || []).map((item) => ({
        value: item.key,
        label: item.label,
      }))
    },
    updateConditionChildNode(item, childNode) {
      item.childNode = childNode
      this.$emit('update:modelValue', this.nodeConfig)
    },
    updateChildNode(childNode) {
      this.nodeConfig.childNode = childNode
      this.$emit('update:modelValue', this.nodeConfig)
    },
    applyWorkflowFragment(fragment) {
      this.form.childNode = fragment
    },
    save() {
      // 保存前再做一次归一化，确保落库/发给后端的运算符合法
      if (this.isFallbackBranch(this.form)) {
        this.form.conditionList = []
      }

      const list = this.form?.conditionList || []
      for (const group of list) {
        for (const c of group || []) {
          c.operator = this.normalizeOperator(c.operator)
        }
      }

      this.swapConditionNodePriority(this.index, Number(this.form.priorityLevel || this.index + 1), this.form)
      this.$emit('update:modelValue', this.nodeConfig)
      this.drawer = false
    },
    isFallbackBranch(branch) {
      return this.index === this.nodeConfig.conditionNodes.length - 1
        || branch?.nodeName === '其他情况'
    },
    isCurrentFallback() {
      return this.isFallbackBranch(this.form)
    },
    addTerm() {
      const fallbackIndex = this.findFallbackIndex()
      const insertIndex = fallbackIndex >= 0 ? fallbackIndex : this.nodeConfig.conditionNodes.length
      this.nodeConfig.conditionNodes.splice(insertIndex, 0, {
        nodeName: '条件' + (insertIndex + 1),
        nodeKey: createWorkflowNodeKey(),
        type: 3,
        priorityLevel: insertIndex + 1,
        conditionMode: 1,
        conditionList: []
      })
      this.normalizeConditionNodePriorities()
      this.$emit('update:modelValue', this.nodeConfig)
    },
    findFallbackIndex() {
      return this.nodeConfig.conditionNodes.findIndex((item) => !item.conditionList || item.conditionList.length === 0)
    },
    normalizeConditionNodePriorities() {
      this.nodeConfig.conditionNodes.forEach((item, index) => {
        item.priorityLevel = index + 1
        if (!item.nodeName || /^条件\d+$/.test(item.nodeName)) {
          item.nodeName = '条件' + (index + 1)
        }
      })
    },
    // 构建同级条件可交换的优先级列表。
    conditionPriorityOptions() {
      const nodes = Array.isArray(this.nodeConfig.conditionNodes) ? this.nodeConfig.conditionNodes : []
      return nodes.map((item, index) => ({
        label: `优先级${index + 1}：${item.nodeName || `条件${index + 1}`}`,
        value: index + 1,
      }))
    },
    // 按目标优先级交换整个条件分支，保留分支下已配置的审批流程。
    swapConditionNodePriority(sourceIndex, targetPriority, sourceBranch) {
      const nodes = this.nodeConfig.conditionNodes
      const targetIndex = targetPriority - 1
      if (!Array.isArray(nodes) || sourceIndex < 0 || sourceIndex >= nodes.length) return

      nodes[sourceIndex] = sourceBranch
      if (targetIndex >= 0 && targetIndex < nodes.length && targetIndex !== sourceIndex) {
        // 交换的是完整分支对象，因此 childNode 会跟随条件一起移动。
        const source = nodes[sourceIndex]
        nodes[sourceIndex] = nodes[targetIndex]
        nodes[targetIndex] = source
      }
      this.normalizeConditionNodePriorities()
    },
    delTerm(index) {
      this.nodeConfig.conditionNodes.splice(index, 1)
      if (this.nodeConfig.conditionNodes.length == 1) {
        if (this.nodeConfig.childNode) {
          if (this.nodeConfig.conditionNodes[0].childNode) {
            this.reData(this.nodeConfig.conditionNodes[0].childNode, this.nodeConfig.childNode)
          } else {
            this.nodeConfig.conditionNodes[0].childNode = this.nodeConfig.childNode
          }
        }
        this.$emit('update:modelValue', this.nodeConfig.conditionNodes[0].childNode)
      }
    },
    reData(data, addData) {
      if (!data.childNode) {
        data.childNode = addData
      } else {
        this.reData(data.childNode, addData)
      }
    },
    arrTransfer(index, type = 1) {
      this.nodeConfig.conditionNodes[index] = this.nodeConfig.conditionNodes.splice(index + type, 1, this.nodeConfig.conditionNodes[index])[0]
      this.normalizeConditionNodePriorities()
      this.$emit('update:modelValue', this.nodeConfig)
    },
    addConditionList(conditionList) {
      conditionList.push({
        label: '',
        field: '',
        operator: '==',
        value: ''
      })
    },
    deleteConditionList(conditionList, index) {
      conditionList.splice(index, 1)
    },
    addConditionGroup() {
      this.addConditionList(this.form.conditionList[this.form.conditionList.push([]) - 1])
    },
    deleteConditionGroup(index) {
      this.form.conditionList.splice(index, 1)
    },
    toText(nodeConfig, index) {
      var { conditionList } = nodeConfig.conditionNodes[index]
      if (conditionList && conditionList.length == 1) {
        const text = conditionList
          .map((conditionGroup) =>
            conditionGroup
              .map(
                (item) =>
                  `${item.label ?? ''}${item.operator}${this.getConditionValueDisplay(item)}`,
              )
              .join(' 且 '),
          )
          .join(' 和 ')
        return text
      } else if (conditionList && conditionList.length > 1) {
        return conditionList.length + '个条件，或满足'
      } else {
        if (index == nodeConfig.conditionNodes.length - 1) {
          return '其他条件进入此流程'
        } else {
          return false
        }
      }
    }
  }
}
</script>

<style scoped lang="scss">
.top-tips {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  color: #646a73;
}

.or-branch-link-tip {
  margin: 10px 0;
  color: #646a73;
}

.condition-group-editor {
  user-select: none;
  border-radius: 4px;
  border: 1px solid #e4e5e7;
  position: relative;
  margin-bottom: 16px;

  .branch-delete-icon {
    font-size: 18px;
  }

  .header {
    background-color: #f4f6f8;
    padding: 0 12px;
    font-size: 14px;
    color: #171e31;
    height: 36px;
    display: flex;
    align-items: center;

    span {
      flex: 1;
    }
  }

  .main-content {
    padding: 0 12px;

    .condition-relation {
      color: #9ca2a9;
      display: flex;
      align-items: center;
      height: 36px;
      display: flex;
      justify-content: space-between;
      padding: 0 2px;
    }

    .condition-content-box {
      display: flex;
      justify-content: space-between;
      align-items: center;

      div {
        width: 100%;
        min-width: 120px;
      }

      div:not(:first-child) {
        margin-left: 16px;
      }
    }

    .cell-box {
      div {
        padding: 16px 0;
        width: 100%;
        min-width: 120px;
        color: #909399;
        font-size: 14px;
        font-weight: 600;
        text-align: center;
      }
    }

    .condition-content {
      display: flex;
      flex-direction: column;

      :deep(.ant-input) {
        border-radius: 2px;
      }

      .content {
        flex: 1;
        padding: 0 0 4px 0;
        display: flex;
        align-items: center;
        min-height: 31.6px;
        flex-wrap: wrap;
      }
    }
  }

  .sub-content {
    padding: 12px;
  }
}
</style>
