# CLAUDE.md

本文件为 Claude Code（claude.ai/code）在此仓库中工作时提供指导。

## 构建与运行

```bash
# 推荐：使用 Aspire 编排器（通过 Docker 自动管理 PostgreSQL、Redis、RabbitMQ）
cd src/Ncp.Admin.AppHost && dotnet run

# 或直接运行 Web 项目（需要先启动 docker-compose 基础设施）
cd src/Ncp.Admin.Web && dotnet run

# 前端
cd src/frontend && pnpm install && pnpm dev:antd      # 开发环境运行于 localhost:5666
cd src/frontend && pnpm lint && pnpm check:type        # 代码检查 + 类型检查

# 测试
dotnet test                                           # 全部测试
dotnet test test/Ncp.Admin.Web.Tests                  # 集成测试（TestContainers）
dotnet test test/Ncp.Admin.Domain.Tests               # 领域单元测试
```

## 架构（平台脚手架）

**三项目 DDD 分层架构**，严格遵循 `Web → Infrastructure → Domain` 依赖方向：

| 项目 | 内容 |
|---------|----------|
| `Ncp.Admin.Domain` | 平台聚合：User、Role、Dept、Position、Workflow、Notification、OperationLog、Dashboard |
| `Ncp.Admin.Infrastructure` | EF Core DbContext、实体配置、仓储、基线迁移 `InitPlatform` |
| `Ncp.Admin.Web` | Identity、Workflows、Notifications、OperationLogs、SystemLogs、Files、Dashboard、BackgroundJobs |

**核心模式：**
- **垂直切片**：`Endpoints/{Feature}/` 下的每个功能拥有自己的 endpoint、command、query、validator 和 handler
- **CQRS + MediatR**：命令（写）与查询（读）使用独立的代码路径
- **FastEndpoints** 替代 MVC 控制器 — 端点使用 `record` 类型定义 Request/Response
- **强类型 ID**：所有聚合根 ID 使用 `IInt64StronglyTypedId` 或 `IGuidStronglyTypedId`（禁止直接使用 `long`/`Guid`）
- **工作流引擎**：通用审批链 + `CreateUser` 示范业务适配器；扩展时实现 `IWorkflowBusinessAdapter`
- **KnownException**：所有业务异常必须使用 `KnownException`，禁止使用普通 `Exception`

**基础设施技术栈**：PostgreSQL（EF Core）、Redis（缓存/分布式锁/Hangfire）、RabbitMQ（CAP 集成事件）、SignalR（`NotificationHub` `/notification`）、Prometheus（`/metrics`）

**种子数据**：`PlatformAdminSeeder`（空库创建 `admin` / `Admin@123456`）

## 数据库迁移

```bash
dotnet ef migrations add 迁移名称 -p src/Ncp.Admin.Infrastructure -s src/Ncp.Admin.Web
dotnet ef database update -p src/Ncp.Admin.Infrastructure -s src/Ncp.Admin.Web
```

## 后端规范

- Request/Response 类型始终使用 `record`，并附带 XML `<summary>` 和 `<param>` 文档注释
- 响应使用 `ResponseData<T>` 和 `.AsResponseData()`
- 领域事件处理器实现 `Handle()` 方法（而非 `HandleAsync()`）
- Command 绝不能直接调用 `SaveChanges` — 由 Unit of Work 行为统一处理持久化
- **Domain 聚合实体**：属性非可空；可选强类型 ID 用 `XxxId.Unassigned`（`IGuid` → `Guid.Empty`，`IInt64` → `0`）；可选时间用 `DateTimeOffset.MinValue`；软删字段名为 `IsDeleted`（类型 `Deleted`）。每个 `XxxId` 定义 `Unassigned`。详见 `.cursor/skills/cleanddd-dotnet-coding/SKILL.md`
- 新增受权限保护的端点时，需要更新 **5 个地方**：`PermissionCodes.cs`、`PermissionDefinitionContext.cs`、端点的 `Permissions(...)`、前端 `permission-codes.ts` + `permission-tree.ts`（`PermissionMapper` 自动从定义上下文读取）

## 前端（Vben Admin）

相对于 `src/frontend/apps/admin-antd/` 的路径规范：
- API 定义：`src/api/system/{feature}.ts`（使用 `namespace` 组织类型，使用 `requestClient` 发起请求；函数命名 `getXxxList`/`getXxx`/`createXxx`/`updateXxx`/`deleteXxx`）
- 页面：`src/views/{module}/{feature}/` → `list.vue`、`modules/form.vue`、`data.ts`
- 路由：`src/router/routes/modules/{module}.ts`
- 国际化：`src/locales/langs/{zh-CN,en-US}/{module}.json`
- 组件使用 `useVbenVxeGrid`、`useVbenModal`、`useVbenForm`（Vben Admin 模式）

**易错点：**
- **data.ts — 搜索表单**：`useGridFormSchema()` 中每个表单项的 `componentProps` 必须包含 `class: 'w-full'`，否则搜索栏出现多余空白列
- **data.ts — 列表列定义**：`useColumns()` 中必须有一列不设 `width`（如弹性占位列 `field: '_flex'`），否则操作列右侧多出空白
- **data.ts — 操作列**：建议 `width: 200`，`attrs` 提供 `nameField`/`nameTitle`
- **子页面保持父菜单高亮**：hideInMenu 的子路由的 `meta` 中设置 `activePath` 为父级列表 path（如 `activePath: '/customer/list'`）
- **表单抽屉**：统一使用 `useVbenDrawer + useVbenForm`，不要直接用 Ant Design Vue 的 `Drawer` 组件 + 手写表单项

**新增带权限的功能时，前端必须同步 5 项：**
1. `src/constants/permission-codes.ts` — 新增权限码（与后端 `PermissionCodes.cs` 一致）
2. `src/utils/permission-tree.ts` — `buildPermissionTree()` 中增加树节点（漏了则角色管理无法勾选、菜单不显示）
3. `src/router/routes/modules/` — 添加路由（`meta.authority`；父路由 authority 写成数组包含父+所有子权限码）
4. `src/views/{module}/{feature}/` + `src/api/system/` + `src/locales/langs/` — 页面、API、文案三件套
5. 后端同步：`PermissionCodes.cs`、`PermissionDefinitionContext.cs`、`PermissionMapper.cs`、端点 `Permissions()`、`SeedDatabaseExtension.cs`

> 详细前端规范参见 `.cursor/skills/ncp-admin-frontend/SKILL.md`；CleanDDD 编码详细规范参见 `.cursor/skills/cleanddd-dotnet-coding/SKILL.md`
