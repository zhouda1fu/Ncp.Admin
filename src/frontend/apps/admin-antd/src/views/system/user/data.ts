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
    value: String(role.roleId ?? '').trim(),
  })).filter((item) => item.value);
}

/** 上传成功后写入 path 到 avatarUrl 的回调，由 form.vue 传入 */
export type SetAvatarUrlPathFn = (path: string) => void;
export type UserDeptChangeFn = (deptId?: string) => void;
export type SetAsDeptResponsibleUserChangeFn = (checked: boolean) => void;

export type UserFormMode = 'create' | 'edit';

/**
 * 获取编辑表单的字段配置
 */
export function useFormSchema(
  mode: UserFormMode,
  setAvatarUrlPath?: SetAvatarUrlPathFn,
  onDeptChange?: UserDeptChangeFn,
  onSetAsDeptResponsibleUserChange?: SetAsDeptResponsibleUserChangeFn,
  showResetPassword = false,
): VbenFormSchema[] {
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
    },
    {
      component: 'Input',
      fieldName: 'phone',
      label: $t('system.user.phone'),
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
        // 与 API 字段 notOrderMeal 一致：true=不订餐，false=订餐；文案为「不订餐」故「是」提交 true。
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
      component: 'Checkbox',
      componentProps: { class: 'w-full' },
      defaultValue: false,
      fieldName: 'notAttendanceRequired',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.notAttendanceRequired'),
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
        onChange: onDeptChange,
      },
      fieldName: 'deptId',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.dept'),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        onChange: (event: unknown) => {
          const value =
            typeof event === 'boolean'
              ? event
              : (event as { target?: { value?: unknown } })?.target?.value;
          onSetAsDeptResponsibleUserChange?.(value === true || value === 'true');
        },
        options: [
          { label: $t('common.yes'), value: true },
          { label: $t('common.no'), value: false },
        ],
        optionType: 'button',
      },
      defaultValue: false,
      fieldName: 'setAsDeptResponsibleUser',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.setAsDeptResponsibleUser'),
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
      dependencies: {
        if(values: Record<string, unknown>) {
          return Boolean(values.setAsDeptResponsibleUser);
        },
        triggerFields: ['setAsDeptResponsibleUser'],
      },
      fieldName: 'setAsDefaultDeptResponsibleUser',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.setAsDefaultDeptResponsibleUser'),
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
        showSearch: true,
        optionFilterProp: 'label',
      },
      fieldName: 'roleIds',
      formItemClass: 'sm:col-span-1',
      label: $t('system.user.roles'),
    },
    {
      component: 'Input',
      componentProps: { type: 'hidden', class: 'hidden' },
      fieldName: 'password',
      formItemClass: 'hidden',
      label: $t('system.user.password'),
      rules:
        mode === 'create'
          ? z
              .string()
              .min(1, { message: $t('ui.formRules.required', [$t('system.user.password')]) })
          : z.string().refine((val) => !val || val.length >= 6, {
              message: $t('ui.formRules.minLength', [$t('system.user.password'), 6]),
            }).optional(),
    },
    ...(mode === 'edit' && showResetPassword
      ? ([
          {
            component: 'Checkbox',
            componentProps: { class: 'w-full' },
            defaultValue: false,
            fieldName: 'resetPassword',
            formItemClass: 'sm:col-span-2',
            label: $t('system.user.resetPassword'),
            rules: z.boolean().optional().default(false),
          },
        ] as VbenFormSchema[])
      : []),
  ];
}

/**
 * 获取列表搜索表单配置
 */
export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: {
        class: 'w-full',
      },
      fieldName: 'keyword',
      label: $t('system.user.keyword'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        class: 'w-full',
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
        class: 'w-full',
        options: [
          { label: $t('system.user.employmentActive'), value: false },
          { label: $t('system.user.employmentResigned'), value: true },
        ],
      },
      defaultValue: false,
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
  onStatusChange?: (newStatus: any, row: T) => PromiseLike<boolean | undefined>,
  onResignedChange?: (isResigned: any, row: T) => PromiseLike<boolean | undefined>,
  perms?: {
    canDelete?: () => boolean;
    canEdit?: () => boolean;
  },
): VxeTableGridOptions['columns'] {
  return [
    { type: 'checkbox', width: 48 },
    {
      field: 'name',
      title: $t('system.user.userName'),
      minWidth: 180,
      slots: { default: 'userListUserName' },
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
      visible: false,
      width: 80,
    },
    {
      field: 'deptName',
      title: $t('system.user.dept'),
      minWidth: 170,
      slots: { header: 'columnFilterHeader' },
    },
    {
      cellRender: {
        attrs: { beforeChange: onResignedChange },
        name: onResignedChange ? 'CellSwitch' : 'CellTag',
        props: {
          checkedChildren: $t('common.yes'),
          checkedValue: true,
          unCheckedChildren: $t('common.no'),
          unCheckedValue: false,
        },
      },
      field: 'isResigned',
      title: $t('system.user.isResigned'),
      width: 110,
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
      slots: { header: 'columnFilterHeader' },
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
    { field: '_flex', minWidth: 1, title: '' },
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.user.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          {
            code: 'edit',
            show: () => perms?.canEdit?.() ?? true,
            text: $t('common.edit'),
          },
          {
            code: 'delete',
            show: () => perms?.canDelete?.() ?? true,
            text: $t('common.delete'),
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      showOverflow: false,
      title: $t('system.user.operation'),
      width: 130,
    },
  ];
}
