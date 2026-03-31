import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { CustomerApi } from '#/api/system/customer';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

import { uploadFile } from '#/api/system/file';

const statusOptions = () => [
  { label: $t('customer.statusFollowUpUnclear'), value: 0 },
  { label: $t('customer.statusFollowUpInterested'), value: 1 },
  { label: $t('customer.statusCooperating'), value: 2 },
  { label: $t('customer.statusFormerCooperating'), value: 3 },
];

const natureOptions = () => [
  { label: $t('customer.natureIndividual'), value: 0 },
  { label: $t('customer.naturePrivate'), value: 1 },
  { label: $t('customer.natureStateOwned'), value: 2 },
  { label: $t('customer.natureForeign'), value: 3 },
  { label: $t('customer.natureOther'), value: 4 },
  { label: $t('customer.natureEndCustomer'), value: 5 },
  { label: $t('customer.natureOperator'), value: 6 },
];

/** 省市区级联选项（与公海项目区域一致） */
export type RegionCascaderOption = { label: string; value: string; children?: RegionCascaderOption[] };

/** 行业树选项（父子层级，用于 TreeSelect） */
export type IndustryTreeOption = { label: string; value: string; children?: IndustryTreeOption[] };

/** 上传成功后写入 path 到 businessLicense 的回调，由 form.vue 传入 */
export type SetBusinessLicensePathFn = (path: string) => void;

export function useSchema(
  industryTreeOptions: IndustryTreeOption[],
  customerSourceOptions: { label: string; value: string }[],
  regionTreeOptions: RegionCascaderOption[] = [],
  setBusinessLicensePath?: SetBusinessLicensePathFn,
): VbenFormSchema[] {
  return [
    // 第 1 行：客户来源、状态、所属行业（三列同一行）
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: customerSourceOptions,
        placeholder: $t('customer.customerSourcePlaceholder'),
      },
      fieldName: 'customerSourceId',
      label: $t('customer.customerSource'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.customerSource')])),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: false,
        class: 'w-full',
        options: statusOptions(),
        placeholder: $t('customer.statusPlaceholder'),
      },
      fieldName: 'status',
      label: $t('customer.status'),
      rules: z.number({ required_error: $t('ui.formRules.required', [$t('customer.status')]) }),
    },
    {
      component: 'TreeSelect',
      componentProps: {
        allowClear: true,
        class: 'w-full customer-industry-treeselect',
        fieldNames: { label: 'label', value: 'value', children: 'children' },
        placeholder: $t('customer.selectIndustry'),
        showSearch: true,
        treeCheckable: true,
        treeData: industryTreeOptions,
        treeLine: true,
        treeNodeFilterProp: 'label',
      },
      fieldName: 'industryIds',
      label: $t('customer.industryIds'),
    },
    // 第 2 行：仅两列 - 客户名称、所在区域（省市区，与公海项目区域一致）
    {
      component: 'Input',
      componentProps: { class: 'w-full', placeholder: $t('customer.fullNamePlaceholder') },
      fieldName: 'fullName',
      label: $t('customer.fullName'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('customer.fullName')])),
      formItemClass: 'sm:col-span-1',
    },
    {
      component: 'Cascader',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        changeOnSelect: false,
        options: regionTreeOptions,
        placeholder: $t('customer.locationRegionPlaceholder'),
        showSearch: {
          filter: (inputValue: string, path: { label: string }[]) =>
            path.some((node) => node.label.toLowerCase().includes(inputValue.toLowerCase())),
        },
      },
      fieldName: 'regionCodes',
      label: $t('customer.locationRegion'),
      formItemClass: 'sm:col-span-2',
    },
    // 第 3 行：仅两列 - 客户覆盖区域、公司注册地址（公司注册地址占 2 列以占满行）
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'coverRegion',
      label: $t('customer.coverRegion'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'registerAddress',
      label: $t('customer.registerAddress'),
      formItemClass: 'sm:col-span-2',
    },
    // 第 4 行：公司性质、员工数量
    {
      component: 'Select',
      componentProps: {
        allowClear: false,
        class: 'w-full',
        options: natureOptions(),
        placeholder: $t('customer.naturePlaceholder'),
      },
      fieldName: 'nature',
      label: $t('customer.nature'),
      rules: z.number({ required_error: $t('ui.formRules.required', [$t('customer.nature')]) }),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, class: 'w-full', placeholder: $t('customer.employeeCountPlaceholder') },
      fieldName: 'employeeCount',
      label: $t('customer.employeeCount'),
    },
    // 第 5 行：营业执照上传（占满一行）。path 存 businessLicense，展示用 businessLicenseFileList
    {
      component: 'Input',
      componentProps: { type: 'hidden', class: 'hidden' },
      fieldName: 'businessLicense',
      label: $t('customer.businessLicense'),
      formItemClass: 'hidden',
    },
    {
      component: 'Upload',
      componentProps: {
        class: 'w-full',
        maxCount: 1,
        accept: 'image/*,.pdf',
        listType: 'picture',
        placeholder: $t('customer.businessLicensePlaceholder'),
        customRequest: (options: { file: File | Blob; onSuccess?: (res: unknown) => void; onError?: (e: Error) => void }) => {
          const file = options.file as File;
          uploadFile(file)
            .then((res) => {
              setBusinessLicensePath?.(res.path);
              options.onSuccess?.(res);
            })
            .catch((e) => options.onError?.(e ?? new Error('Upload failed')));
        },
      },
      fieldName: 'businessLicenseFileList',
      label: $t('customer.businessLicense'),
      formItemClass: 'sm:col-span-2',
    },
    // 备注（简称、负责人、联系人等不在前端展示，提交时用默认值或编辑时用接口返回值）
    {
      component: 'Textarea',
      componentProps: {
        class: 'w-full',
        placeholder: $t('customer.remarkPlaceholder'),
        rows: 6,
        autoSize: { minRows: 6, maxRows: 14 },
      },
      fieldName: 'remark',
      label: $t('customer.remark'),
      formItemClass: 'sm:col-span-2',
    },
  ];
}

export function useGridFormSchema(
  customerSourceOptions: { label: string; value: string }[] = [],
): VbenFormSchema[] {
  return [
    { component: 'Input', componentProps: { class: 'w-full' }, fieldName: 'fullName', label: $t('customer.fullName') },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
        options: customerSourceOptions,
      },
      fieldName: 'customerSourceId',
      label: $t('customer.customerSource'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: () => $t('customer.isInSeaYes'), value: true },
          { label: () => $t('customer.isInSeaNo'), value: false },
        ],
        class: 'w-full',
      },
      fieldName: 'isInSea',
      label: $t('customer.isInSea'),
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<CustomerApi.CustomerItem>,
): VxeTableGridOptions<CustomerApi.CustomerItem>['columns'] {
  return [
    { field: 'fullName', title: $t('customer.fullName'), minWidth: 140 },
    { field: 'shortName', title: $t('customer.shortName'), width: 120 },
    { field: 'customerSourceName', title: $t('customer.customerSource'), width: 100 },
    { field: 'ownerDeptName', title: $t('customer.ownerDept'), width: 120 },
    {
      field: 'status',
      title: $t('customer.status'),
      width: 130,
      formatter: ({ cellValue }: { cellValue?: number }) => {
        if (cellValue == null) return '';
        const map: Record<number, string> = {
          0: $t('customer.statusFollowUpUnclear'),
          1: $t('customer.statusFollowUpInterested'),
          2: $t('customer.statusCooperating'),
          3: $t('customer.statusFormerCooperating'),
        };
        return map[cellValue] ?? '';
      },
    },
    {
      field: 'nature',
      title: $t('customer.nature'),
      width: 110,
      formatter: ({ cellValue }: { cellValue?: number }) => {
        if (cellValue == null) return '';
        const map: Record<number, string> = {
          0: $t('customer.natureIndividual'),
          1: $t('customer.naturePrivate'),
          2: $t('customer.natureStateOwned'),
          3: $t('customer.natureForeign'),
          4: $t('customer.natureOther'),
          5: $t('customer.natureEndCustomer'),
          6: $t('customer.natureOperator'),
        };
        return map[cellValue] ?? '';
      },
    },
    { field: 'mainContactName', title: $t('customer.mainContactName'), width: 100 },
    { field: 'mainContactPhone', title: $t('customer.mainContactPhone'), width: 120 },
    {
      field: 'contactCount',
      title: $t('customer.contactCount'),
      width: 90,
    },
    {
      field: 'isKeyAccount',
      title: $t('customer.isKeyAccount'),
      width: 90,
      formatter: ({ cellValue }) => (cellValue ? $t('common.yes') : $t('common.no')),
    },
    {
      field: 'isInSea',
      title: $t('customer.isInSea'),
      width: 80,
      formatter: ({ cellValue }) => (cellValue ? $t('customer.isInSeaYes') : $t('customer.isInSeaNo')),
    },
    {
      formatter: 'formatDateTime',
      field: 'createdAt',
      title: $t('customer.createdAt'),
      width: 170,
    },
    {
      align: 'right',
      cellRender: {
        attrs: { nameField: 'fullName', nameTitle: $t('customer.fullName'), onClick: onActionClick },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('customer.edit') },
          { code: 'share', text: $t('customer.share') },
          { code: 'releaseToSea', text: $t('customer.releaseToSea'), show: (row: CustomerApi.CustomerItem) => !row.isInSea },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('customer.operation'),
      width: 200,
    },
  ];
}
