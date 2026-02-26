import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { VehicleApi } from '#/api/system/vehicle';

import { $t } from '#/locales';

function statusLabel(value: number) {
  const map: Record<number, string> = {
    0: 'vehicle.statusBooked',
    1: 'vehicle.statusCancelled',
    2: 'vehicle.statusCompleted',
  };
  return $t(map[value] ?? '');
}

export function useColumns(
  onActionClick?: OnActionClickFn<VehicleApi.BookingItem>,
): VxeTableGridOptions<VehicleApi.BookingItem>['columns'] {
  return [
    { field: 'plateNumber', title: $t('vehicle.plateNumber'), width: 110 },
    { field: 'model', title: $t('vehicle.model'), width: 120 },
    { field: 'purpose', title: $t('vehicle.purpose'), minWidth: 120 },
    {
      formatter: 'formatDateTime',
      field: 'startAt',
      title: $t('vehicle.startAt'),
      width: 170,
    },
    {
      formatter: 'formatDateTime',
      field: 'endAt',
      title: $t('vehicle.endAt'),
      width: 170,
    },
    {
      formatter: ({ cellValue }) => statusLabel(cellValue as number),
      field: 'status',
      title: $t('vehicle.bookingStatus'),
      width: 90,
    },
    {
      align: 'right',
      cellRender: {
        attrs: { nameField: 'purpose', nameTitle: $t('vehicle.purpose'), onClick: onActionClick },
        name: 'CellOperation',
        options: [
          { code: 'cancel', text: $t('vehicle.cancel'), show: (row: VehicleApi.BookingItem) => row.status === 0 },
          { code: 'complete', text: $t('vehicle.complete'), show: (row: VehicleApi.BookingItem) => row.status === 0 },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('vehicle.operation'),
      width: 180,
    },
  ];
}
