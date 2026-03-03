import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ProjectApi } from '#/api/system/project';

import { z } from '#/adapter/form';
import { getCustomerSearch } from '#/api/system/customer';
import { getProjectIndustryList } from '#/api/system/project-industry';
import { getProjectStatusList as getStatusList } from '#/api/system/project-status';
import { getProjectTypeList as getTypeList } from '#/api/system/project-type';
import { $t } from '#/locales';

export type RegionCascaderOption = {
  label: string;
  value: string;
  children?: RegionCascaderOption[];
};

const STATUS_OPTIONS = [
  { label: () => $t('task.project.statusActive'), value: 0 },
  { label: () => $t('task.project.statusArchived'), value: 1 },
];

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'task.project.statusActive',
    1: 'task.project.statusArchived',
  };
  return $t(map[value] ?? '');
}

async function getProjectTypeOptions() {
  const list = await getTypeList();
  return list.map((x) => ({ label: x.name, value: x.id }));
}

async function getProjectStatusOptions() {
  const list = await getStatusList();
  return list.map((x) => ({ label: x.name, value: x.id }));
}

async function getProjectIndustryOptions() {
  const list = await getProjectIndustryList();
  return list.map((x) => ({ label: x.name, value: x.id }));
}

async function getCustomerOptions() {
  const res = await getCustomerSearch({ pageIndex: 1, pageSize: 200 });
  const items = res?.items ?? [];
  return items.map((x) => ({
    label: x.shortName ? `${x.fullName}（${x.shortName}）` : x.fullName,
    value: x.id,
  }));
}

export function useSchema(
  regionTreeOptions: RegionCascaderOption[] = [],
): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: false,
        api: getProjectStatusOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectStatusOptionId',
      label: $t('task.projectStatus.title'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.projectStatus.title')])),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: false,
        api: getProjectTypeOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectTypeId',
      label: $t('task.projectType.title'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.projectType.title')])),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'projectNumber',
      label: $t('task.project.projectNumber'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('task.project.projectName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.project.projectName')])),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: false,
        api: getProjectIndustryOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectIndustryId',
      label: $t('task.projectIndustry.title'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.projectIndustry.title')])),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: false,
        api: getCustomerOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'customerId',
      label: $t('task.project.customer'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('task.project.customer')])),
    },
    {
      component: 'Cascader',
      componentProps: {
        allowClear: true,
        changeOnSelect: false,
        class: 'w-full',
        options: regionTreeOptions,
        placeholder: $t('task.project.regionPlaceholder'),
      },
      fieldName: 'regionIds',
      label: $t('task.project.region'),
      rules: z
        .array(z.string())
        .min(3, $t('ui.formRules.required', [$t('task.project.region')])),
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'startDate',
      label: $t('task.project.startDate'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'projectEstimate',
      label: $t('task.project.projectEstimate'),
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full', min: 0, precision: 2 },
      fieldName: 'purchaseAmount',
      label: $t('task.project.purchaseAmount'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', type: 'textarea', rows: 4 },
      fieldName: 'description',
      label: $t('task.project.description'),
      formItemClass: 'md:col-span-4',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', type: 'textarea', rows: 4 },
      fieldName: 'projectContent',
      label: $t('task.project.projectContent'),
      formItemClass: 'md:col-span-4',
    },
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('task.project.projectName'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: STATUS_OPTIONS,
        class: 'w-full',
      },
      fieldName: 'status',
      label: $t('task.project.status'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getCustomerOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'customerId',
      label: $t('task.project.customer'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getProjectTypeOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectTypeId',
      label: $t('task.projectType.title'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getProjectStatusOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectStatusOptionId',
      label: $t('task.projectStatus.title'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getProjectIndustryOptions,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
      },
      fieldName: 'projectIndustryId',
      label: $t('task.projectIndustry.title'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ProjectApi.ProjectItem>,
): VxeTableGridOptions<ProjectApi.ProjectItem>['columns'] {
  return [
    { field: 'name', title: $t('task.project.projectName'), width: 160 },
    { field: 'projectNumber', title: $t('task.project.projectNumber'), width: 120 },
    { field: 'description', title: $t('task.project.description'), minWidth: 120 },
    { field: 'customerId', title: $t('task.project.customer'), width: 120 },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('task.project.status'),
      width: 90,
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('task.project.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('task.project.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          'edit',
          {
            code: 'archive',
            text: $t('task.project.archive'),
            show: (row: ProjectApi.ProjectItem) => row.status === 0,
          },
          {
            code: 'activate',
            text: $t('task.project.activate'),
            show: (row: ProjectApi.ProjectItem) => row.status === 1,
          },
          'delete',
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('task.project.operation'),
      width: 180,
    },
  ];
}

export { statusLabel };
