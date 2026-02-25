import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { DocumentApi } from '#/api/system/document';

import { $t } from '#/locales';

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'title',
      label: $t('document.titleField'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<DocumentApi.DocumentItem>,
): VxeTableGridOptions<DocumentApi.DocumentItem>['columns'] {
  return [
    { field: 'title', title: $t('document.titleField'), minWidth: 200 },
    { field: 'versionCount', title: $t('document.versionCount'), width: 100 },
    {
      field: 'currentVersion',
      title: $t('document.currentVersion'),
      width: 180,
      formatter: ({ cellValue }: { cellValue?: DocumentApi.DocumentVersionItem }) =>
        cellValue ? `${cellValue.fileName} (v${cellValue.versionNumber})` : '-',
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('document.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'title',
          nameTitle: $t('document.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('document.operation'),
      width: 180,
    },
  ];
}
