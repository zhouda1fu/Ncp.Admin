---
name: ncp-admin-ddd
description: Follows DDD workflow and project conventions when developing Ncp.Admin backend (NetCorePal). Use when adding or modifying aggregates, commands, queries, endpoints, domain events, repositories, or entity configurations in this repository.
---

# Ncp.Admin DDD 开发规范（补充）

开发顺序、文件位置、强制约束见 **.cursor/rules/backend-ddd.mdc** 与 **.cursor/rules/project-conventions.mdc**。本技能仅保留易漏的**权限检查清单**与 **API Request/Response 格式规范**；具体类型写法以 `.cursor/rules/` 下对应的规则文件为准（如 `aggregate.mdc`、`command.mdc`、`query.mdc`、`endpoint.mdc` 等）。

---

## 新增"权限 + 菜单/接口"检查清单（后端，防止漏做）

新增需权限控制的端点时，必须同步以下后端 5 处；前端需同步 `permission-codes.ts`、`permission-tree.ts`、路由 `authority`，若为新菜单页还需在父路由下增加子路由及视图与多语言。

| 步骤 | 位置 | 说明 |
|------|------|------|
| 1 | `AppPermissions/PermissionCodes.cs` | 新增权限常量（与前端 `permission-codes.ts` 一致） |
| 2 | `AppPermissions/PermissionDefinitionContext.cs` | 在对应父权限下 `AddChild(权限码, "显示名")` |
| 3 | `AppPermissions/PermissionMapper.cs` | `_permissionDescriptionMap` 中增加 `{ 权限码, "描述" }` |
| 4 | 各端点 | `Permissions(PermissionCodes.AllApiAccess, PermissionCodes.XxxView)` 等，按接口职责绑定 |
| 5 | `Utils/SeedDatabaseExtension.cs` | 若需默认给管理员：在 `adminPermissionCodes` 中加入新权限码（仅新库或重跑种子时生效） |

---

## 领域事件文件组织（可选优化）

同一聚合/业务下的多个领域事件可放在一个文件里（如 `LeaveDomainEvents.cs` 内含 Created、Submitted、Approved），路径示例：`DomainEvents/{Feature}Events/{Feature}DomainEvents.cs`。

---

## API 端点 Request/Response 格式规范（必须遵守）

新增或修改 `src/Ncp.Admin.Web/Endpoints/**/*.cs` 时，Request/Response 与端点类需按以下格式书写，参考：`Endpoints/Identity/Admin/UserEndpoints/UpdateUserEndpoint.cs`、`Endpoints/Customer/*.cs`。

### 1. Request / Response 使用 record

- **一律使用 record**，不用 class。
- **优先使用位置参数形式**（便于与文档注释一一对应），例如：
  - `public record UpdateXxxRequest(CustomerId Id, UserId? OwnerId, DeptId? DeptId, ...);`
  - `public record CreateXxxResponse(CustomerId Id);`

### 2. 强类型 ID

- 请求/响应中的 ID 必须使用领域强类型，禁止使用裸 `Guid`、`long`、`string`（如 `CustomerId`、`UserId`、`DeptId`、`XxxId` 等）。
- 列表类字段使用 `IReadOnlyList<XxxId>?`，避免 `IList<string>` 再在 Handler 里解析。

### 3. 文档注释（必须）

- **每个 Request/Response record**：上方 `<summary>`，每个参数一行 `<param name="参数名">描述</param>`，与位置参数顺序一致。
- **端点类**：类上方 `<summary>`，主构造函数参数用 `<param name="参数名">描述</param>`；`Configure()`、`HandleAsync()` 可只写 `/// <inheritdoc />`。

### 4. 继承查询入参的 Request

- 若请求体等价于现有查询入参（如分页列表），可继承对应 Input 类型，但**端点类仍需加 summary 与构造函数 param 注释**。

### 5. 检查清单（新增/修改端点时）

- [ ] Request/Response 为 record，且使用强类型 ID（无裸 Guid/long/string）
- [ ] Request/Response 有 `<summary>` 与每个参数的 `<param>`
- [ ] 端点类有 `<summary>`，主构造函数每个依赖有 `<param>`
- [ ] 响应使用 `ResponseData<T>` 包装；发送时使用 `.AsResponseData()`
