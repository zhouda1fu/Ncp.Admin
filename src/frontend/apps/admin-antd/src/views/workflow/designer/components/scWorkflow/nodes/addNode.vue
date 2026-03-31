<template>
  <div class="add-node-btn-box">
    <div class="add-node-btn">
      <a-popover placement="rightTop" trigger="click">
        <template #content>
          <div class="add-node-popover-body">
            <ul>
              <li @click="addType(1)">
                <IconifyIcon icon="lucide:user" class="icon icon-approver" />
                <p>审批节点</p>
              </li>
              <li @click="addType(2)">
                <IconifyIcon icon="lucide:send" class="icon icon-send" />
                <p>抄送节点</p>
              </li>
              <li @click="addType(4)">
                <IconifyIcon icon="lucide:git-branch" class="icon icon-branch" />
                <p>条件分支</p>
              </li>
            </ul>
          </div>
        </template>
        <a-button type="primary" shape="circle">
          <template #icon><Plus class="size-4" /></template>
        </a-button>
      </a-popover>
    </div>
  </div>
</template>

<script>
import { Plus } from '@vben/icons';
import { Icon as IconifyIcon } from '@iconify/vue';
import { Button, Popover } from 'ant-design-vue';

export default {
  name: 'AddNode',
  components: { AButton: Button, APopover: Popover, IconifyIcon, Plus },
  props: { modelValue: { type: Object, default: () => ({}) } },
  methods: {
    getNodeKey() { return 'flk' + Date.now(); },
    addType(type) {
      let node = {};
      if (type === 1) {
        node = {
          nodeName: '审核人',
          nodeKey: this.getNodeKey(),
          type: 1,
          setType: 1,
          nodeAssigneeList: [],
          examineLevel: 1,
          directorLevel: 1,
          selectMode: 1,
          termAuto: false,
          term: 0,
          termMode: 1,
          examineMode: 1,
          directorMode: 0,
          childNode: this.modelValue,
        };
      } else if (type === 2) {
        node = {
          nodeName: '抄送人',
          nodeKey: this.getNodeKey(),
          type: 2,
          userSelectFlag: true,
          nodeAssigneeList: [],
          childNode: this.modelValue,
        };
      } else if (type === 4) {
        node = {
          nodeName: '条件路由',
          nodeKey: this.getNodeKey(),
          type: 4,
          conditionNodes: [
            { nodeName: '条件1', nodeKey: this.getNodeKey(), type: 3, priorityLevel: 1, conditionMode: 1, conditionList: [] },
            { nodeName: '条件2', nodeKey: this.getNodeKey(), type: 3, priorityLevel: 2, conditionMode: 1, conditionList: [] },
          ],
          childNode: this.modelValue,
        };
      }
      this.$emit('update:modelValue', node);
    },
  },
};
</script>

<style scoped>
.add-node-popover-body ul { display: flex; gap: 8px; flex-wrap: wrap; list-style: none; padding: 0; margin: 0; }
.add-node-popover-body li { width: 80px; text-align: center; padding: 10px 0; cursor: pointer; }
.add-node-popover-body li .icon { font-size: 24px; display: block; margin: 0 auto 4px; }
.add-node-popover-body li .icon-approver { color: #ff943e; }
.add-node-popover-body li .icon-send { color: hsl(var(--primary)); }
.add-node-popover-body li .icon-branch { color: #15bc83; }
.add-node-popover-body li p { font-size: 12px; margin: 0; }
</style>
