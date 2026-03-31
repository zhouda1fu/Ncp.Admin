<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type { SystemUserApi } from '#/api/system/user';

import { ColPage } from '@vben/common-ui';
import { computed, onMounted, ref, watch } from 'vue';

import { IconifyIcon } from '@vben/icons';

import { Spin, Tree } from 'ant-design-vue';

import { getDeptTree } from '#/api/system/dept';
import type { SystemDeptApi } from '#/api/system/dept';
import { getPositionList } from '#/api/system/position';
import type { SystemPositionApi } from '#/api/system/position';
import { getUserList } from '#/api/system/user';
import { $t } from '#/locales';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useColumns, useGridFormSchema } from '../user/data';

/** 树节点数据类型 */
interface OrgTreeNode {
  key: string;
  title: string;
  type: 'dept' | 'position';
  id: string;
  deptId?: string;
  children?: OrgTreeNode[];
}

const treeLoading = ref(true);
const treeData = ref<OrgTreeNode[]>([]);
/** key -> { type, id, deptId? } 用于选中时解析 */
const nodeInfoMap = ref<Record<string, { type: 'dept' | 'position'; id: string; deptId?: string }>>({});

const selectedDeptId = ref<string | undefined>();
const selectedPositionId = ref<string | undefined>();

/** 是否有选中节点（部门或岗位），用于决定是否请求用户列表 */
const hasSelection = computed(() => selectedDeptId.value != null || selectedPositionId.value != null);

function buildNodeInfoMap(nodes: OrgTreeNode[], map: Record<string, { type: 'dept' | 'position'; id: string; deptId?: string }>) {
  for (const n of nodes) {
    map[n.key] = { type: n.type, id: n.id, deptId: n.deptId };
    if (n.children?.length) buildNodeInfoMap(n.children, map);
  }
}

/** 将部门树 + 岗位列表 合并为 部门→岗位 树 */
async function loadTree() {
  treeLoading.value = true;
  try {
    const [deptRes, posRes] = await Promise.all([
      getDeptTree(),
      getPositionList({ pageIndex: 1, pageSize: 9999, countTotal: false }),
    ]);
    const depts = Array.isArray(deptRes) ? deptRes : (deptRes as any)?.data ?? [];
    const positions: SystemPositionApi.PositionListItem[] = posRes?.items ?? [];
    const byDeptId = new Map<string, SystemPositionApi.PositionListItem[]>();
    for (const p of positions) {
      const did = p.deptId ?? '';
      if (!byDeptId.has(did)) byDeptId.set(did, []);
      byDeptId.get(did)!.push(p);
    }

    function toOrgTree(nodes: SystemDeptApi.SystemDept[]): OrgTreeNode[] {
      return nodes.map((d) => {
        const deptId = String(d.id ?? '');
        const posList = byDeptId.get(deptId) ?? [];
        const positionChildren: OrgTreeNode[] = posList.map((p) => ({
          key: `position-${p.id}`,
          title: p.name,
          type: 'position' as const,
          id: String(p.id),
          deptId,
        }));
        const subDeptChildren = d.children?.length ? toOrgTree(d.children) : [];
        const children: OrgTreeNode[] = [...positionChildren, ...subDeptChildren];
        return {
          key: `dept-${deptId}`,
          title: d.name,
          type: 'dept' as const,
          id: deptId,
          children: children.length ? children : undefined,
        };
      });
    }

    const tree = toOrgTree(depts);
    treeData.value = tree;
    const map: Record<string, { type: 'dept' | 'position'; id: string; deptId?: string }> = {};
    buildNodeInfoMap(tree, map);
    nodeInfoMap.value = map;
  } finally {
    treeLoading.value = false;
  }
}

const selectedKeys = ref<string[]>([]);

function onTreeSelect(
  _selectedKeys: (string | number)[],
  e: { node: { key: string | number } },
) {
  const key = String(e?.node?.key ?? '');
  if (!key) return;
  const info = nodeInfoMap.value[key];
  if (!info) return;
  selectedKeys.value = [key];
  if (info.type === 'dept') {
    selectedDeptId.value = info.id;
    selectedPositionId.value = undefined;
  } else {
    selectedPositionId.value = info.id;
    selectedDeptId.value = info.deptId;
  }
}

/** 树节点扩展：带 type 供 title 插槽区分部门/岗位 */
interface TreeDataNode {
  key: string;
  title: string;
  type: 'dept' | 'position';
  children?: TreeDataNode[];
}

/** 转为 Ant Design Tree 需要的 key/title/children 结构，并保留 type */
function toTreeData(nodes: OrgTreeNode[]): TreeDataNode[] {
  return nodes.map((n) => ({
    key: n.key,
    title: n.title,
    type: n.type,
    children: n.children?.length ? toTreeData(n.children) : undefined,
  }));
}
const treeDataForTree = computed<TreeDataNode[]>(() => toTreeData(treeData.value));

const [Grid, gridApi] = useVbenVxeGrid<SystemUserApi.SystemUser>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(() => {}, undefined),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          if (!hasSelection.value) {
            return { items: [], total: 0 };
          }
          const params: Recordable<any> = {
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            countTotal: true,
            ...formValues,
          };
          if (selectedPositionId.value) params.positionId = selectedPositionId.value;
          else if (selectedDeptId.value) params.deptId = selectedDeptId.value;
          const result = await getUserList(params);
          return { items: result.items, total: result.total };
        },
      },
    },
    rowConfig: { keyField: 'userId' },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

watch([selectedDeptId, selectedPositionId], () => {
  gridApi.query();
});

onMounted(loadTree);
</script>

<template>
  <ColPage auto-content-height :left-width="28" :right-width="72">
    <template #left>
      <div class="flex h-full flex-col px-2">
        <div class="py-2 text-sm font-medium text-muted-foreground">
          {{ $t('system.orgUsers.treeTitle') }}
        </div>
        <Spin :spinning="treeLoading" class="min-h-[200px] flex-1">
          <Tree
            v-if="treeData.length"
            :selected-keys="selectedKeys"
            :tree-data="treeDataForTree"
            block-node
            class="overflow-auto org-users-tree"
            @select="onTreeSelect"
          >
            <template #title="slotData">
              <span class="inline-flex items-center gap-2">
                <template v-if="(slotData?.type ?? (String(slotData?.key ?? '').startsWith('dept-') ? 'dept' : 'position')) === 'dept'">
                  <IconifyIcon icon="charm:organisation" class="size-4 shrink-0 text-amber-600" />
                  <span class="truncate">{{ slotData?.title }}</span>
                  <span class="shrink-0 rounded px-1.5 py-0 text-xs bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
                    {{ $t('system.dept.name') }}
                  </span>
                </template>
                <template v-else>
                  <IconifyIcon icon="mdi:briefcase-outline" class="size-4 shrink-0 text-blue-600" />
                  <span class="truncate">{{ slotData?.title }}</span>
                  <span class="shrink-0 rounded px-1.5 py-0 text-xs bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">
                    {{ $t('system.position.name') }}
                  </span>
                </template>
              </span>
            </template>
          </Tree>
          <div v-else-if="!treeLoading" class="py-4 text-center text-sm text-muted-foreground">
            {{ $t('system.orgUsers.noTreeData') }}
          </div>
        </Spin>
      </div>
    </template>
    <div class="flex h-full flex-col">
      <div v-if="!hasSelection" class="flex flex-1 items-center justify-center text-muted-foreground">
        {{ $t('system.orgUsers.selectDeptOrPosition') }}
      </div>
      <Grid
        v-else
        :table-title="$t('system.orgUsers.userList')"
      />
    </div>
  </ColPage>
</template>
