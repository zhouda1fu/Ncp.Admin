import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { ProductApi, ProductTypeItem, SupplierItem } from '#/api/system/product';

import type { Ref } from 'vue';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

/** 产品表单下拉选项（产品类型、分类树、供应商列表），由表单打开时加载 */
export interface ProductFormOptions {
  productTypeOptions: Ref<{ value: string; label: string }[]>;
  categoryTreeOptions: Ref<{ value: string; label: string; children?: unknown[] }[]>;
  supplierOptions: Ref<{ value: string; label: string }[]>;
}

/** 状态（布尔）：有效/无效 */
const statusOptions = [
  { value: true, label: '有效' },
  { value: false, label: '无效' },
];

export function useSchema(
  options?: ProductFormOptions,
): VbenFormSchema[] {
  const base: VbenFormSchema[] = [
    // ---------- 两列布局：左列第1行 | 右列第1行 → 左列第7行 | 右列第7行，然后底部全宽 功能特点 ----------
    // 第1行：产品类型* | 分类*
    ...(options
      ? [
          {
            component: 'Select',
            componentProps: {
              class: 'w-full',
              options: options.productTypeOptions,
              placeholder: $t('product.productType'),
            },
            fieldName: 'productTypeId',
            label: $t('product.productType'),
            rules: z.string().min(1, $t('ui.formRules.required', [$t('product.productType')])),
          },
        ]
      : []),
    ...(options
      ? [
          {
            component: 'TreeSelect',
            componentProps: {
              allowClear: true,
              class: 'w-full',
              placeholder: $t('product.category'),
              treeData: options.categoryTreeOptions,
            },
            fieldName: 'categoryId',
            label: $t('product.category'),
            rules: z.string().min(1, $t('ui.formRules.required', [$t('product.category')])),
          },
        ]
      : []),
    // 第2行：状态* | 产品名称*
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        options: statusOptions,
        placeholder: $t('product.status'),
      },
      fieldName: 'status',
      label: $t('product.status'),
      rules: z.boolean().optional().default(true),
    },
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('product.name'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.name')])).max(200),
    },
    // 第3行：产品编号* | 条形码
    {
      component: 'Input',
      fieldName: 'code',
      label: $t('product.code'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.code')])).max(100),
    },
    {
      component: 'Input',
      fieldName: 'barcode',
      label: $t('product.barcode'),
      rules: z.string().max(50),
    },
    // 第4行：激活码 | 型号
    {
      component: 'Input',
      fieldName: 'activationCode',
      label: $t('product.activationCode'),
      rules: z.string().max(200),
    },
    {
      component: 'Input',
      fieldName: 'model',
      label: $t('product.model'),
      rules: z.string().max(100),
    },
    // 第5行：单位* | 数量
    {
      component: 'Input',
      fieldName: 'unit',
      label: $t('product.unit'),
      rules: z.string().min(1, $t('ui.formRules.required', [$t('product.unit')])).max(20),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, precision: 0, class: 'w-full' },
      fieldName: 'qty',
      label: $t('product.qty'),
    },
    // 第6行：标签 | 供应商
    {
      component: 'Input',
      fieldName: 'tags',
      label: $t('product.tags'),
      rules: z.string().max(200),
    },
    ...(options
      ? [
          {
            component: 'Select',
            componentProps: {
              allowClear: true,
              class: 'w-full',
              options: options.supplierOptions,
              placeholder: $t('product.supplier'),
            },
            fieldName: 'supplierId',
            label: $t('product.supplier'),
          },
        ]
      : []),
    // 第7行：价格标准 | 市场销售
    {
      component: 'Input',
      fieldName: 'priceStandard',
      label: $t('product.priceStandard'),
      rules: z.string().max(200),
    },
    {
      component: 'Input',
      fieldName: 'marketSales',
      label: $t('product.marketSales'),
      rules: z.string().max(500),
    },
    // 底部全宽：功能特点（跨两列）
    {
      component: 'Textarea',
      componentProps: { rows: 4, class: 'w-full' },
      fieldName: 'feature',
      label: $t('product.feature'),
      rules: z.string().max(4000),
      formItemClass: 'col-span-2',
    },
    // ---------- 硬件配置、使用说明：上下堆叠，全宽大文本框 ----------
    {
      component: 'Textarea',
      componentProps: { rows: 6, class: 'w-full resize-y' },
      fieldName: 'configuration',
      label: $t('product.configuration'),
      rules: z.string().max(4000),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { rows: 6, class: 'w-full resize-y' },
      fieldName: 'instructions',
      label: $t('product.instructions'),
      rules: z.string().max(4000),
      formItemClass: 'col-span-2',
    },
    // ---------- 操作流程、产品介绍等（样式与使用说明一致；资源为紧邻的上传块） ----------
    {
      component: 'Textarea',
      componentProps: { rows: 6, class: 'w-full resize-y' },
      fieldName: 'installProcess',
      label: $t('product.installProcess'),
      rules: z.string().max(4000),
      formItemClass: 'col-span-2',
    },
    {
      component: 'ProductResourceUpload',
      componentProps: { type: 'operation' },
      fieldName: '_opRes',
      label: '',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { rows: 6, class: 'w-full resize-y' },
      fieldName: 'introduction',
      label: $t('product.introduction'),
      rules: z.string().max(4000),
      formItemClass: 'col-span-2',
    },
    {
      component: 'ProductResourceUpload',
      componentProps: { type: 'introduction' },
      fieldName: '_introRes',
      label: '',
      formItemClass: 'col-span-2',
    },
    // ---------- 第三块：照片 ----------
    {
      component: 'Input',
      fieldName: 'imagePath',
      label: $t('product.imagePath'),
      rules: z.string().max(500),
    },
    // 兼容旧字段（描述、成本价、客户价可放在扩展区域或隐藏）
    {
      component: 'Textarea',
      componentProps: { rows: 2 },
      fieldName: 'description',
      label: $t('product.description'),
      rules: z.string().max(4000),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, precision: 4 },
      fieldName: 'costPrice',
      label: $t('product.costPrice'),
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, precision: 4 },
      fieldName: 'customerPrice',
      label: $t('product.customerPrice'),
    },
  ];
  return base;
}

export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'keyword',
      label: $t('product.keyword'),
    },
  ];
}

/** 供应商列表搜索表单 */
export function useSupplierGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'keyword',
      label: $t('product.keyword'),
    },
  ];
}

/** 产品类型列表表格列 */
export function useProductTypeColumns(
  onActionClick: OnActionClickFn<ProductTypeItem>,
): VxeTableGridOptions<ProductTypeItem>['columns'] {
  return [
    { field: 'name', title: $t('product.productTypeName'), minWidth: 160 },
    { field: 'sortOrder', title: $t('product.productTypeSortOrder'), width: 100, align: 'right' },
    {
      field: 'visible',
      title: $t('product.productTypeVisible'),
      width: 90,
      formatter: ({ cellValue }: { cellValue?: boolean }) => (cellValue ? $t('common.yes') : $t('common.no')),
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('product.productTypeList'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('common.edit') },
          { code: 'delete', text: $t('common.delete') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      title: $t('product.operation'),
      width: 140,
    },
  ];
}

/** 供应商列表表格列 */
export function useSupplierColumns(
  onActionClick: OnActionClickFn<SupplierItem>,
): VxeTableGridOptions<SupplierItem>['columns'] {
  return [
    { field: 'fullName', title: $t('product.supplierFullName'), minWidth: 160 },
    { field: 'shortName', title: $t('product.supplierShortName'), width: 120 },
    { field: 'contact', title: $t('product.supplierContact'), width: 100 },
    { field: 'phone', title: $t('product.supplierPhone'), width: 140 },
    { field: 'email', title: $t('product.supplierEmail'), minWidth: 160 },
    { field: 'address', title: $t('product.supplierAddress'), minWidth: 160 },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'fullName',
          nameTitle: $t('product.supplierList'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('common.edit') },
          { code: 'delete', text: $t('common.delete') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      title: $t('product.operation'),
      width: 140,
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<ProductApi.ProductItem>,
): VxeTableGridOptions<ProductApi.ProductItem>['columns'] {
  return [
    { type: 'checkbox', width: 88 },
    { field: 'name', title: $t('product.name'), minWidth: 160 },
    { field: 'code', title: $t('product.code'), minWidth: 120 },
    { field: 'model', title: $t('product.model'), minWidth: 120 },
    { field: 'unit', title: $t('product.unit'), width: 80 },
    { field: 'qty', title: $t('product.qty'), width: 90, align: 'right' },
    {
      field: 'customerPrice',
      title: $t('product.customerPrice'),
      width: 110,
      align: 'right',
      formatter: ({ cellValue }: { cellValue?: number }) =>
        cellValue != null ? Number(cellValue).toFixed(2) : '',
    },
    {
      align: 'right',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('product.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          { code: 'edit', text: $t('common.edit') },
          { code: 'delete', text: $t('common.delete') },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('product.operation'),
      width: 140,
    },
  ];
}
