---
name: ncp-admin-ddd
description: Follows DDD workflow and project conventions when developing Ncp.Admin backend (NetCorePal). Use when adding or modifying aggregates, commands, queries, endpoints, domain events, repositories, or entity configurations in this repository.
---

# Ncp.Admin DDD 开发规范

在 Ncp.Admin 或同架构后端中开发新功能时，按本技能的工作流和规范执行，并优先查阅项目内指令文件。

## 权威文档

- **总览与规则**： [.github/copilot-instructions.md](../../.github/copilot-instructions.md)
- **分层与顺序**：同上；具体类型开发见 `.github/instructions/*.instructions.md`

按需阅读的指令文件：

| 工作内容 | 指令文件 |
|----------|----------|
| 聚合根、强类型 ID | `.github/instructions/aggregate.instructions.md` |
| 领域事件 | `.github/instructions/domain-event.instructions.md` |
| 仓储 | `.github/instructions/repository.instructions.md` |
| 实体配置 / DbContext | `.github/instructions/entity-configuration.instructions.md` / `.github/instructions/dbcontext.instructions.md` |
| 命令与验证器 | `.github/instructions/command.instructions.md` |
| 查询 | `.github/instructions/query.instructions.md` |
| API 端点 | `.github/instructions/endpoint.instructions.md` |
| 领域事件处理器 | `.github/instructions/domain-event-handler.instructions.md` |
| 集成事件 / 转换器 / 处理器 | `integration-event*.instructions.md` |
| 单元测试 | `.github/instructions/unit-testing.instructions.md` |

## 开发顺序（必须按此顺序）

1. 定义聚合、实体与强类型 ID  
2. 定义领域事件  
3. 创建仓储接口与实现  
4. 配置实体映射（EntityConfiguration）与 DbContext  
5. 定义命令与命令处理器（含验证器）  
6. 定义查询与查询处理器  
7. 定义 Endpoints  
8. 定义领域事件处理器  
9. 如需跨限界上下文：定义集成事件、转换器、处理器  

## 文件位置

| 类型 | 路径 |
|------|------|
| 聚合根 | `src/Ncp.Admin.Domain/AggregatesModel/{AggregateName}Aggregate/` |
| 领域事件 | `src/Ncp.Admin.Domain/DomainEvents/` 或 `DomainEvents/{Feature}Events/`；**同一业务/限界上下文的多个事件可合并在一个文件中**（如 `LeaveEvents/LeaveDomainEvents.cs`、`RoleEvents.cs`），不必每个事件一个文件 |
| 仓储 | `src/Ncp.Admin.Infrastructure/Repositories/` |
| 实体配置 | `src/Ncp.Admin.Infrastructure/EntityConfigurations/` |
| 命令/处理器 | `src/Ncp.Admin.Web/Application/Commands/{Module}/` |
| 查询/处理器 | `src/Ncp.Admin.Web/Application/Queries/` |
| 端点 | `src/Ncp.Admin.Web/Endpoints/{Module}/` |
| 领域事件处理器 | `src/Ncp.Admin.Web/Application/DomainEventHandlers/` |
| 集成事件相关 | `Application/IntegrationEvents/`、`IntegrationEventConverters/`、`IntegrationEventHandlers/` |

## 强制约束（易错点）

- **聚合根**：强类型 ID 与聚合同文件；不手动赋值 ID；有 `protected` 无参构造；状态变更用 `AddDomainEvent()`。
- **命令**：每个命令有验证器；验证器继承 `AbstractValidator<T>`（不要用 `Validator<T>`）；命令处理器**不要**调用 `SaveChanges`。
- **异常**：业务异常使用 `KnownException`，不要用普通 `Exception`。
- **端点**：用特性配置（如 `[HttpPost]`、`[AllowAnonymous]`、`[Tags]`），不用 `Configure()`；用 `Send.OkAsync()`、`.AsResponseData()`；主构造函数注入 `IMediator`。
- **仓储**：全部使用异步方法（如 `GetAsync`、`AddAsync`）；通过构造函数注入的 `ApplicationDbContext` 访问数据。
- **领域事件处理器**：实现 `Handle()`（不是 `HandleAsync()`）。
- **依赖方向**：Web → Infrastructure → Domain，严格单向。
- **权限**：新增需权限控制的端点时，必须同步以下后端 4 处 + 端点绑定 + 可选 Seed；前端需同步 `permission-codes.ts`、`permission-tree.ts`、路由 `authority`，若为新菜单页还需在父路由下增加子路由及视图与多语言。**完整检查清单见下。**

## 新增“权限 + 菜单/接口”检查清单（后端，防止漏做）

| 步骤 | 位置 | 说明 |
|------|------|------|
| 1 | `AppPermissions/PermissionCodes.cs` | 新增权限常量（与前端 `permission-codes.ts` 一致） |
| 2 | `AppPermissions/PermissionDefinitionContext.cs` | 在对应父权限下 `AddChild(权限码, "显示名")` |
| 3 | `AppPermissions/PermissionMapper.cs` | `_permissionDescriptionMap` 中增加 `{ 权限码, "描述" }` |
| 4 | 各端点 `Configure()` | `Permissions(PermissionCodes.AllApiAccess, PermissionCodes.XxxView)` 等，按接口职责绑定 |
| 5 | `Utils/SeedDatabaseExtension.cs` | 若需默认给管理员：在 `adminPermissionCodes` 中加入新权限码（仅新库或重跑种子时生效） |

## 最佳实践

- 优先使用主构造函数、`await` 与 Async 方法。
- 新建或修改某类文件前，先打开对应的 `*.instructions.md` 按其中示例与规则实现。
- **领域事件**：同一聚合/业务下的多个领域事件可放在一个文件里（如 `LeaveDomainEvents.cs` 内含 Created、Submitted、Approved），减少文件数量；路径为 `DomainEvents/{Feature}Events/{Feature}DomainEvents.cs`。

---

## API 端点 Request/Response 格式规范（必须遵守）

新增或修改 `src/Ncp.Admin.Web/Endpoints/**/*.cs` 时，Request/Response 与端点类需按以下格式书写，参考：`Endpoints/Identity/Admin/UserEndpoints/UpdateUserEndpoint.cs`、`Endpoints/Customer/*.cs`。

### 1. Request / Response 使用 record

- **一律使用 record**，不用 class。
- **优先使用位置参数形式**（便于与文档注释一一对应），例如：
  - `public record UpdateXxxRequest(CustomerId Id, UserId? OwnerId, DeptId? DeptId, ...);`
  - `public record CreateXxxResponse(CustomerId Id);`

### 2. 强类型 ID

- 请求/响应中的 ID 必须使用领域强类型，禁止使用裸 `Guid`、`long`、`string`：
  - 客户：`CustomerId`、`CustomerContactId`
  - 用户/部门：`UserId`、`DeptId`
  - 客户来源/行业：`CustomerSourceId`、`IndustryId`
  - 其他聚合：对应聚合的 `XxxId`（如 `ContractId`、`RoleId` 等）
- 列表类字段使用 `IReadOnlyList<IndustryId>?`、`IReadOnlyList<XxxId>?` 等，避免 `IList<string>` 再在 Handler 里解析。

### 3. 文档注释（必须）

- **每个 Request/Response record**：
  - 上方写 `<summary>` 描述用途。
  - 每个参数一行 `<param name="参数名">描述</param>`，与位置参数顺序一致。
- **端点类**：
  - 类上方 `<summary>` 描述端点职责。
  - 主构造函数参数用 `<param name="参数名">描述</param>`（如 `<param name="mediator">MediatR 中介者</param>`）。
- **Override 方法**：`Configure()`、`HandleAsync()` 可只写 `/// <inheritdoc />`。

示例（节选）：

```csharp
/// <summary>
/// 更新客户请求
/// </summary>
/// <param name="Id">客户 ID</param>
/// <param name="OwnerId">负责人用户 ID，null 表示公海</param>
/// <param name="DeptId">所属部门 ID</param>
/// <param name="CustomerSourceId">客户来源 ID</param>
/// <param name="FullName">客户全称</param>
/// …（其余参数略）
public record UpdateCustomerRequest(
    CustomerId Id,
    UserId? OwnerId,
    DeptId? DeptId,
    CustomerSourceId CustomerSourceId,
    string FullName,
    …);

/// <summary>
/// 更新客户
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateCustomerEndpoint(IMediator mediator) : Endpoint<UpdateCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure() { … }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateCustomerRequest req, CancellationToken ct) { … }
}
```

### 4. 继承查询入参的 Request

- 若请求体等价于现有查询入参（如分页列表、搜索），可继承对应 Input 类型并保留 record/class 其一，但**端点类仍需加 summary 与构造函数 param 注释**，例如：
  - `public class GetCustomerListRequest : CustomerQueryInput { }`
  - 端点类：`/// <summary>获取客户分页列表</summary>`、`/// <param name="query">客户查询</param>`

### 5. 检查清单（新增/修改端点时）

- [ ] Request/Response 为 record，且使用强类型 ID（无裸 Guid/long/string）
- [ ] Request/Response 有 `<summary>` 与每个参数的 `<param>`
- [ ] 端点类有 `<summary>`，主构造函数每个依赖有 `<param>`
- [ ] 响应使用 `ResponseData<T>` 包装；发送时使用 `.AsResponseData()`
