import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { WorkflowApi } from '#/api/system/workflow';

import { $t } from '#/locales';

/** 流程分类选项（统一维护） */
export function useCategoryOptions() {
  return [
    { label: $t('system.workflow.category.userManagement'), value: 'UserManagement' },
    { label: $t('system.workflow.category.roleManagement'), value: 'RoleManagement' },
    { label: $t('system.workflow.category.leaveRequest'), value: 'LeaveRequest' },
    { label: $t('system.workflow.category.purchaseOrder'), value: 'PurchaseOrder' },
    { label: $t('system.workflow.category.reimbursement'), value: 'Reimbursement' },
    { label: $t('system.workflow.category.general'), value: 'General' },
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

const statusMap: Record<number, { color: string; label: string }> = {
  0: { color: 'default', label: '' },
  1: { color: 'success', label: '' },
  2: { color: 'warning', label: '' },
};

function getStatusLabel(status: number): string {
  const labels: Record<number, string> = {
    0: $t('system.workflow.definition.statusDraft'),
    1: $t('system.workflow.definition.statusPublished'),
    2: $t('system.workflow.definition.statusArchived'),
  };
  return labels[status] ?? '';
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
        props: (row: { status: number }) => {
          const info = statusMap[row.status];
          return {
            color: info?.color ?? 'default',
          };
        },
      },
      field: 'status',
      formatter: ({ row }: { row: { status: number } }) =>
        getStatusLabel(row.status),
      title: $t('system.workflow.definition.status'),
      width: 100,
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
          'edit',
          {
            code: 'publish',
            text: $t('system.workflow.definition.publish'),
            show: (row: WorkflowApi.WorkflowDefinition) => row.status === 0,
          },
          'delete',
        ],
      },
      field: 'operation',
      fixed: 'right',
      title: $t('system.workflow.definition.operation'),
      width: 230,
    },
  ];
}
