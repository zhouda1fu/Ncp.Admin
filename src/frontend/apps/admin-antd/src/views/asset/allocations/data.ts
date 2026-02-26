import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { AssetApi } from '#/api/system/asset';

import { $t } from '#/locales';

export function useColumns(
  onActionClick?: OnActionClickFn<AssetApi.AllocationItem>,
): VxeTableGridOptions<AssetApi.AllocationItem>['columns'] {
  return [
    { field: 'assetCode', title: $t('asset.code'), width: 120 },
    { field: 'assetName', title: $t('asset.name'), minWidth: 120 },
    { field: 'userId', title: $t('asset.userId'), width: 100 },
    {
      formatter: 'formatDateTime',
      field: 'allocatedAt',
      title: $t('asset.allocatedAt'),
      width: 170,
    },
    {
      formatter: 'formatDateTime',
      field: 'returnedAt',
      title: $t('asset.returnedAt'),
      width: 170,
    },
    { field: 'note', title: $t('asset.note'), minWidth: 100 },
    {
      align: 'right',
      cellRender: {
        attrs: { nameField: 'assetName', nameTitle: $t('asset.name'), onClick: onActionClick },
        name: 'CellOperation',
        options: [
          {
            code: 'return',
            text: $t('asset.return'),
            show: (row: AssetApi.AllocationItem) => !row.returnedAt,
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('asset.operation'),
      width: 100,
    },
  ];
}
