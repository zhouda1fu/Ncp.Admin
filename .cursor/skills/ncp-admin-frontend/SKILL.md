---
name: ncp-admin-frontend
description: Follows Vben Admin patterns when developing Ncp.Admin frontend (Vue 3 + Vite + TypeScript + Ant Design Vue). Use when adding or modifying views, API modules, routes, locales, or components in src/frontend/apps/admin-antd.
---

# Ncp.Admin 前端开发规范

在 Ncp.Admin 前端（Vben Admin / Vue 3 + Ant Design Vue）中开发新功能时，按本技能的结构和规范执行。

## 技术栈与路径

- **应用根目录**：`src/frontend/apps/admin-antd/`
- **技术**：Vue 3、Vite、TypeScript、Ant Design Vue、Vben Admin
- **路径别名**：`#/` 指向 `src/`（如 `#/api/system/dept`、`#/views/system/dept/list.vue`）

## 文件结构

| 类型 | 路径 | 说明 |
|------|------|------|
| API | `src/api/system/{feature}.ts` | 按模块划分，与后端接口一一对应 |
| 视图-列表 | `src/views/{module}/{feature}/list.vue` | 主列表页 |
| 视图-表单弹窗 | `src/views/{module}/{feature}/modules/form.vue` | 新建/编辑弹窗 |
| 视图-数据配置 | `src/views/{module}/{feature}/data.ts` | 表格列、表单 Schema |
| 路由 | `src/router/routes/modules/{module}.ts` | 路由模块 |
| 多语言 | `src/locales/langs/{zh-CN,en-US}/{module}.json` | 按模块组织 |
| 适配器 | `src/adapter/` | form、vxe-table 等 |

## API 层规范

- 使用 `namespace` 定义类型：`export namespace SystemDeptApi { export interface SystemDept { ... } }`
- 使用 `requestClient`（来自 `#/api/request`）：`requestClient.get/post/put/delete`
- 函数命名：`getXxxList`、`getXxx`、`createXxx`、`updateXxx`、`deleteXxx`
- 文件末尾集中 `export { ... }`

## 视图层规范

### list.vue（列表页）

- 使用 `Page`、`useVbenVxeGrid`、`useVbenModal`
- `data.ts` 提供 `useColumns(onActionClick)` 和 `useSchema()`
- 表单弹窗通过 `modules/form.vue` 作为 connectedComponent
- 操作：新建、编辑、删除等通过 `onActionClick` 与 `CellOperation` 联动
- 使用 `$t()` 做文案国际化

### modules/form.vue（表单弹窗）

- 使用 `useVbenForm`、`useVbenModal`
- Schema 来自 `../data` 的 `useSchema()`
- `formModalApi.getData<T>()` 获取传入数据，`formModalApi.setData()` 设置编辑/新建数据
- 提交后 `emit('success')` 触发父级刷新

### data.ts

- `useSchema()`：返回 `VbenFormSchema[]`，用 `z` 做校验，`$t()` 做文案；表单项的 `componentProps` 建议统一加 `class: 'w-full'`，保证表单位宽。
- `useGridFormSchema()`：列表页顶部搜索表单的 Schema；**每个表单项的 `componentProps` 必须包含 `class: 'w-full'`**，否则搜索栏会多出一列或操作列后出现空白列。
- `useColumns(onActionClick)`：返回表格列配置，操作列用 `CellOperation`，支持 `edit`、`delete` 及自定义 `code`。**必须有一列不设 `width`**（如最后一列前的备注列，或占位列 `field: '_flex', title: ''`），作为弹性列吸收剩余空间，否则操作列右侧会多出一块空白列（参考 `views/system/dept/data.ts`）；操作列建议 `width: 200`、`attrs` 中提供 `nameField`/`nameTitle`

## 路由规范

- 使用 `PermissionCodes` 做权限控制：`authority: [PermissionCodes.XxxManagement]`
- 父路由可用 `authority` 数组表示“任一权限即可”
- 懒加载：`component: () => import('#/views/...')`
- `meta` 含 `icon`、`title`、`order`

## 多语言规范

- 文案统一放 `locales/langs/zh-CN/*.json` 与 `en-US/*.json`
- 页面使用 `$t('module.sub.key')`
- 新增功能时同步补充 zh-CN 和 en-US

## 常用组件与依赖

- `@vben/common-ui`：`Page`、`useVbenModal`
- `@vben/plugins/vxe-table`：`useVbenVxeGrid`
- `#/adapter/form`：`useVbenForm`、`VbenFormSchema`、`z`
- `#/adapter/vxe-table`：`useVbenVxeGrid`、`OnActionClickParams`
- `ant-design-vue`：`Button`、`message` 等
- `#/locales`：`$t`

## 权限与角色授权（易漏，必须做）

新增**带权限控制**的功能时，除路由里写 `authority` 外，必须同步维护以下两处，否则菜单不显示、角色管理里也无法勾选新权限：

1. **权限码常量**：`src/constants/permission-codes.ts`  
   - 与后端 `PermissionCodes.cs` 保持一致，新增本模块的 `XxxManagement` 及子权限（如 `XxxView`、`XxxCreate` 等）。

2. **权限树（角色授权用）**：`src/utils/permission-tree.ts`  
   - 在 `buildPermissionTree()` 中增加**与后端权限层级一致**的一组节点：父节点为“某管理”，子节点为具体权限（查看/创建/编辑等）。  
   - 角色管理里“编辑角色”的权限树即由此生成，**不在这里加就不会出现授权选项**。

3. **父级菜单显示**：若希望用户只要拥有任一子权限就能看到父菜单，父路由的 `authority` 应写成数组，包含父权限码和所有子权限码，例如：  
   `authority: [PermissionCodes.XxxManagement, PermissionCodes.XxxView, PermissionCodes.XxxCreate, ...]`

## 新增功能流程

1. 在 `api/system/` 或相应目录添加 API 与类型
2. 在 `views/{module}/{feature}/` 创建 `data.ts`、`list.vue`、`modules/form.vue`
3. 在 `router/routes/modules/` 添加路由（含 `meta.authority`）
4. 在 `locales/langs/` 中补充 zh-CN、en-US 文案
5. **若该功能需要权限控制**：在 `constants/permission-codes.ts` 新增权限码，并在 **`utils/permission-tree.ts`** 的 `buildPermissionTree()` 中增加对应树节点（否则角色管理无法勾选、菜单可能不显示）
