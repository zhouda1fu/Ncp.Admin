<template>
  <a-modal
    v-model:open="dialogVisible"
    :title="titleMap[type - 1]"
    :width="type === 1 ? 680 : 460"
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
                    show-size-changer="false"
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
                  <a-avatar size="small">{{ item.name.substring(0, 1) }}</a-avatar>
                  <label>{{ item.name }}</label>
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
          <div class="sc-user-select__select">
            <div class="sc-user-select__tree">
              <a-spin :spinning="showGrouploading">
                <div class="tree-wrap">
                  <a-tree
                    ref="groupTreeRef"
                    :tree-data="role"
                    :field-names="{ key: roleProps.key, title: roleProps.label, children: 'children' }"
                    v-model:checked-keys="checkedRoleKeys"
                    checkable
                    check-strictly
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
      titleMap: ['人员选择', '角色选择'],
      dialogVisible: false,
      showGrouploading: false,
      showUserloading: false,
      keyword: '',
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
      userScrollRef: null,
    };
  },
  computed: {
    selectedIds() {
      return this.selected.map((t) => t.id);
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
  methods: {
    open(type, data) {
      this.type = type;
      this.value = data || [];
      this.selected = JSON.parse(JSON.stringify(data || []));
      this.dialogVisible = true;
      this.checkedUserKeys = this.selectedIds;
      this.checkedRoleKeys = this.selectedIds;
      if (this.type === 1) {
        this.getGroup();
        this.getUser();
      } else if (this.type === 2) {
        this.getRole();
      }
    },
    async getGroup() {
      this.showGrouploading = true;
      const res = await config.group.apiObj.get();
      this.showGrouploading = false;
      const allNode = { [config.group.props.key]: '', [config.group.props.label]: '所有' };
      res.data.unshift(allNode);
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
      this.$nextTick(() => {
        if (this.userScrollRef) this.userScrollRef.scrollTop = 0;
      });
    },
    async getRole() {
      this.showGrouploading = true;
      const res = await config.role.apiObj.get();
      this.showGrouploading = false;
      this.role = config.role.parseData(res).rows;
    },
    onGroupSelect(keys, { node }) {
      if (!node) return;
      this.keyword = '';
      this.currentPage = 1;
      this.groupId = node[config.group.props.key];
      this.getUser();
    },
    onUserCheck(checkedKeysVal, e) {
      const nodes = e?.checkedNodes || [];
      this.selected = nodes.map((n) => ({
        id: n.key,
        name: n.title ?? n[this.userProps.label],
      }));
    },
    onRoleCheck(checkedKeysVal, e) {
      const nodes = e?.checkedNodes || [];
      this.selected = nodes.map((n) => ({
        id: n.key,
        name: n.title ?? n[this.roleProps.label],
      }));
    },
    paginationChange(page) {
      this.currentPage = page;
      this.getUser();
    },
    search() {
      this.groupId = '';
      this.currentPage = 1;
      this.getUser();
    },
    deleteSelected(index) {
      this.selected.splice(index, 1);
      this.checkedUserKeys = this.selectedIds;
      this.checkedRoleKeys = this.selectedIds;
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
.sc-user-select { display: flex; }
.sc-user-select__left { width: 400px; }
.sc-user-select__search { padding-bottom: 10px; }
.sc-user-select__select {
  display: flex;
  border: 1px solid hsl(var(--border));
  background: hsl(var(--card));
}
.sc-user-select__tree { width: 200px; height: 300px; border-right: 1px solid hsl(var(--border)); }
.tree-wrap { height: 300px; overflow: auto; }
.sc-user-select__user { width: 200px; height: 300px; display: flex; flex-direction: column; }
.sc-user-select__user__list { flex: 1; overflow: auto; min-height: 200px; }
.sc-user-select__user footer { height: 36px; padding-top: 5px; border-top: 1px solid hsl(var(--border)); }
.sc-user-select__toicon { display: flex; justify-content: center; align-items: center; margin: 0 10px; }
.sc-user-select__selected {
  height: 345px; width: 200px; border: 1px solid hsl(var(--border)); background: hsl(var(--card));
}
.sc-user-select__selected header {
  height: 43px; line-height: 43px; border-bottom: 1px solid hsl(var(--border)); padding: 0 15px; font-size: 12px;
}
.sc-user-select__selected ul { height: 300px; overflow: hidden; padding: 0; margin: 0; list-style: none; }
.selected-list-wrap { height: 100%; overflow: auto; }
.sc-user-select__selected li {
  display: flex; align-items: center; justify-content: space-between; padding: 5px 5px 5px 15px; height: 38px;
}
.sc-user-select__selected li .name { display: flex; align-items: center; gap: 8px; }
.sc-user-select__selected li .name .ant-avatar { background: hsl(var(--primary)); }
.sc-user-select__selected li .delete { display: none; }
.sc-user-select__selected li:hover { background: hsl(var(--primary) / 0.08); }
.sc-user-select__selected li:hover .delete { display: inline-block; }
.sc-user-select-role .sc-user-select__left { width: 200px; }
.sc-user-select-role .sc-user-select__tree { border: none; height: 343px; }
.dark .sc-user-select__selected li:hover { background: rgba(0, 0, 0, 0.2); }
</style>
