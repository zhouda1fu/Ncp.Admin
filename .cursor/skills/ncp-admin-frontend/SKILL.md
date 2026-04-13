---
name: ncp-admin-frontend
description: Follows Vben Admin patterns when developing Ncp.Admin frontend (Vue 3 + Vite + TypeScript + Ant Design Vue). Use when adding or modifying views, API modules, routes, locales, or components in src/frontend/apps/admin-antd.
---

# Ncp.Admin 前端开发规范（详细）

路径与常用约定摘要见 **`.cursor/rules/frontend-vben.mdc`**。下文为易错点、**权限/菜单检查清单**、表单抽屉与路由高亮的完整说明（以本技能为准）。

---

## API 层

- `namespace` 定义类型；`requestClient` 发请求；函数命名：`getXxxList`、`getXxx`、`createXxx`、`updateXxx`、`deleteXxx`；文件末尾集中 `export { ... }`。

---

## 表单抽屉（Drawer）

**带表单的抽屉必须与主题一致**，统一使用 **useVbenDrawer + useVbenForm**，不要使用 Ant Design Vue 的 `Drawer` 组件 + 手写表单项。

- **抽屉壳**：`useVbenDrawer`（来自 `@vben/common-ui`），提供标题、关闭、底部取消/确定、提交中 lock 等，与其它抽屉样式一致。
- **表单内容**：`useVbenForm` + 在 `data.ts` 中定义的 **表单 schema**（与主表单一样用 `VbenFormSchema`、`z` 校验、`formItemClass` 等），保证标签、间距、栅格与主题表单一致。
- **流程**：打开时 `drawerApi.setData({ ... }).open()`，在 `onOpenChange(isOpen)` 里根据 `drawerApi.getData()` 给 `formApi.setValues(...)`；确定时在 `onConfirm` 里 `formApi.validate()` → `formApi.getValues()` → 调接口或写本地状态，成功后 `drawerApi.close()`，提交中用 `drawerApi.lock(true/false)`。
- **参考**：`src/views/contract/form-page.vue` 中的新增/编辑发票抽屉、`src/views/customer/modules/contact-drawer.vue`、`src/views/leave/balances/modules/form.vue`。

---

## 视图层易错点

### data.ts

- **useSchema()**：`VbenFormSchema[]`，`z` 校验，`$t()` 文案；表单项 `componentProps` 建议加 `class: 'w-full'`。
- **useGridFormSchema()**：列表顶部搜索表单；**每个表单项 `componentProps` 必须包含 `class: 'w-full'`**，否则搜索栏多列或操作列后出现空白列。
- **useColumns(onActionClick)**：操作列用 `CellOperation`。**必须有一列不设 `width`**（如备注列或占位列 `field: '_flex', title: ''`）作为弹性列，否则操作列右侧多出空白（参考 `views/system/dept/data.ts`）；操作列建议 `width: 200`，`attrs` 提供 `nameField`/`nameTitle`。

### list.vue / form.vue

- 列表：`Page`、`useVbenVxeGrid`、`useVbenModal`；弹窗用 `modules/form.vue` 作 connectedComponent；`onActionClick` 与 `CellOperation` 联动。
- 表单：`useVbenForm`、`formModalApi.getData<T>()` / `formModalApi.setData()`；提交后 `emit('success')`。

---

## 路由：左侧菜单选中（子页保持父菜单高亮）

"当前选中项"由 **`route.meta?.activePath || route.path`** 决定。若新增的是 **hideInMenu 的子路由**（如新建/编辑页），且希望进入后**左侧仍高亮父级菜单**，在该子路由 `meta` 中设置 **`activePath`** 为父级列表 path：

```ts
meta: {
  activePath: '/customer/list',  // 进入此页时左侧高亮「客户列表」
  hideInMenu: true,
  title: $t('customer.create'),
},
```

---

## 权限与角色授权（易漏，必须做）

新增**带权限控制**的功能时，除路由 `authority` 外，必须同步：

1. **权限码**：`src/constants/permission-codes.ts`，与后端 `PermissionCodes.cs` 一致。
2. **权限树**：`src/utils/permission-tree.ts` 的 `buildPermissionTree()` 中增加与后端层级一致的节点；**不加则角色管理里无法勾选、菜单可能不显示**。
3. **父级菜单**：父路由 `authority` 写成数组，包含父权限码和所有子权限码，这样拥有任一子权限即可看到父菜单。
4. **新菜单项**：若新功能是独立菜单页，除 1–3 外，必须在父路由 `children` 中增加一条路由（path、name、meta、component），并配套 views/api/locales；**只加权限不加子路由，侧边栏不会出现该菜单**。后端需同步：PermissionCodes、PermissionDefinitionContext、PermissionMapper、端点 `Permissions()`、必要时 Seed（见 **`.cursor/rules/project-conventions.mdc`** 中「新增需权限的接口时（后端 5 处）」表格）。

---

## 新增功能流程

1. `api/system/` 或相应目录添加 API 与类型  
2. `views/{module}/{feature}/` 创建 `data.ts`、`list.vue`、`modules/form.vue`  
3. `router/routes/modules/` 添加路由（含 `meta.authority`）  
4. `locales/langs/` 补充 zh-CN、en-US  
5. **需权限时**：在 `permission-codes.ts` 新增，并在 **`permission-tree.ts`** 的 `buildPermissionTree()` 中增加树节点  

---

## 常用引用

- `@vben/common-ui`：`Page`、`useVbenModal`
- `@vben/plugins/vxe-table`：`useVbenVxeGrid`
- `#/adapter/form`：`useVbenForm`、`VbenFormSchema`、`z`
- `#/adapter/vxe-table`：`useVbenVxeGrid`、`OnActionClickParams`
- 文案：`$t()`，键放在 `locales/langs/{zh-CN,en-US}/*.json`
