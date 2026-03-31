import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SystemUserApi } from '#/api/system/user';

import { z } from '#/adapter/form';
import { getDeptTree } from '#/api/system/dept';
import { getRoleList } from '#/api/system/role';
import { uploadFile } from '#/api/system/file';
import { $t } from '#/locales';

/**
 * 获取所有角色列表（用于下拉选择）
 */
async function getAllRolesForSelect() {
  const result = await getRoleList({
    pageIndex: 1,
    pageSize: 1000, // 获取所有角色
    countTotal: false,
  });
  return result.items.map((role) => ({
    label: role.name,
    value: role.roleId,
  }));
}

/** 上传成功后写入 path 到 avatarUrl 的回调，由 form.vue 传入 */
export type SetAvatarUrlPathFn = (path: string) => void;

/**
 * 获取编辑表单的字段配置
 */
export function useFormSchema(setAvatarUrlPath?: SetAvatarUrlPathFn): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.user.userName'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'email',
      label: $t('system.user.email'),
      rules: z.string().email($t('ui.formRules.email')),
    },
    {
      component: 'Input',
      fieldName: 'phone',
      label: $t('system.user.phone'),
      rules: z.string().refine((val) => !val || /^1[3-9]\d{9}$/.test(val), {
        message: $t('ui.formRules.phone'),
      }).optional(),
    },
    {
      component: 'Input',
      fieldName: 'realName',
      label: $t('system.user.realName'),
      rules: 'required',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('system.user.male'), value: '男' },
          { label: $t('system.user.female'), value: '女' },
          { label: $t('system.user.other'), value: '其他' },
        ],
      },
      fieldName: 'gender',
      label: $t('system.user.gender'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'birthDate',
      label: $t('system.user.birthDate'),
    },
    {
      component: 'Input',
      fieldName: 'idCardNumber',
      label: $t('system.user.idCardNumber'),
    },
    {
      component: 'Input',
      fieldName: 'address',
      label: $t('system.user.address'),
    },
    {
      component: 'Input',
      fieldName: 'education',
      label: $t('system.user.education'),
    },
    {
      component: 'Input',
      fieldName: 'graduateSchool',
      label: $t('system.user.graduateSchool'),
    },
    {
      component: 'Input',
      componentProps: { type: 'hidden', class: 'hidden' },
      fieldName: 'avatarUrl',
      label: $t('system.user.avatarUrl'),
      formItemClass: 'hidden',
    },
    {
      component: 'Upload',
      componentProps: {
        accept: 'image/*',
        class: 'w-full',
        listType: 'picture',
        maxCount: 1,
        placeholder: $t('system.user.avatarUploadPlaceholder'),
        customRequest: (options: { file: File | Blob; onSuccess?: (res: unknown) => void; onError?: (e: Error) => void }) => {
          const file = options.file as File;
          uploadFile(file)
            .then((res) => {
              setAvatarUrlPath?.(res.path);
              options.onSuccess?.(res);
            })
            .catch((e) => options.onError?.(e ?? new Error('Upload failed')));
        },
      },
      fieldName: 'avatarFileList',
      label: $t('system.user.avatarUrl'),
      formItemClass: 'sm:col-span-1',
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
        optionType: 'button',
      },
      defaultValue: 1,
      fieldName: 'status',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.status'),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.yes'), value: true },
          { label: $t('common.no'), value: false },
        ],
        optionType: 'button',
      },
      defaultValue: false,
      fieldName: 'notOrderMeal',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.notOrderMeal'),
      rules: z.boolean().optional().default(false),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.yes'), value: true },
          { label: $t('common.no'), value: false },
        ],
        optionType: 'button',
      },
      defaultValue: false,
      fieldName: 'isResigned',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.isResigned'),
      rules: z.boolean().optional().default(false),
    },
    {
      component: 'ApiTreeSelect',
      componentProps: {
        allowClear: true,
        api: getDeptTree,
        class: 'w-full',
        labelField: 'name',
        valueField: 'id',
        childrenField: 'children',
      },
      fieldName: 'deptId',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.dept'),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.yes'), value: true },
          { label: $t('common.no'), value: false },
        ],
        optionType: 'button',
      },
      defaultValue: false,
      fieldName: 'isDeptManager',
      formItemClass: 'sm:col-span-1',
      label: $t('system.dept.manager'),
      rules: z.boolean().optional().default(false),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getAllRolesForSelect,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
        mode: 'multiple',
      },
      fieldName: 'roleIds',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.roles'),
    },
    {
      component: 'InputPassword',
      fieldName: 'password',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.password'),
      rules: z.string().refine((val) => !val || val.length >= 6, {
        message: $t('ui.formRules.minLength', [$t('system.user.password'), 6]),
      }).optional(),
    },
  ];
}

/**
 * 获取列表搜索表单配置
 */
export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'keyword',
      label: $t('system.user.keyword'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
      },
      fieldName: 'status',
      label: $t('system.user.status'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('system.user.employmentActive'), value: false },
          { label: $t('system.user.employmentResigned'), value: true },
        ],
      },
      fieldName: 'isResigned',
      label: $t('system.user.employmentStatus'),
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns<T = SystemUserApi.SystemUser>(
  onActionClick: OnActionClickFn<T>,
  onStatusChange?: (
    newStatus: any,
    row: T,
  ) => PromiseLike<boolean | undefined>,
): VxeTableGridOptions['columns'] {
  return [
    {
      field: 'name',
      title: $t('system.user.userName'),
      width: 150,
    },
    {
      field: 'realName',
      title: $t('system.user.realName'),
      width: 120,
    },
    {
      field: 'email',
      title: $t('system.user.email'),
      width: 200,
    },
    {
      field: 'phone',
      title: $t('system.user.phone'),
      width: 120,
    },
    {
      field: 'gender',
      title: $t('system.user.gender'),
      width: 80,
    },
    {
      field: 'age',
      title: $t('system.user.age'),
      width: 80,
    },
    {
      field: 'deptName',
      title: $t('system.user.dept'),
      width: 150,
    },
    {
      field: 'isResigned',
      title: $t('system.user.isResigned'),
      width: 110,
      formatter: ({ cellValue }) => (cellValue ? $t('common.yes') : $t('common.no')),
    },
    {
      field: 'roles',
      title: $t('system.user.roles'),
      minWidth: 200,
      formatter: ({ cellValue }) => {
        if (Array.isArray(cellValue)) {
          return cellValue.join(', ');
        }
        return cellValue || '';
      },
    },
    {
      cellRender: {
        attrs: { beforeChange: onStatusChange },
        name: onStatusChange ? 'CellSwitch' : 'CellTag',
      },
      field: 'status',
      title: $t('system.user.status'),
      width: 100,
    },
    {
      field: 'createdAt',
      formatter: 'formatDateTime',
      title: $t('system.user.createTime'),
      width: 180,
    },
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.user.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
      },
      field: 'operation',
      fixed: 'right',
      title: $t('system.user.operation'),
      width: 130,
    },
  ];
}
