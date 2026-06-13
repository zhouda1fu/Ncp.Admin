<script lang="ts" setup>
import type { Recordable } from '@vben/types';
import type {
  OnActionClickParams,
} from '#/adapter/vxe-table';
import type { SystemDeptApi } from '#/api/system/dept';

import { computed, nextTick, ref, watch } from 'vue';

import { Page, useVbenModal } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { useAccessStore } from '@vben/stores';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteDept, getDeptTree, reorderDeptSort } from '#/api/system/dept';
import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';
import { handleVxeCellDblclick } from '#/utils/vxe-row-navigation';

import {
  buildSiblingOrderAfterDrag,
  filterDeptTree,
  flattenDeptTree,
  getSiblingOrderedIds,
  normalizeDeptId,
  normalizeDeptParentId,
  useColumns,
  useGridFormSchema,
} from './data';
import Form from './modules/form.vue';

const accessStore = useAccessStore();
const canEdit = computed(
  () => accessStore.accessCodes?.includes(PermissionCodes.DeptEdit) ?? false,
);

const dragSortEnabled = ref(true);
const reordering = ref(false);
const siblingOrderBeforeDrag = ref<string[]>([]);
/** 最近一次查询返回的铺平部门数据（vxe 树表 fullData 不可靠，排序以此为准） */
const lastFlatDeptRows = ref<SystemDeptApi.SystemDept[]>([]);

function hasDeptSearchFilters(formValues?: Recordable<any>) {
  const name = typeof formValues?.name === 'string' ? formValues.name.trim() : '';
  const status = formValues?.status;
  return (
    !!name ||
    (status !== undefined && status !== null && status !== '')
  );
}

function needIncludeInactiveDepts(formValues?: Recordable<any>) {
  return formValues?.status === 0 || formValues?.status === '0';
}

async function expandDeptTreeAfterSearch(formValues?: Recordable<any>) {
  if (!hasDeptSearchFilters(formValues)) {
    return;
  }
  await nextTick();
  gridApi.grid?.setAllTreeExpand?.(true);
}

const [FormModal, formModalApi] = useVbenModal({
  connectedComponent: Form,
  destroyOnClose: true,
});

function onRowDblclick(row: SystemDeptApi.SystemDept) {
  if (!canEdit.value) return;
  onEdit(row);
}

function onEdit(row: SystemDeptApi.SystemDept) {
  formModalApi.setData(row).open();
}

function onAppend(row: SystemDeptApi.SystemDept) {
  formModalApi.setData({ parentId: row.id }).open();
}

function onCreate() {
  formModalApi.setData(null).open();
}

function onDelete(row: SystemDeptApi.SystemDept) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  deleteDept(row.id)
    .then(() => {
      message.success({
        content: $t('ui.actionMessage.deleteSuccess', [row.name]),
        key: 'action_process_msg',
      });
      refreshGrid();
    })
    .catch(() => {
      hideLoading();
    });
}

function onActionClick({
  code,
  row,
}: OnActionClickParams<SystemDeptApi.SystemDept>) {
  switch (code) {
    case 'append': {
      onAppend(row);
      break;
    }
    case 'delete': {
      onDelete(row);
      break;
    }
    case 'edit': {
      onEdit(row);
      break;
    }
  }
}

function updateDragSortEnabled(formValues?: Recordable<any>) {
  dragSortEnabled.value = canEdit.value && !hasDeptSearchFilters(formValues);
}

function rememberSiblingOrderBeforeDrag(row?: SystemDeptApi.SystemDept) {
  if (!row) {
    return;
  }
  const parentId = normalizeDeptParentId(row.parentId);
  siblingOrderBeforeDrag.value = getSiblingOrderedIds(
    lastFlatDeptRows.value,
    parentId,
  );
}

function resolveDragRow(event: {
  dragRow?: SystemDeptApi.SystemDept;
  oldRow?: SystemDeptApi.SystemDept;
  row?: SystemDeptApi.SystemDept;
}) {
  return event.dragRow ?? event.oldRow ?? event.row;
}

async function persistSiblingOrder(event: {
  dragPos?: 'bottom' | 'top';
  dragRow?: SystemDeptApi.SystemDept;
  newRow?: SystemDeptApi.SystemDept;
  oldRow?: SystemDeptApi.SystemDept;
  row?: SystemDeptApi.SystemDept;
}) {
  const dragRow = resolveDragRow(event);
  if (!dragRow) {
    siblingOrderBeforeDrag.value = [];
    return;
  }

  const parentId = normalizeDeptParentId(dragRow.parentId);
  const before =
    siblingOrderBeforeDrag.value.length > 0
      ? siblingOrderBeforeDrag.value
      : getSiblingOrderedIds(lastFlatDeptRows.value, parentId);
  const targetRow = event.newRow;
  const orderedIds =
    targetRow && before.length > 0
      ? buildSiblingOrderAfterDrag(
          before,
          normalizeDeptId(dragRow.id),
          normalizeDeptId(targetRow.id),
          event.dragPos ?? 'top',
        )
      : null;

  if (!orderedIds || orderedIds.length === 0) {
    siblingOrderBeforeDrag.value = [];
    return;
  }

  if (before.length > 0 && orderedIds.join(',') === before.join(',')) {
    siblingOrderBeforeDrag.value = [];
    return;
  }

  reordering.value = true;
  try {
    await reorderDeptSort(parentId ?? undefined, orderedIds);
    message.success($t('system.dept.reorderSuccess'));
    await gridApi.query();
  } catch {
    message.error($t('system.dept.reorderFailed'));
    await gridApi.query();
  } finally {
    reordering.value = false;
    siblingOrderBeforeDrag.value = [];
  }
}

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridEvents: {
    'cell-dblclick': (event: any) => handleVxeCellDblclick(event, onRowDblclick),
    rowDragstart(event: { row: SystemDeptApi.SystemDept }) {
      rememberSiblingOrderBeforeDrag(event.row);
    },
    async rowDragend(event: {
      dragPos?: 'bottom' | 'top';
      dragRow?: SystemDeptApi.SystemDept;
      newRow?: SystemDeptApi.SystemDept;
      oldRow?: SystemDeptApi.SystemDept;
      row?: SystemDeptApi.SystemDept;
    }) {
      await persistSiblingOrder(event);
    },
  },
  gridOptions: {
    columnConfig: {
      useKey: true,
    },
    columns: useColumns(onActionClick, { dragSort: false }),
    height: 'auto',
    keepSource: true,
    pagerConfig: {
      enabled: false,
    },
    proxyConfig: {
      ajax: {
        query: async (
          _params: unknown,
          formValues: Recordable<any>,
        ) => {
          updateDragSortEnabled(formValues);
          const tree = await getDeptTree(
            needIncludeInactiveDepts(formValues)
              ? { includeInactive: true }
              : undefined,
          );
          const source = !hasDeptSearchFilters(formValues)
            ? tree
            : filterDeptTree(tree ?? [], formValues);
          const flatRows = flattenDeptTree(source ?? []);
          lastFlatDeptRows.value = flatRows;
          return flatRows;
        },
        querySuccess: async () => {
          const formValues =
            gridApi.formApi?.getLatestSubmissionValues?.() ??
            (await gridApi.formApi?.getValues?.());
          updateDragSortEnabled(formValues);
          await expandDeptTreeAfterSearch(formValues);
        },
      },
    },
    rowClassName: () => (canEdit.value ? 'vxe-row-clickable' : ''),
    rowConfig: {
      drag: false,
      keyField: 'id',
      useKey: true,
    },
    rowDragConfig: {
      disabledMethod: () => !dragSortEnabled.value || reordering.value,
      isCrossDrag: false,
      isPeerDrag: true,
      trigger: 'cell',
    },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
    treeConfig: {
      parentField: 'parentId',
      rowField: 'id',
      transform: true,
    },
  },
} as any);

function refreshGrid() {
  gridApi.query();
}

watch(
  canEdit,
  (enabled) => {
    gridApi.setState({
      gridOptions: {
        columns: useColumns(onActionClick, { dragSort: enabled }),
        rowConfig: {
          drag: enabled,
          keyField: 'id',
          useKey: true,
        },
      },
    });
  },
  { immediate: true },
);
</script>
<template>
  <Page auto-content-height>
    <FormModal @success="refreshGrid" />
    <Grid :table-title="$t('system.dept.list')">
      <template #toolbar-tools>
        <Button type="primary" class="inline-flex items-center gap-1" @click="onCreate">
          <Plus class="size-5 shrink-0" />
          {{ $t('ui.actionTitle.create', [$t('system.dept.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
