<template>
  <a-modal
    v-model:open="dialogVisible"
    :title="titleMap[type - 1]"
    :width="type === 1 ? 760 : 520"
    wrap-class-name="sc-workflow-select-modal"
    destroy-on-close
    get-container="body"
    @after-close="$emit('closed')">
    <template v-if="type === 1">
      <div class="sc-user-select">
        <div class="sc-user-select__left">
          <div class="sc-user-select__search">
            <a-input-search
              v-model:value="keyword"
              placeholder="搜索成员"
              allow-clear
              @change="onKeywordChange"
              @search="search" />
          </div>
          <div class="sc-user-select__select">
            <div class="sc-user-select__tree">
              <a-spin :spinning="showGrouploading">
                <div class="tree-wrap">
                  <a-tree
                    ref="groupTreeRef"
                    :tree-data="group"
                    :field-names="{ key: groupProps.key, title: groupProps.label, children: 'children' }"
                    :selected-keys="groupId ? [groupId] : []"
                    block-node
                    @select="onGroupSelect" />
                </div>
              </a-spin>
            </div>
            <div class="sc-user-select__user">
              <a-spin :spinning="showUserloading">
                <div
                  ref="userScrollRef"
                  class="sc-user-select__user__list">
                  <a-tree
                    ref="userTreeRef"
                    :tree-data="user"
                    :field-names="{ key: userProps.key, title: userProps.label, children: 'children' }"
                    v-model:checked-keys="checkedUserKeys"
                    checkable
                    block-node
                    @check="onUserCheck" />
                </div>
                <footer>
                  <a-pagination
                    v-model:current="currentPage"
                    :total="total"
                    :page-size="pageSize"
                    size="small"
                    simple
                    :show-size-changer="false"
                    @change="paginationChange" />
                </footer>
              </a-spin>
            </div>
          </div>
        </div>
        <div class="sc-user-select__toicon">
          <ChevronRight class="size-4" />
        </div>
        <div class="sc-user-select__selected">
          <header>已选 ({{ selected.length }})</header>
          <ul>
            <div class="selected-list-wrap">
              <li
                v-for="(item, index) in selected"
                :key="item.id">
                <span class="name">
                  <a-avatar size="small">{{ displayName(item).substring(0, 1) }}</a-avatar>
                  <label :title="displayName(item)">{{ displayName(item) }}</label>
                </span>
                <span class="delete">
                  <a-button
                    type="primary"
                    danger
                    size="small"
                    shape="circle"
                    @click="deleteSelected(index)">
                    <template #icon><X class="size-4" /></template>
                  </a-button>
                </span>
              </li>
            </div>
          </ul>
        </div>
      </div>
    </template>

    <template v-if="type === 2">
      <div class="sc-user-select sc-user-select-role">
        <div class="sc-user-select__left">
          <div class="sc-user-select__search">
            <a-input-search
              v-model:value="roleKeyword"
              placeholder="搜索角色"
              allow-clear />
          </div>
          <div class="sc-user-select__select">
            <div class="sc-user-select__tree">
              <a-spin :spinning="showGrouploading">
                <div class="tree-wrap">
                  <a-tree
                    ref="groupTreeRef"
                    :tree-data="filteredRole"
                    :field-names="{ key: roleProps.key, title: roleProps.label, children: 'children' }"
                    v-model:checked-keys="checkedRoleKeys"
                    checkable
                    block-node
                    @check="onRoleCheck" />
                </div>
              </a-spin>
            </div>
          </div>
        </div>
        <div class="sc-user-select__toicon">
          <ChevronRight class="size-4" />
        </div>
        <div class="sc-user-select__selected">
          <header>已选 ({{ selected.length }})</header>
          <ul>
            <div class="selected-list-wrap">
              <li
                v-for="(item, index) in selected"
                :key="item.id">
                <span class="name"><label>{{ item.name }}</label></span>
                <span class="delete">
                  <a-button
                    type="primary"
                    danger
                    size="small"
                    shape="circle"
                    @click="deleteSelected(index)">
                    <template #icon><X class="size-4" /></template>
                  </a-button>
                </span>
              </li>
            </div>
          </ul>
        </div>
      </div>
    </template>

    <template v-if="type === 3">
      <div class="sc-user-select sc-user-select-role">
        <div class="sc-user-select__left">
          <div class="sc-user-select__select">
            <div class="sc-user-select__tree">
              <a-spin :spinning="showGrouploading">
                <div class="tree-wrap">
                  <a-tree
                    ref="deptTreeRef"
                    :tree-data="group"
                    :field-names="{ key: groupProps.key, title: groupProps.label, children: 'children' }"
                    v-model:checked-keys="checkedDeptKeys"
                    checkable
                    block-node
                    @check="onDeptCheck" />
                </div>
              </a-spin>
            </div>
          </div>
        </div>
        <div class="sc-user-select__toicon">
          <ChevronRight class="size-4" />
        </div>
        <div class="sc-user-select__selected">
          <header>已选 ({{ selected.length }})</header>
          <ul>
            <div class="selected-list-wrap">
              <li
                v-for="(item, index) in selected"
                :key="item.id">
                <span class="name"><label>{{ item.name }}</label></span>
                <span class="delete">
                  <a-button
                    type="primary"
                    danger
                    size="small"
                    shape="circle"
                    @click="deleteSelected(index)">
                    <template #icon><X class="size-4" /></template>
                  </a-button>
                </span>
              </li>
            </div>
          </ul>
        </div>
      </div>
    </template>

    <template #footer>
      <a-button @click="dialogVisible = false">取 消</a-button>
      <a-button type="primary" @click="save">确 认</a-button>
    </template>
  </a-modal>
</template>

<script>
import { ChevronRight, X } from '@vben/icons';
import { Avatar, Button, Input, Modal, Pagination, Spin, Tree } from 'ant-design-vue';
import config from '../../config/workflow';

const AInputSearch = Input.Search;

export default {
  name: 'ScWorkflowSelect',
  components: {
    AInputSearch,
    AAvatar: Avatar,
    AButton: Button,
    AModal: Modal,
    APagination: Pagination,
    ASpin: Spin,
    ATree: Tree,
    ChevronRight,
    X,
  },
  props: {
    modelValue: { type: Boolean, default: false },
  },
  emits: ['closed'],
  data() {
    return {
      groupProps: config.group.props,
      userProps: config.user.props,
      roleProps: config.role.props,
      titleMap: ['人员选择', '角色选择', '部门选择'],
      dialogVisible: false,
      showGrouploading: false,
      showUserloading: false,
      keyword: '',
      roleKeyword: '',
      groupId: '',
      pageSize: config.user.pageSize,
      total: 0,
      currentPage: 1,
      group: [],
      user: [],
      role: [],
      type: 1,
      selected: [],
      value: [],
      checkedUserKeys: [],
      checkedRoleKeys: [],
      checkedDeptKeys: [],
      userScrollRef: null,
      searchTimer: null,
    };
  },
  computed: {
    selectedIds() {
      return this.selected.map((t) => String(t.id));
    },
    filteredRole() {
      return this.filterTreeByKeyword(this.role, this.roleKeyword, this.roleProps);
    },
  },
  watch: {
    modelValue(v) {
      this.dialogVisible = v;
    },
    dialogVisible(v) {
      if (!v) this.$emit('closed');
    },
  },
  beforeUnmount() {
    this.clearSearchTimer();
  },
  methods: {
    open(type, data) {
      this.type = type;
      this.value = data || [];
      this.selected = this.normalizeSelected(data || []);
      this.dialogVisible = true;
      this.clearSearchTimer();
      this.roleKeyword = '';
      this.checkedUserKeys = this.selectedIds;
      this.checkedRoleKeys = this.selectedIds;
      this.checkedDeptKeys = this.selectedIds;
      if (this.type === 1) {
        this.getGroup();
        this.getUser();
      } else if (this.type === 2) {
        this.getRole();
      } else if (this.type === 3) {
        this.getGroup();
      }
    },
    async getGroup() {
      this.showGrouploading = true;
      const res = await config.group.apiObj.get();
      this.showGrouploading = false;
      if (this.type === 1) {
        const allNode = { [config.group.props.key]: '', [config.group.props.label]: '所有' };
        res.data.unshift(allNode);
      }
      this.group = config.group.parseData(res).rows;
    },
    async getUser() {
      this.showUserloading = true;
      const params = {
        [config.user.request.keyword]: this.keyword || null,
        [config.user.request.groupId]: this.groupId || null,
        [config.user.request.page]: this.currentPage,
        [config.user.request.pageSize]: this.pageSize,
      };
      const res = await config.user.apiObj.get(params);
      this.showUserloading = false;
      this.user = config.user.parseData(res).rows;
      this.total = config.user.parseData(res).total || 0;
      this.checkedUserKeys = this.selectedIds;
      this.$nextTick(() => {
        if (this.userScrollRef) this.userScrollRef.scrollTop = 0;
      });
    },
    async getRole() {
      this.showGrouploading = true;
      const res = await config.role.apiObj.get();
      this.showGrouploading = false;
      this.role = config.role.parseData(res).rows;
      this.checkedRoleKeys = this.selectedIds;
    },
    onGroupSelect(keys, { node }) {
      if (!node) return;
      this.keyword = '';
      this.currentPage = 1;
      this.groupId = node[config.group.props.key];
      this.getUser();
    },
    onUserCheck(checkedKeysVal) {
      this.selected = this.mergeCheckedTreeItems(this.user, checkedKeysVal, this.userProps);
      this.checkedUserKeys = this.selectedIds;
    },
    onRoleCheck(checkedKeysVal) {
      this.selected = this.collectTopCheckedTreeItems(this.role, checkedKeysVal, this.roleProps);
      this.checkedRoleKeys = this.selectedIds;
    },
    onDeptCheck(checkedKeysVal) {
      this.selected = this.collectTopCheckedTreeItems(this.group, checkedKeysVal, this.groupProps);
      this.checkedDeptKeys = this.selectedIds;
    },
    paginationChange(page) {
      this.currentPage = page;
      this.getUser();
    },
    search() {
      this.clearSearchTimer();
      this.groupId = '';
      this.currentPage = 1;
      this.getUser();
    },
    onKeywordChange() {
      this.clearSearchTimer();
      this.searchTimer = window.setTimeout(() => {
        this.groupId = '';
        this.currentPage = 1;
        this.getUser();
      }, 300);
    },
    clearSearchTimer() {
      if (this.searchTimer) {
        window.clearTimeout(this.searchTimer);
        this.searchTimer = null;
      }
    },
    filterTreeByKeyword(nodes, keyword, props) {
      const text = String(keyword || '').trim().toLowerCase();
      if (!text) return nodes;

      const walk = (items) => (items || [])
        .map((node) => {
          const label = String(node?.[props.label] ?? '').toLowerCase();
          const id = String(node?.[props.key] ?? '').toLowerCase();
          const children = walk(node?.children);
          if (label.includes(text) || id.includes(text) || children.length > 0) {
            return { ...node, children };
          }
          return null;
        })
        .filter(Boolean);

      return walk(nodes);
    },
    deleteSelected(index) {
      this.selected.splice(index, 1);
      this.checkedUserKeys = this.selectedIds;
      this.checkedRoleKeys = this.selectedIds;
      this.checkedDeptKeys = this.selectedIds;
    },
    displayName(item) {
      return item?.realName || item?.label || item?.name || '';
    },
    normalizeSelected(items) {
      const map = new Map();
      (items || []).forEach((item) => {
        if (!item?.id) return;
        map.set(String(item.id), {
          ...item,
          id: String(item.id),
          name: this.displayName(item),
        });
      });
      return [...map.values()];
    },
    /** 兼容 Tree 严格模式和非严格模式的 checkedKeys 返回结构。 */
    normalizeCheckedKeys(checkedKeysVal) {
      const keys = Array.isArray(checkedKeysVal) ? checkedKeysVal : checkedKeysVal?.checked;
      return (keys || []).map((key) => String(key));
    },
    /** 从树中按 checkedKeys 收集完整选项信息，用于右侧已选列表和保存值。 */
    collectCheckedTreeItems(nodes, checkedKeysVal, props) {
      const checkedKeySet = new Set(this.normalizeCheckedKeys(checkedKeysVal));
      const selected = [];
      const walk = (items) => {
        (items || []).forEach((node) => {
          const id = String(node?.[props.key] ?? '');
          if (id && checkedKeySet.has(id)) {
            selected.push({
              id,
              name: String(node?.[props.label] ?? ''),
            });
          }
          // 保持树的自然顺序，父级选中时下级会紧跟着出现在已选列表里。
          if (node?.children?.length) walk(node.children);
        });
      };
      walk(nodes);
      return selected;
    },
    /** 收集最上层已选节点：父级已选时不再把子级重复放入右侧已选列表。 */
    collectTopCheckedTreeItems(nodes, checkedKeysVal, props) {
      const checkedKeySet = new Set(this.normalizeCheckedKeys(checkedKeysVal));
      const selected = [];
      const walk = (items, parentChecked = false) => {
        (items || []).forEach((node) => {
          const id = String(node?.[props.key] ?? '');
          const checked = id ? checkedKeySet.has(id) : false;
          if (id && checked && !parentChecked) {
            selected.push({
              id,
              name: String(node?.[props.label] ?? ''),
            });
          }
          if (node?.children?.length) walk(node.children, parentChecked || checked);
        });
      };
      walk(nodes);
      return selected;
    },
    /** 收集当前树范围内的全部节点ID，用于局部替换而不是清空跨页已选项。 */
    collectTreeIds(nodes, props) {
      const ids = new Set();
      const walk = (items) => {
        (items || []).forEach((node) => {
          const id = String(node?.[props.key] ?? '');
          if (id) ids.add(id);
          if (node?.children?.length) walk(node.children);
        });
      };
      walk(nodes);
      return ids;
    },
    /** 合并当前树的勾选结果，保留不在当前树范围内的历史选择。 */
    mergeCheckedTreeItems(nodes, checkedKeysVal, props) {
      const currentTreeIds = this.collectTreeIds(nodes, props);
      const checkedItems = this.collectTopCheckedTreeItems(nodes, checkedKeysVal, props);
      const selectedMap = new Map();

      // 成员列表存在分页，先保留其他页已选项，再用当前页树的勾选结果覆盖当前页。
      this.selected
        .filter((item) => !currentTreeIds.has(String(item.id)))
        .forEach((item) => selectedMap.set(String(item.id), item));
      checkedItems.forEach((item) => selectedMap.set(String(item.id), item));
      return [...selectedMap.values()];
    },
    save() {
      this.value.splice(0, this.value.length);
      this.selected.forEach((item) => this.value.push(item));
      this.dialogVisible = false;
    },
  },
};
</script>

<style scoped>
.sc-user-select { display: flex; gap: 14px; }
.sc-user-select__left { width: 470px; }
.sc-user-select__search { padding-bottom: 12px; }
.sc-user-select__select {
  display: flex;
  border: 1px solid hsl(var(--border));
  background: hsl(var(--card));
  border-radius: 8px;
  overflow: hidden;
}
.sc-user-select__tree { width: 220px; height: 340px; border-right: 1px solid hsl(var(--border)); }
.tree-wrap { height: 340px; overflow: auto; padding: 4px; }
.sc-user-select__user { width: 250px; height: 340px; display: flex; flex-direction: column; }
.sc-user-select__user__list { flex: 1; overflow: auto; min-height: 200px; }
.sc-user-select__user footer {
  height: 40px;
  padding: 6px 8px 0;
  border-top: 1px solid hsl(var(--border));
  overflow: visible;
}
.sc-user-select__toicon {
  display: flex;
  justify-content: center;
  align-items: center;
  color: hsl(var(--muted-foreground));
}
.sc-user-select__selected {
  height: 394px;
  width: 220px;
  border: 1px solid hsl(var(--border));
  background: hsl(var(--card));
  border-radius: 8px;
  overflow: hidden;
}
.sc-user-select__selected header {
  height: 42px;
  line-height: 42px;
  border-bottom: 1px solid hsl(var(--border));
  padding: 0 14px;
  font-size: 13px;
  font-weight: 600;
}
.sc-user-select__selected ul { height: 352px; overflow: hidden; padding: 0; margin: 0; list-style: none; }
.selected-list-wrap { height: 100%; overflow: auto; }
.sc-user-select__selected li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 7px 8px 7px 12px;
  min-height: 42px;
}
.sc-user-select__selected li .name {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}
.sc-user-select__selected li .name label {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.sc-user-select__selected li .name .ant-avatar { background: hsl(var(--primary)); }
.sc-user-select__selected li .delete { display: none; }
.sc-user-select__selected li:hover { background: hsl(var(--primary) / 0.08); }
.sc-user-select__selected li:hover .delete { display: inline-block; }
.sc-user-select-role .sc-user-select__left { width: 240px; }
.sc-user-select-role .sc-user-select__tree { border: none; width: 240px; height: 360px; }
.sc-user-select-role .tree-wrap { height: 360px; }
.sc-user-select-role .sc-user-select__selected { height: 360px; }
.sc-user-select-role .sc-user-select__selected ul { height: 318px; }
.dark .sc-user-select__selected li:hover { background: rgba(0, 0, 0, 0.2); }

.sc-user-select__user :deep(.ant-spin-nested-loading),
.sc-user-select__user :deep(.ant-spin-container) {
  height: 100%;
}

.sc-user-select__user :deep(.ant-spin-container) {
  display: flex;
  min-height: 0;
  flex-direction: column;
}

:deep(.ant-tree-treenode) {
  width: 100%;
}

:deep(.ant-tree-node-content-wrapper) {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
}

:deep(.ant-tree-title) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sc-user-select__user footer :deep(.ant-pagination) {
  display: flex;
  justify-content: center;
  width: 100%;
  white-space: nowrap;
}

.sc-user-select__user footer :deep(.ant-pagination-simple-pager) {
  flex: 0 0 auto;
}
</style>
