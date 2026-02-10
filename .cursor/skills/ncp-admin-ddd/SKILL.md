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
| 领域事件 | `src/Ncp.Admin.Domain/DomainEvents/` |
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

## 最佳实践

- 优先使用主构造函数、`await` 与 Async 方法。
- 新建或修改某类文件前，先打开对应的 `*.instructions.md` 按其中示例与规则实现。
