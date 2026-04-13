import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { WorkflowApi } from '#/api/system/workflow';

import { $t } from '#/locales';

/** 流程分类选项：仅保留用户管理、订单审批 */
export function useCategoryOptions() {
  return [
    { label: $t('system.workflow.category.userManagement'), value: 'CreateUser' },
    { label: $t('system.workflow.category.order'), value: 'Order' },
    { label: $t('system.workflow.category.customerSeaVoid'), value: 'CustomerSeaVoid' },
  ];
}

export function useFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.workflow.definition.flowName'),
      rules: 'required',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: useCategoryOptions(),
        class: 'w-full',
      },
      fieldName: 'category',
      label: $t('system.workflow.definition.category'),
      rules: 'required',
    },
    {
      component: 'Textarea',
      fieldName: 'description',
      label: $t('system.workflow.definition.description'),
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.workflow.definition.flowName'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: useCategoryOptions(),
      },
      fieldName: 'category',
      label: $t('system.workflow.definition.category'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('system.workflow.definition.statusDraft'), value: 0 },
          {
            label: $t('system.workflow.definition.statusPublished'),
            value: 1,
          },
          {
            label: $t('system.workflow.definition.statusArchived'),
            value: 2,
          },
        ],
      },
      fieldName: 'status',
      label: $t('system.workflow.definition.status'),
    },
  ];
}

/** 状态列 Tag 选项：草稿 / 已发布 / 已归档，使用主题色区分 */
function useStatusTagOptions(): Array<{ color: string; label: string; value: number }> {
  return [
    {
      value: 0,
      label: $t('system.workflow.definition.statusDraft'),
      color: 'default',
    },
    {
      value: 1,
      label: $t('system.workflow.definition.statusPublished'),
      color: 'success',
    },
    {
      value: 2,
      label: $t('system.workflow.definition.statusArchived'),
      color: 'warning',
    },
  ];
}

export function useColumns<T = WorkflowApi.WorkflowDefinition>(
  onActionClick: OnActionClickFn<T>,
): VxeTableGridOptions['columns'] {
  return [
    {
      field: 'name',
      title: $t('system.workflow.definition.flowName'),
      width: 200,
    },
    {
      field: 'category',
      title: $t('system.workflow.definition.category'),
      width: 140,
      formatter: ({ cellValue }: { cellValue: string }) => {
        const opt = useCategoryOptions().find((o) => o.value === cellValue);
        return opt?.label ?? cellValue ?? '';
      },
    },
    {
      field: 'version',
      title: $t('system.workflow.definition.version'),
      width: 80,
    },
    {
      cellRender: {
        name: 'CellTag',
        options: useStatusTagOptions(),
      },
      field: 'status',
      title: $t('system.workflow.definition.status'),
      width: 120,
    },
    {
      field: 'description',
      minWidth: 150,
      title: $t('system.workflow.definition.description'),
    },
    {
      field: 'createdAt',
      formatter: 'formatDateTime',
      title: $t('system.workflow.definition.createTime'),
      width: 180,
    },
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.workflow.definition.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'view',
            text: $t('system.workflow.definition.view'),
          },
          {
            code: 'edit',
            text: $t('system.workflow.definition.edit'),
            show: (row: WorkflowApi.WorkflowDefinition) => row.status === 0,
          },
          {
            code: 'newVersion',
            text: '基于此创建新版本',
            show: (row: WorkflowApi.WorkflowDefinition) =>
              row.status === 1 || row.status === 2,
          },
          {
            code: 'publish',
            text: $t('system.workflow.definition.publish'),
            show: (row: WorkflowApi.WorkflowDefinition) => row.status === 0,
          },
          {
            code: 'delete',
            text: $t('common.delete'),
            show: (row: WorkflowApi.WorkflowDefinition) =>
              row.status === 0,
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      title: $t('system.workflow.definition.operation'),
      width: 230,
    },
  ];
}
