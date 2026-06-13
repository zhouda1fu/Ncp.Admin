import type { VbenFormSchema } from '#/adapter/form';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SystemLogApi } from '#/api/system/system-log';

const levelOptions = [
  { label: '致命', value: 'Critical' },
  { label: '错误', value: 'Error' },
  { label: '告警', value: 'Warning' },
  { label: '信息', value: 'Information' },
  { label: '调试', value: 'Debug' },
  { label: '跟踪', value: 'Trace' },
];

export function formatLevel(level?: string) {
  return levelOptions.find((x) => x.value === level)?.label ?? level ?? '-';
}

export function levelColor(level?: string) {
  if (level === 'Critical' || level === 'Error') return 'error';
  if (level === 'Warning') return 'warning';
  if (level === 'Information') return 'processing';
  return 'default';
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: levelOptions,
      },
      fieldName: 'level',
      label: '级别',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'category',
      label: '来源',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'keyword',
      label: '关键词',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'traceId',
      label: 'TraceId',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: [
          { label: '有异常', value: true },
          { label: '无异常', value: false },
        ],
      },
      fieldName: 'hasException',
      label: '异常',
    },
    {
      component: 'RangePicker',
      componentProps: { class: 'w-full' },
      fieldName: 'timestamp',
      label: '时间',
    },
  ];
}

export function useColumns(): VxeTableGridOptions<SystemLogApi.SystemLogItem>['columns'] {
  return [
    {
      field: 'timestamp',
      formatter: 'formatDateTime',
      title: '时间',
      width: 180,
    },
    {
      field: 'level',
      slots: { default: 'level' },
      title: '级别',
      width: 90,
    },
    {
      field: 'category',
      title: '来源',
      minWidth: 220,
      showOverflow: 'tooltip',
    },
    {
      field: 'message',
      title: '消息',
      minWidth: 280,
      showOverflow: 'tooltip',
    },
    {
      field: 'requestPath',
      title: '请求路径',
      minWidth: 180,
      showOverflow: 'tooltip',
    },
    {
      field: 'userId',
      title: '用户ID',
      width: 120,
    },
    {
      field: 'traceId',
      title: 'TraceId',
      minWidth: 180,
      showOverflow: 'tooltip',
    },
    {
      field: 'hasException',
      slots: { default: 'exception' },
      title: '异常',
      width: 80,
    },
    {
      align: 'center',
      field: 'operation',
      fixed: 'right',
      showOverflow: false,
      slots: { default: 'action' },
      title: '操作',
      width: 100,
    },
  ];
}
