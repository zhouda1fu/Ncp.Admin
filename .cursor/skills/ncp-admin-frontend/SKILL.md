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

- `useSchema()`：返回 `VbenFormSchema[]`，用 `z` 做校验，`$t()` 做文案
- `useColumns(onActionClick)`：返回表格列配置，操作列用 `CellOperation`，支持 `edit`、`delete` 及自定义 `code`

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

## 新增功能流程

1. 在 `api/system/` 或相应目录添加 API 与类型
2. 在 `views/{module}/{feature}/` 创建 `data.ts`、`list.vue`、`modules/form.vue`
3. 在 `router/routes/modules/` 添加路由
4. 在 `locales/langs/` 中补充 zh-CN、en-US 文案
5. 如需权限码，在 `constants/permission-codes.ts` 中新增
