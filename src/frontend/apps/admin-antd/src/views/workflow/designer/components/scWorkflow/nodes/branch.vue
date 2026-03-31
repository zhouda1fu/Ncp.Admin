<template>
  <div class="branch-wrap">
    <div class="branch-box-wrap">
      <div class="branch-box">
        <a-button
          class="add-branch"
          type="primary"
          @click="addTerm">
          添加条件
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
              <add-node v-model="item.childNode" />
            </div>
          </div>
          <slot v-if="item.childNode" :node="item" />
          <div class="top-left-cover-line" v-if="index == 0" />
          <div class="bottom-left-cover-line" v-if="index == 0" />
          <div class="top-right-cover-line" v-if="index == nodeConfig.conditionNodes.length - 1" />
          <div class="bottom-right-cover-line" v-if="index == nodeConfig.conditionNodes.length - 1" />
        </div>
      </div>
      <add-node v-model="nodeConfig.childNode" />
    </div>
    <a-drawer
      v-model:open="drawer"
      title="条件设置"
      :width="600"
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
              placeholder="如：金额>1万、销售部"
            />
          </a-form-item>
        </a-form>
        <div class="top-tips">满足以下条件时进入当前分支</div>
        <template v-for="(conditionGroup, conditionGroupIdx) in form.conditionList" :key="conditionGroupIdx">
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
                    :loading="conditionFieldsLoading" />
                  <a-select v-model:value="condition.operator" placeholder="运算符" style="min-width: 100px">
                    <a-select-option value="==">等于</a-select-option>
                    <a-select-option value="!=">不等于</a-select-option>
                    <a-select-option value=">">大于</a-select-option>
                    <a-select-option value=">=">大于等于</a-select-option>
                    <a-select-option value="<">小于</a-select-option>
                    <a-select-option value="<=">小于等于</a-select-option>
                    <a-select-option value="include">包含</a-select-option>
                    <a-select-option value="notinclude">不包含</a-select-option>
                  </a-select>
                  <a-select
                    v-if="getValueSelectOptions(condition).length"
                    v-model:value="condition.value"
                    allow-clear
                    placeholder="请选择"
                    style="min-width: 180px"
                    :options="getValueSelectOptions(condition)" />
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
        <a-button type="dashed" block @click="addConditionGroup">添加条件组</a-button>
      </div>
      <template #footer>
        <a-button v-if="!viewOnly" type="primary" @click="save">保存</a-button>
        <a-button @click="drawer = false">取消</a-button>
      </template>
    </a-drawer>
  </div>
</template>

<script>
import { ChevronLeft, ChevronRight, X } from '@vben/icons';
import { Icon as IconifyIcon } from '@iconify/vue';
import { Button, Drawer, Form, Input, Select } from 'ant-design-vue';

import { getConditionFields } from '#/api/system/workflow';

import addNode from './addNode.vue';

export default {
  name: 'BranchNode',
  components: {
    addNode,
    AButton: Button,
    ADrawer: Drawer,
    AForm: Form,
    AFormItem: Form.Item,
    AInput: Input,
    ASelect: Select,
    ASelectOption: Select.Option,
    ChevronLeft,
    ChevronRight,
    IconifyIcon,
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
      isEditTitle: false,
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
    }
  },
  mounted() {
    this.nodeConfig = this.modelValue
  },
  methods: {
    getFieldDef(fieldKey) {
      return (this.conditionFieldDefs || []).find((d) => d.key === fieldKey)
    },
    getValueSelectOptions(condition) {
      const def = this.getFieldDef(condition?.field)
      const opts = def?.options
      if (!opts?.length) return []
      return opts.map((o) => ({ value: o.value, label: o.label }))
    },
    async show(index) {
      this.index = index
      this.form = {}
      this.form = JSON.parse(JSON.stringify(this.nodeConfig.conditionNodes[index]))
      this.drawer = true
      this.conditionFieldOptions = []
      if (this.category) {
        this.conditionFieldsLoading = true
        try {
          const list = await getConditionFields(this.category)
          this.conditionFieldDefs = list || []
          this.conditionFieldOptions = (list || []).map((item) => ({
            value: item.key,
            label: item.label,
          }))
        } finally {
          this.conditionFieldsLoading = false
        }
      }
    },
    editTitle() {
      this.isEditTitle = true
      this.$nextTick(() => this.$refs.nodeTitleRef?.focus())
    },
    saveTitle() {
      this.isEditTitle = false
    },
    save() {
      this.nodeConfig.conditionNodes[this.index] = this.form
      this.$emit('update:modelValue', this.nodeConfig)
      this.drawer = false
    },
    addTerm() {
      let len = this.nodeConfig.conditionNodes.length + 1
      this.nodeConfig.conditionNodes.push({
        nodeName: '条件' + len,
        type: 3,
        priorityLevel: len,
        conditionMode: 1,
        conditionList: []
      })
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
      this.nodeConfig.conditionNodes.map((item, index) => {
        item.priorityLevel = index + 1
      })
      this.$emit('update:modelValue', this.nodeConfig)
    },
    addConditionList(conditionList) {
      conditionList.push({
        label: '',
        field: '',
        operator: '=',
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
        const text = conditionList.map((conditionGroup) => conditionGroup.map((item) => `${item.label}${item.operator}${item.value}`)).join(' 和 ')
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
