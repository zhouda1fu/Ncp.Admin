<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { WorkflowApi } from '#/api/system/workflow';

import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { Plus } from '@vben/icons';
import { useAccessStore } from '@vben/stores';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { useListReturnState } from '#/composables/use-list-return-state';
import { collectRouteStringParams, parseNumberQuery, readStringQuery } from '#/utils/list-return-state';
import {
  WORKFLOW_DEFINITION_EXPORT_FORMAT,
  WORKFLOW_DEFINITION_EXPORT_LEGACY_VERSION,
  WORKFLOW_DEFINITION_EXPORT_VERSION,
  createDefinitionNewVersion,
  deleteDefinition,
  exportWorkflowDefinition,
  getDefinitionList,
  importWorkflowDefinition,
  publishDefinition,
} from '#/api/system/workflow';
import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';
import { handleVxeCellDblclick } from '#/utils/vxe-row-navigation';

import { useColumns, useGridFormSchema } from './data';

const LIST_PATH = '/workflow/definitions';
const SEARCH_KEYS = ['name', 'category', 'status'] as const;

const router = useRouter();
const route = useRoute();
const accessStore = useAccessStore();

const { shouldDeferGridAutoLoad, pagerConfig, trackPage, buildReturnQuery, restoreOnMount, clearRestoreKey } = useListReturnState({
  route,
  listPath: LIST_PATH,
  searchKeys: SEARCH_KEYS,
  parseRouteToSearchValues: (query) => {
    const values: Record<string, unknown> = collectRouteStringParams(query, SEARCH_KEYS);
    const statusNum = parseNumberQuery(readStringQuery(query, 'status'));
    if (statusNum !== undefined) values.status = statusNum;
    return values;
  },
});

function hasPermission(code: string) {
  return accessStore.accessCodes?.includes(code) ?? false;
}

const canDeletePublished = () =>
  hasPermission(PermissionCodes.WorkflowDefinitionDeletePublished);

const canExport = () => hasPermission(PermissionCodes.WorkflowDefinitionView);
const canImport = () => hasPermission(PermissionCodes.WorkflowDefinitionCreate);

const importFileInputRef = ref<HTMLInputElement | null>(null);

const [Grid, gridApi] = useVbenVxeGrid<WorkflowApi.WorkflowDefinition>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridEvents: {
    'cell-dblclick': (event: any) => handleVxeCellDblclick(event, onRowDblclick),
  } as any,
  gridOptions: {
    columns: useColumns(onActionClick, canDeletePublished),
    height: 'auto',
    keepSource: true,
    checkboxConfig: { highlight: true },
    pagerConfig,
    proxyConfig: {
      autoLoad: !shouldDeferGridAutoLoad,
      ajax: {
        query: async (
          { page }: { page: { currentPage: number; pageSize: number } },
          formValues: Recordable<any>,
        ) => {
          trackPage(page);
          const result = await getDefinitionList({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            countTotal: true,
            ...formValues,
          });
          return {
            items: result.items,
            total: result.total,
          };
        },
      },
    },
    rowConfig: {
      keyField: 'id',
    },
    rowClassName: () => 'vxe-row-clickable',
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

function getSelectedRows(): WorkflowApi.WorkflowDefinition[] {
  return (
    (((gridApi as any)?.grid?.getCheckboxRecords?.() ?? []) as WorkflowApi.WorkflowDefinition[]) ??
    []
  );
}

function sanitizeFileBaseName(name: string): string {
  return String(name || 'workflow')
    .replace(/[/\\?*:|"<>]/g, '_')
    .slice(0, 80);
}

function resolveDesignerSchemaJson(definition: Record<string, any>): string {
  const jsonField = definition.designerSchemaJson ?? definition.DesignerSchemaJson;
  if (typeof jsonField === 'string' && jsonField.trim()) {
    return jsonField;
  }
  if (jsonField && typeof jsonField === 'object') {
    return JSON.stringify(jsonField);
  }
  const schema = definition.designerSchema ?? definition.DesignerSchema;
  if (schema && typeof schema === 'object') {
    return JSON.stringify(schema);
  }
  return '';
}

function isImportUpdatedAction(action: unknown): boolean {
  return action === 'Updated' || action === 1 || action === '1';
}

async function reloadListAfterImport() {
  await gridApi.formApi?.resetForm?.();
  await gridApi.formApi?.setLatestSubmissionValues?.({});
  await gridApi.reload();
}

async function saveJsonFile(content: string, suggestedName: string) {
  const blob = new Blob([content], { type: 'application/json;charset=utf-8' });
  const w = window as any;
  if (typeof w.showSaveFilePicker === 'function') {
    try {
      const handle = await w.showSaveFilePicker({
        suggestedName,
        types: [
          {
            description: 'JSON',
            accept: { 'application/json': ['.json'] },
          },
        ],
      });
      const writable = await handle.createWritable();
      await writable.write(blob);
      await writable.close();
      return;
    } catch (e: any) {
      if (e?.name === 'AbortError') return;
    }
  }
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = suggestedName;
  a.rel = 'noopener';
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}

async function onExport() {
  const rows = getSelectedRows();
  if (rows.length !== 1) {
    message.warning($t('system.workflow.definition.selectOneToExport'));
    return;
  }
  const row = rows[0]!;
  const hide = message.loading({ content: '…', duration: 0, key: 'wf_export' });
  try {
    const doc = await exportWorkflowDefinition(row.id);
    const text = JSON.stringify(doc, null, 2);
    const base = sanitizeFileBaseName(doc.definition?.name ?? row.name);
    await saveJsonFile(text, `${base}-workflow-definition.json`);
    message.success({ content: $t('system.workflow.definition.exportSuccess'), key: 'wf_export' });
    (gridApi as any)?.grid?.clearCheckboxRow?.();
  } finally {
    hide();
  }
}

function onPickImportFile() {
  importFileInputRef.value?.click();
}

async function onImportFileChange(ev: Event) {
  const input = ev.target as HTMLInputElement;
  const file = input.files?.[0];
  input.value = '';
  if (!file) return;
  const hide = message.loading({ content: '…', duration: 0, key: 'wf_import' });
  try {
    const text = await file.text();
    const parsed = JSON.parse(text) as Record<string, any>;
    if (Array.isArray(parsed?.definitions ?? parsed?.Definitions)) {
      message.error($t('system.workflow.definition.invalidImportSeedFile'));
      return;
    }
    const format = parsed?.format ?? parsed?.Format;
    const version = parsed?.version ?? parsed?.Version;
    const definition = parsed?.definition ?? parsed?.Definition;
    const versionNum = Number(version);
    const supportedVersions = [
      WORKFLOW_DEFINITION_EXPORT_VERSION,
      WORKFLOW_DEFINITION_EXPORT_LEGACY_VERSION,
    ];
    if (
      format !== WORKFLOW_DEFINITION_EXPORT_FORMAT ||
      !supportedVersions.includes(versionNum) ||
      !definition ||
      typeof definition !== 'object'
    ) {
      message.error($t('system.workflow.definition.invalidImportFile'));
      return;
    }
    const name = String(definition.name ?? definition.Name ?? '').trim();
    const category = String(definition.category ?? definition.Category ?? '').trim();
    const description = String(definition.description ?? definition.Description ?? '');
    const designerSchemaJson = resolveDesignerSchemaJson(definition);
    if (!name || !designerSchemaJson.trim()) {
      message.error($t('system.workflow.definition.invalidImportFile'));
      return;
    }
    const result = await importWorkflowDefinition({
      format: WORKFLOW_DEFINITION_EXPORT_FORMAT,
      version: versionNum,
      exportedAt: String(parsed?.exportedAt ?? parsed?.ExportedAt ?? new Date().toISOString()),
      remapStrategy: String(parsed?.remapStrategy ?? parsed?.RemapStrategy ?? 'byName'),
      definition: { name, description, category, designerSchemaJson },
      identityCatalog: parsed?.identityCatalog ?? parsed?.IdentityCatalog,
    });
    const warnings = [
      ...(result.warnings ?? []),
      ...(result.remapReport?.warnings ?? []),
    ];
    if (warnings.length > 0) {
      Modal.warning({
        title: $t('system.workflow.definition.importRemapWarningsTitle'),
        content: warnings.join('\n'),
        width: 520,
      });
    }
    const successKey = isImportUpdatedAction(result.action)
      ? 'system.workflow.definition.importUpdatedSuccess'
      : 'system.workflow.definition.importSuccess';
    message.success($t(successKey, [name]));
    await reloadListAfterImport();
  } catch (e) {
    if (e instanceof SyntaxError) {
      message.error($t('system.workflow.definition.invalidImportFile'));
    }
  } finally {
    hide();
  }
}

function onActionClick(
  e: OnActionClickParams<WorkflowApi.WorkflowDefinition>,
) {
  switch (e.code) {
    case 'delete': {
      onDelete(e.row);
      break;
    }
    case 'edit': {
      onEdit(e.row);
      break;
    }
    case 'view': {
      onView(e.row);
      break;
    }
    case 'newVersion': {
      onCreateNewVersion(e.row);
      break;
    }
    case 'publish': {
      onPublish(e.row);
      break;
    }
  }
}

async function onView(row: WorkflowApi.WorkflowDefinition) {
  clearRestoreKey();
  void router.push({
    path: `/workflow/designer/${row.id}`,
    query: { view: '1', ...(await buildReturnQuery(gridApi)) },
  });
}

async function onEdit(row: WorkflowApi.WorkflowDefinition) {
  if (row.status === 1) {
    message.warning($t('system.workflow.definition.cannotEditPublished'));
    return;
  }
  clearRestoreKey();
  void router.push({
    path: `/workflow/designer/${row.id}`,
    query: await buildReturnQuery(gridApi),
  });
}

function onRowDblclick(row: WorkflowApi.WorkflowDefinition) {
  if (row.status === 0) {
    void onEdit(row);
    return;
  }
  void onView(row);
}

function onDelete(row: WorkflowApi.WorkflowDefinition) {
  const isPublishedOrArchived = row.status === 1 || row.status === 2;
  const doDelete = () => {
    const hideLoading = message.loading({
      content: $t('ui.actionMessage.deleting', [row.name]),
      duration: 0,
      key: 'action_process_msg',
    });
    deleteDefinition(row.id)
      .then(() => {
        message.success({
          content: $t('ui.actionMessage.deleteSuccess', [row.name]),
          key: 'action_process_msg',
        });
        onRefresh();
      })
      .catch(() => {
        hideLoading();
      });
  };

  if (isPublishedOrArchived) {
    Modal.confirm({
      title: '删除已发布流程确认',
      content: `流程定义「${row.name}」已发布或已归档，删除后不可恢复，且可能影响历史流程实例关联。确认删除吗？`,
      okType: 'danger',
      onOk: doDelete,
    });
    return;
  }

  doDelete();
}

function onPublish(row: WorkflowApi.WorkflowDefinition) {
  Modal.confirm({
    content: `确认要发布流程定义「${row.name}」吗？发布后将不能修改。`,
    title: '发布确认',
    async onOk() {
      await publishDefinition(row.id);
      message.success(`流程定义「${row.name}」已发布`);
      onRefresh();
    },
  });
}

function onCreateNewVersion(row: WorkflowApi.WorkflowDefinition) {
  Modal.confirm({
    content: `确认要基于流程定义「${row.name}」创建新版本吗？`,
    title: '创建新版本确认',
    async onOk() {
      const newId = await createDefinitionNewVersion(row.id);
      message.success(`已基于「${row.name}」创建新版本`);
      clearRestoreKey();
      void router.push({
        path: `/workflow/designer/${newId}`,
        query: await buildReturnQuery(gridApi),
      });
    },
  });
}

function onRefresh() {
  void gridApi.reload();
}

async function onDesigner() {
  clearRestoreKey();
  void router.push({ path: '/workflow/designer', query: await buildReturnQuery(gridApi) });
}

onMounted(async () => {
  await restoreOnMount(gridApi);
});
</script>
<template>
  <Page auto-content-height>
    <Grid :table-title="$t('system.workflow.definition.list')">
      <template #toolbar-tools>
        <Button
          v-if="canExport()"
          class="inline-flex items-center gap-1"
          @click="onExport"
        >
          {{ $t('system.workflow.definition.exportDefinition') }}
        </Button>
        <Button
          v-if="canImport()"
          class="inline-flex items-center gap-1"
          @click="onPickImportFile"
        >
          {{ $t('system.workflow.definition.importDefinition') }}
        </Button>
        <input
          v-if="canImport()"
          ref="importFileInputRef"
          type="file"
          class="hidden"
          accept=".json,application/json"
          @change="onImportFileChange"
        />
        <Button
          type="primary"
          class="inline-flex items-center gap-1"
          @click="onDesigner"
        >
          <Plus class="size-5 shrink-0" />
          流程设计器
        </Button>
      </template>
    </Grid>
  </Page>
</template>
