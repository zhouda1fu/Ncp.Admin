# Ncp.Admin 后端代码规范审查报告

> 审查依据：`.cursor/skills/cleanddd-dotnet-coding`、`.cursor/skills/cleanddd-modeling`、`.cursor/rules/project-conventions.mdc`
>
> 审查范围：`src/Ncp.Admin.Domain`、`src/Ncp.Admin.Infrastructure`、`src/Ncp.Admin.Web`
>
> **同步说明**：部分章节标题或表格中已标注 **✅ 已处理**，表示截至 **2026-04-13** 在仓库中已落实或与当时扫描结果不一致处已更正；**未标注项**仍视为待改进（或需抽样复核）。

---

## 目录

1. [总体评价](#1-总体评价)
2. [残留 / 重复 / 放置错误的文件](#2-残留--重复--放置错误的文件)
   - 2.0.1 IOrderLogisticsCompanyRepository.cs 残留在 Domain 层 ✅ 已处理
   - 2.0.2 OrderLogisticsCompany / OrderLogisticsMethod 聚合根放错目录 ✅ 已处理
   - 2.0.3 CustomerShare 子实体未继承 Entity
3. [Domain 层问题](#3-domain-层问题)
   - 3.1 聚合根缺少 Deleted / RowVersion 属性
   - 3.2 子实体与 `Entity<TId>`、关系建模（勘误：无独立 `IEntity` 接口）
   - 3.3 多个聚合状态变更未发布领域事件
   - 3.4 目录命名不一致（ContractTypeOptions）✅ 已处理
   - 3.5 Contract 聚合使用裸 Guid 而非强类型 ID ✅ 已处理
   - 3.6 Contract 聚合使用 bool IsDeleted 而非框架 Deleted 类型 ✅ 已处理
   - 3.7 DomainEvents 目录结构不统一 ✅ 已处理
   - 3.8 ErrorCodes 分区混乱与部分常量缺少注释
4. [Infrastructure 层问题](#4-infrastructure-层问题)
   - 4.1 大量实体配置缺少 HasComment
   - 4.2 部分实体配置缺少字符串列 MaxLength
   - 4.3 OrderRepository 使用构造参数 context 而非 DbContext 属性 ✅ 已处理
5. [Web 层 — 查询（Queries）问题](#5-web-层--查询queries问题)
   - 5.1 查询未使用 IQuery<T>/IQueryHandler 模式
   - 5.2 查询类命名不符合约定
6. [Web 层 — 命令（Commands）问题](#6-web-层--命令commands问题)
   - 6.1 命令目录命名不统一
7. [Web 层 — 端点（Endpoints）问题](#7-web-层--端点endpoints问题)
   - 7.1 部分 Request/Response 缺少 XML 文档注释
   - 7.2 大量端点缺少 Description 配置
   - 7.3 端点内包含业务逻辑（解析 Claims）✅ 已处理（抽取扩展方法与 JWT claim 常量）
   - 7.4 查询端点直接注入 Query 而非使用 IMediator
8. [Web 层 — 领域事件处理器问题](#8-web-层--领域事件处理器问题)
   - 8.1 命名已规范，方法签名正确 ✅
9. [异常处理问题](#9-异常处理问题)
   - 9.1 SeedDatabaseExtension 使用 InvalidOperationException
   - 9.2 PermissionDefinitionContext 使用 ArgumentException
10. [汇总统计与优先级建议](#10-汇总统计与优先级建议)

---

## 1. 总体评价

项目整体遵循 CleanDDD 规范程度 **较高**，具体表现为：

- ✅ 分层依赖 Web → Infrastructure → Domain 严格单向
- ✅ 强类型 ID 普遍使用 `partial record XxxId : IGuidStronglyTypedId/IInt64StronglyTypedId`
- ✅ 聚合根继承 `Entity<TId>, IAggregateRoot`，protected 无参构造，属性 private set
- ✅ 命令处理器未出现 `SaveChanges` 调用
- ✅ 业务异常统一使用 `KnownException`（聚合与命令层）
- ✅ 领域事件处理器方法签名使用 `Handle()`（非 `HandleAsync()`），符合本仓库特有约定
- ✅ 端点使用 `Send.OkAsync` + `.AsResponseData()` 模式
- ✅ Request/Response 一律 record，ID 参数使用强类型

以下列出发现的 **不符合规范** 之处及改进建议。

---

## 2. 残留 / 重复 / 放置错误的文件

### 2.0.1 `IOrderLogisticsCompanyRepository.cs` 残留在 Domain 层（应删除）✅ **已处理**

**规范要求**：仓储接口与实现均放在 `Infrastructure/Repositories/` 下同一文件中。

**原问题**：`src/Ncp.Admin.Domain/IOrderLogisticsCompanyRepository.cs` 曾是一个 **已废弃的残留文件**：
- 历史上可能曾在 `Ncp.Admin.Domain.csproj` 中用 `<Compile Remove>` 排除编译
- 真正的接口定义在 `src/Ncp.Admin.Infrastructure/Repositories/OrderLogisticsCompanyRepository.cs`（**重复定义**）
- 该文件的命名空间 `namespace Ncp.Admin.Domain;` 也不符合约定

```csharp
// src/Ncp.Admin.Domain/IOrderLogisticsCompanyRepository.cs（应删除）
public interface IOrderLogisticsCompanyRepository : IRepository<OrderLogisticsCompany, OrderLogisticsCompanyId> { }

// src/Ncp.Admin.Infrastructure/Repositories/OrderLogisticsCompanyRepository.cs（正确位置）
public interface IOrderLogisticsCompanyRepository : IRepository<OrderLogisticsCompany, OrderLogisticsCompanyId> { }
```

**当前状态**：Domain 层已无该文件；仓储接口在 `Infrastructure/Repositories/OrderLogisticsCompanyRepository.cs`。**若** `Ncp.Admin.Domain.csproj` 中曾存在 `<Compile Remove="...">`，应已一并移除（以 csproj 为准）。

---

### 2.0.2 `OrderLogisticsCompany` 和 `OrderLogisticsMethod` 聚合根放错目录 ✅ **已处理**

**规范要求**：每个聚合根应有独立的 `{Aggregate}Aggregate` 目录。

**原问题**：`OrderLogisticsCompany` 与 `OrderLogisticsMethod` 曾放在 `OrderAggregate/` 下；`CustomerContactRecord` 曾放在 `CustomerAggregate/` 下。

**当前状态**：
- `OrderLogisticsCompany` → `AggregatesModel/OrderLogisticsCompanyAggregate/`
- `OrderLogisticsMethod` → `AggregatesModel/OrderLogisticsMethodAggregate/`
- `CustomerContactRecord` 聚合根 → `AggregatesModel/CustomerContactRecordAggregate/`（枚举等辅助类型可仍留在 `CustomerAggregate`，以仓库为准）

以下树形结构保留为 **迁移前对照**，不再代表当前目录：

```
（历史）❌ 曾错误地放在 OrderAggregate 下的独立聚合根 → 已迁出
```

---

### 2.0.3 `CustomerShare` 子实体未继承 `Entity<TId>`

**规范要求**：子实体应继承 `Entity<TId>`。

**问题**：`CustomerShare` 是 `Customer` 的子实体，但未继承任何基类，也没有强类型 ID：

```csharp
// CustomerShare.cs
public class CustomerShare  // ❌ 没有继承 Entity<TId>，没有 Id
{
    public CustomerId CustomerId { get; private set; } = default!;
    public UserId SharedToUserId { get; private set; } = default!;
    public UserId SharedByUserId { get; private set; } = default!;
    public DateTimeOffset SharedAt { get; private set; }
}
```

`UserRole` 也同样没有继承 `Entity<TId>`，没有强类型 ID。

**建议**：如果这些是值对象（无 Id 的关联表），可以保留；但如果需要独立标识，应加上强类型 ID 并继承 `Entity<TId>`。需要在规范中明确值对象与子实体的区分。

---

## 3. Domain 层问题

### 3.1 聚合根缺少 Deleted / RowVersion 属性

**规范要求**：聚合根应包含 `Deleted` 与 `RowVersion` 属性，使用框架提供的类型。（注：主数据/字典类聚合如需精简可酌情省略）

**问题**：主数据/字典类聚合可酌情省略；其余聚合应逐步对齐 `Deleted` + `RowVersion`。

**2026-04-13 复核（抽样）**：

| 聚合 | Deleted | RowVersion | 备注 |
|------|---------|------------|------|
| Customer | ✅ `Deleted` | ✅ | 已具备 |
| Contract | ✅ `Deleted`（属性名 `IsDeleted`） | ✅ | 已具备 |
| Announcement | ✅ | ✅ | 已具备 |
| Vehicle | ✅ | ✅ | 已具备 |
| ContractTypeOption | — | — | 主数据聚合，仍无 `Deleted`/`RowVersion`（可酌情） |
| User | ✅ | ✅ | 合规 |
| Dept | ✅ | ✅ | 已补充 RowVersion |
| Role | ✅ | ✅ | 已补充 RowVersion |
| Order | ✅ | ✅ | 已补充 RowVersion |

**仍建议**：对其余未抽样聚合与新增聚合做清单核对；主数据类是否在规范中明确「可省略」需团队共识。补齐时参考：

```csharp
public Deleted Deleted { get; private set; } = new();
public RowVersion RowVersion { get; private set; } = new(0);
```

---

### 3.2 子实体与 `Entity<TId>`、关系建模 ✅ **审查结论更正**

**勘误（2026-04）**：初稿误写「子实体须再实现 `IEntity` 接口」。在本仓库使用的 **NetCorePal.Extensions.Domain** 中，**不存在**供领域实体实现的独立 `IEntity` 标记接口；子实体 **`继承 Entity<TId>`** 即符合基类约定。**请勿**与 EF Core 的 `IEntityTypeConfiguration<T>`（基础设施层配置接口）混淆。

**规范要求（更正后）**：
- 聚合根：`Entity<TAggregateId>, IAggregateRoot`
- 聚合内子实体：一般 **`Entity<TChildId>`** + `protected` 无参构造 + 强类型 ID（与 `cleanddd-dotnet-coding` 技能一致）

**一对多、多对多是否还能继承 `Entity<TId>`？**

| 关系 | 是否常用 `Entity<TId>` | 说明 |
|------|------------------------|------|
| **一对多**（父聚合 ↔ 子集合，子属于同一聚合边界） | ✅ 常见 | 子实体有独立标识（如 `OrderItemId`），由父聚合维护集合与不变式；典型如订单明细、合同发票行。 |
| **多对多**（同一聚合内需显式关联表） | ✅ 可以 | 联结行若有 **独立主键/强类型 Id**，可建模为 `Entity<TLinkId>`（如需要由聚合根控制生命周期）。 |
| **多对多**（仅关联两 Id、无独立业务身份） | ⚠️ 视设计 | 可用 **联结实体类**（未必继承 `Entity`，或复合键），或拆成值对象；以不变式与持久化映射为准。 |
| **无标识的关联/快照** | ❌ 通常不 | 更常建模为 **值对象** 或简单 POCO，由 EF 配置映射；与「是否有 `IEntity`」无关，本仓库亦无该接口。 |

**仍待团队自行梳理的项**：如 `CustomerShare`、`RolePermission` 等未继承 `Entity<TId>` 的类型，应明确为 **值对象 / 联结 POCO** 还是将来升级为带 Id 的实体（见 2.0.3）。

---

### 3.3 多个聚合状态变更未发布领域事件

**规范要求**：状态变更时应使用 `this.AddDomainEvent()` 发布领域事件。

**原问题（2026-04-10 扫描）**：部分聚合在关键行为中未发布领域事件。

**2026-04-13 复核**：以下聚合已存在领域事件文件或聚合内 `AddDomainEvent`（非穷尽列举场景）：

| 聚合 | 状态 | 说明 |
|------|------|------|
| Contract | ✅ | 如创建/更新/提交审批/通过/归档/删除等（见 `ContractDomainEvents` / 聚合方法） |
| Announcement | ✅ | 创建、草稿更新、发布等 |
| Vehicle | ✅ | 创建、更新、状态变更等 |
| VehicleBooking | ✅ | 创建、取消、完成等 |
| ContractTypeOption | ✅ | 创建等（主数据） |
| Project | ✅ | 创建、更新、归档等 |
| Project 跟进记录 | ✅ | 增/改/删发布 `ProjectFollowUpRecordAdded/Updated/RemovedDomainEvent`（见 `ProjectDomainEvents.cs`） |
| Project 联系人 | ✅ | 增/改/删发布 `ProjectContactAdded/Updated/RemovedDomainEvent` |

**仍待加强**：跨聚合通知、统计等场景是否订阅上述细分事件，可按业务再定；其它子实体路径同理。

**仍有领域事件的聚合（示例）**：`Customer`、`Order`、`User`、`Role`、`Dept`、`Position`、`WorkflowInstance`、`WorkflowDefinition`、`LeaveRequest` 等。

**建议**：对仍无事件或仅部分路径有事件的聚合做差异清单；通知类需求优先补齐。

---

### 3.4 目录命名不一致（ContractTypeOptions）✅ **已处理**

**规范要求**：聚合目录命名应为 `{Aggregate}Aggregate`。

**当前状态**：已使用 `AggregatesModel/ContractTypeOptionAggregate/`，命名空间为 `Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate`。

**原问题（对照）**：曾使用 `ContractTypeOptions` 目录与命名空间，不符合约定。

---

### 3.5 Contract 聚合使用裸 Guid 而非强类型 ID ✅ **已处理**

**规范要求**：ID 应使用强类型（禁止裸 `Guid`/`long`/`string`）。

**当前状态**：`Contract` 使用 `DeptId DepartmentId`（及命令/端点侧同步，以代码为准）。

**原问题（对照）**：曾使用 `Guid DepartmentId`。

---

### 3.6 Contract 聚合使用 bool IsDeleted 而非框架 Deleted 类型 ✅ **已处理**

**规范要求**：软删标记应使用框架提供的 `Deleted` 类型。

**当前状态**：`public Deleted IsDeleted { get; private set; } = new Deleted(false);`，删除等行为与领域事件以 `Contract` 聚合为准。

**原问题（对照）**：曾使用 `bool IsDeleted`。

---

### 3.7 DomainEvents 目录结构不统一 ✅ **已处理**

**规范要求**：领域事件文件命名为 `{Aggregate}DomainEvents.cs`，放在 `src/Ncp.Admin.Domain/DomainEvents` 目录下。

**当前状态**：事件文件已集中在 `DomainEvents/` **根目录**，文件名多为 `{Aggregate}DomainEvents.cs`（如 `RoleDomainEvents.cs`、`DeptDomainEvents.cs`、`PositionDomainEvents.cs`、`OrderDomainEvents.cs` 等）。若仍有新增事件，保持同约定即可。

**原问题（对照）**：曾存在子目录混杂、`RoleEvents.cs`、按单文件命名等历史结构。

---

### 3.8 ErrorCodes 分区混乱与部分常量缺少注释

**规范要求**：`ErrorCodes.cs` 中每个常量应有 `<summary>` XML 注释。编号应保持有序一致。

**问题 A — 部分常量缺少 `<summary>`**：

```csharp
// ErrorCodes.cs 第 643-647 行
public const int OrderInvoiceTypeOptionNotFound = 290014;   // ❌ 无 summary
public const int OrderLogisticsCompanyIdInvalid = 290015;   // ❌ 无 summary
public const int OrderLogisticsMethodIdInvalid = 290016;    // ❌ 无 summary
```

**问题 B — 客户区域 (280xxx) 混入大量非客户错误码**：

`#region 客户相关错误 (280xxx)` 下除客户本身的错误码外，还混入了以下与客户无直接关系的错误码：

| 常量 | 值 | 实际所属模块 |
|------|-----|-------------|
| `ProjectTypeNotFound` | 280008 | 项目类型 |
| `ProjectStatusOptionNotFound` | 280009 | 项目状态 |
| `ProjectIndustryNotFound` | 280010 | 项目行业 |
| `ContractTypeOptionNotFound` | 280011 | 合同类型 |
| `IncomeExpenseTypeOptionNotFound` | 280012 | 收支类型 |
| `ContractInvoiceNotFound` | 280013 | 合同发票 |
| `IndustryNotFound` | 280016 | 行业主数据 |

**问题 C — 编号规则被打破**：

`CustomerSeaRegionAssign*` 系列错误码使用 7 位数 `2801001`-`2801003`，打破了其他区域统一的 6 位数编号规则 `(280xxx)`。

**建议**：
1. 给所有常量补充 `<summary>` XML 注释
2. 将非客户模块错误码拆分到对应的 `#region` 中，或统一管理方案
3. 重新分配 `CustomerSeaRegionAssign*` 编号，保持 6 位数一致（如 `280101`-`280103`）

---

## 4. Infrastructure 层问题

### 4.1 大量实体配置缺少 HasComment

**规范要求**：字段应添加 `HasComment` 注释。

**问题**：经统计，共约 **600+** 处 `.Property()` 配置，但仅约 **250** 处添加了 `HasComment`。以下是典型的未添加 HasComment 的配置文件：

| 文件 | Property 数 | HasComment 数 | 覆盖率 |
|------|------------|--------------|--------|
| VehicleBookingEntityTypeConfiguration.cs | 9 | 0 | 0% |
| AttendanceRecordEntityTypeConfiguration.cs | 7 | 0 | 0% |
| ChatGroupEntityTypeConfiguration.cs | 8 | 0 | 0% |
| ChatMessageEntityTypeConfiguration.cs | 6 | 0 | 0% |
| ContactGroupEntityTypeConfiguration.cs | 6 | 0 | 0% |
| ScheduleEntityTypeConfiguration.cs | 8 | 0 | 0% |
| MeetingRoomEntityTypeConfiguration.cs | 8 | 0 | 0% |
| MeetingBookingEntityTypeConfiguration.cs | 9 | 0 | 0% |
| LeaveBalanceEntityTypeConfiguration.cs | 8 | 0 | 0% |
| ShareLinkEntityTypeConfiguration.cs | 6 | 0 | 0% |
| AnnouncementEntityTypeConfiguration.cs | 9 | 0 | 0% |
| AnnouncementReadRecordEntityTypeConfiguration.cs | 4 | 0 | 0% |
| ExpenseClaimEntityTypeConfiguration.cs | 13 | 1 | 8% |
| LeaveRequestEntityTypeConfiguration.cs | 12 | 1 | 8% |
| CustomerSeaVisibilityBoardEntityTypeConfiguration.cs | 1 | 1 | — |
| DeptEntityTypeConfiguration.cs | 9 | 1 | 11% |

**建议**：为所有 `.Property()` 配置补充 `.HasComment("说明")`，保持数据库字段可读性。

---

### 4.2 部分实体配置缺少字符串列 MaxLength

**规范要求**：字符串列应设置 `MaxLength`。

**问题**：部分配置文件中字符串属性未设置 `HasMaxLength()`，例如：

- `CustomerEntityTypeConfiguration.cs` 中 `ShortName` 有 MaxLength，但部分区域码字段只设了 `IsRequired(false)` 而未配 HasComment
- 部分新增配置可能遗漏 MaxLength

**建议**：全面检查所有字符串属性是否配置了 `HasMaxLength()`。

---

### 4.3 OrderRepository 使用构造参数 context 而非 DbContext 属性 ✅ **已处理**

**规范要求**：仓储实现应优先使用基类的 `DbContext` 属性访问 DbSet。

**当前状态**：`GetAggregateForEditAsync` 使用 `DbContext.Orders` 及 `Include`，与基类约定一致。

**原问题（对照）**：曾使用构造注入的 `context` 字段访问 `Orders`。

---

## 5. Web 层 — 查询（Queries）问题

### 5.1 查询未使用 IQuery<T>/IQueryHandler 模式

**规范要求**：查询应定义为 `record : IQuery<T>`，处理器实现 `IQueryHandler<TQuery, TResult>`，通过 MediatR 调度。

**问题**：项目中 **所有查询** 均采用自定义 `IQuery` 标记接口 + 直接注入 Query 类的模式，而非 MediatR 的 `IQuery<T>/IQueryHandler` 模式。

```csharp
// 当前模式（全局一致）
public class ContractQuery(ApplicationDbContext dbContext) : IQuery { ... }

// 端点中直接注入
public class GetContractListEndpoint(ContractQuery query) : Endpoint<...> { ... }
```

```csharp
// 规范推荐模式
public record GetContractListQuery(...) : IQuery<PagedData<ContractQueryDto>>;
public class GetContractListQueryHandler(ApplicationDbContext ctx) : IQueryHandler<GetContractListQuery, ...> { ... }

// 端点中通过 mediator 调度
public class GetContractListEndpoint(IMediator mediator) : Endpoint<...>
{
    public override async Task HandleAsync(...) =>
        await mediator.Send(new GetContractListQuery(...), ct);
}
```

**影响**：共计 **50+** 个 Query 类涉及，属于全局性架构偏差。

**建议**：此项属于系统性架构选择，若团队已统一此模式并运行良好，可保留现状并在规范文档中注明偏差。若需严格对齐规范，需逐步重构。

---

### 5.2 查询类命名不符合约定

**规范要求**：查询文件命名为 `{Action}{Entity}Query.cs`，如 `GetContractListQuery.cs`。

**问题**：当前查询类命名为 `{Entity}Query.cs`（如 `ContractQuery.cs`、`OrderQuery.cs`），每个类内包含多个查询方法。这与规范中"每个查询一个 record + handler"的模式不同。

**建议**：同 5.1，如保留现有模式则在规范中注明。

---

## 6. Web 层 — 命令（Commands）问题

### 6.1 命令目录命名不统一

**规范要求**：命令目录应为 `Commands/{Module}s`（复数）。

**问题**：命令目录命名风格不统一：

```
Commands/
├── Announcements/          ← 仅类名不带 Module ❌（应为 AnnouncementModule 或 Announcements）
├── Announcement/           ← 有的文件夹不加 s
├── ContractModule/         ← 带 Module 后缀
├── VehicleModule/          ← 带 Module 后缀
├── Order/                  ← 无后缀
├── Product/                ← 无后缀
├── Identity/Admin/UserCommands/  ← 层级嵌套
├── Customers/              ← 复数
├── Workflow/               ← 无后缀
```

**建议**：统一目录命名风格，推荐使用复数形式（如 `Contracts`、`Orders`、`Vehicles`），或统一使用 `{Module}Module` 格式。

---

## 7. Web 层 — 端点（Endpoints）问题

### 7.1 部分 Request/Response 缺少 XML 文档注释

**规范要求**：每个 Request/Response 须有 `<summary>` 与各参数 `<param>` 注释。

**问题**：部分端点的 Request/Response record 缺少完整的 XML 文档注释。

**缺失示例**：

```csharp
// CreateAnnouncementEndpoint.cs - Request 和 Response 均无 summary/param
public record CreateAnnouncementRequest(string Title, string Content);
public record CreateAnnouncementResponse(AnnouncementId Id);

// UpdateAnnouncementEndpoint.cs
public record UpdateAnnouncementRequest(AnnouncementId Id, string Title, string Content);

// MarkAnnouncementReadEndpoint.cs
public record MarkAnnouncementReadRequest(AnnouncementId Id);

// PublishAnnouncementEndpoint.cs
public record PublishAnnouncementRequest(AnnouncementId Id);

// UpdateOrderRequest - 完全没有注释
public record UpdateOrderRequest(string CustomerName, ProjectId ProjectId, ...);

// CreateOrderRequest - 完全没有注释
public record CreateOrderRequest(...);
```

**合规示例**（对比）：

```csharp
// CreateSeaCustomerEndpoint.cs ✅ 有完整注释
/// <summary>
/// 公海创建客户请求
/// </summary>
/// <param name="CustomerSourceId">客户来源 ID</param>
/// ...
public record CreateSeaCustomerRequest(...);
```

**建议**：为所有 Request/Response 补充 `<summary>` 和 `<param>` 注释。

---

### 7.2 大量端点缺少 Description 配置

**规范要求**：端点应在 `Configure()` 中配置 `Description`。

**问题**：经统计，约 **270** 个端点中仅约 **68** 个配置了 `Description()`（约 25%）。大部分端点只配置了 `Tags`、`AuthSchemes`、`Permissions`。

**示例（缺少 Description）**：
```csharp
public override void Configure()
{
    Tags("Vehicle");
    Put("/api/admin/vehicles/{id}");
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    Permissions(PermissionCodes.AllApiAccess, PermissionCodes.VehicleEdit);
    // ❌ 缺少 Description
}
```

**建议**：为所有端点补充 `Description(d => d.Summary("xxx"))` 以增强 Swagger 文档可读性。

---

### 7.3 端点内包含业务逻辑（解析 Claims）✅ **已处理**

**规范要求**：API 端点以调度为主，避免重复、易错的 Claims 手写解析。

**当前状态（2026-04）**：
1. **`ClaimsPrincipalUserExtensions`**（`TryGetUserId` / `GetUserIdOrNull` / `GetUserDisplayName` / `GetUserDisplayNameOrGivenName`）统一 `NameIdentifier` 与显示名解析。
2. **`JwtDataPermissionClaimTypes`**、**`JwtPermissionClaimTypes`** 与登录签发一致，避免魔法字符串漂移。
3. **操作日志 PostProcessor**、**`DataPermissionContextExtensions`**、**SignalR `NameUserIdProvider`** 等已改用上述扩展或常量。

**可选后续**：若团队希望「用户身份仅在应用层出现」，可再评估将 `UserId` 注入命令处理器（`IHttpContextAccessor` / 自定义 `ICurrentUser`），与扩展方法方案二选一或并存。

**原问题（对照）**：曾大量直接使用 `FindFirstValue` + `TryParse`。

---

### 7.4 部分查询端点直接注入 Query 而非使用 IMediator

**规范要求**：端点应通过 mediator 发送命令/查询。

**问题**：与 5.1 一致，查询端点直接注入 Query 类而非通过 mediator。这是系统性的架构选择。

```csharp
// 当前
public class GetContractListEndpoint(ContractQuery query) : Endpoint<...>

// 规范推荐
public class GetContractListEndpoint(IMediator mediator) : Endpoint<...>
```

---

## 8. Web 层 — 领域事件处理器问题

### 8.1 命名已规范，方法签名正确 ✅

领域事件处理器命名遵循 `{DomainEvent}DomainEventHandlerFor{Action}` 约定，方法签名使用 `Handle()`（非 `HandleAsync()`），符合本仓库特有约定。

所有 18 个处理器均通过 mediator 发送命令驱动聚合，未直接改 Db。✅ 完全合规。

---

## 9. 异常处理问题

### 9.1 SeedDatabaseExtension 使用 InvalidOperationException

**规范要求**：业务异常应使用 `KnownException`。

**问题**：`SeedDatabaseExtension.cs` 中大量使用 `throw new InvalidOperationException(...)`（约 **20+** 处）。

```csharp
// SeedDatabaseExtension.cs:338
?? throw new InvalidOperationException("无法找到部门'公司'");
```

**评估**：种子数据初始化属于基础设施层的初始化逻辑，此处使用 `InvalidOperationException` 有其合理性（不是面向用户的业务异常）。但如果需要严格统一，可考虑使用 `KnownException`。

**建议**：可保留现状（低优先级），初始化逻辑中的异常不面向终端用户。

---

### 9.2 PermissionDefinitionContext 使用 ArgumentException

**问题**：权限定义上下文中使用了 `ArgumentException` 和 `InvalidOperationException`：

```csharp
// PermissionDefinitionContext.cs:262
throw new ArgumentException($"There is already an existing permission group with name: {name}");

// PermissionDefinitionContext.cs:299
throw new InvalidOperationException($"Duplicate permission code detected: {permission.Code}");
```

**评估**：这属于框架级防御编程，非业务异常，保留合理。

---

## 10. 汇总统计与优先级建议

### 按严重程度分级

| 优先级 | 问题 | 涉及范围 | 状态 / 建议 |
|--------|------|---------|------------|
| ~~🔴 高~~ | ~~Domain 层残留 / 重复文件 (2.0.1)~~ | ~~1 个文件 + csproj~~ | ✅ **已处理** |
| ~~🔴 高~~ | ~~聚合根放错目录 (2.0.2)~~ | ~~物流聚合 + CustomerContactRecord~~ | ✅ **已处理** |
| 🟡 中 | 聚合根缺少 Deleted/RowVersion（部分聚合） | 未抽样全覆盖 | 对剩余聚合做清单核对；主数据是否省略需共识 |
| ~~🔴 高~~ | ~~Contract 裸 Guid / bool 软删~~ | ~~Contract 聚合~~ | ✅ **已处理**（`DeptId` + `Deleted`） |
| 🟡 中 | ErrorCodes 分区混乱 + 缺注释 (3.8) | ErrorCodes.cs | 重新分区并补注释 |
| 🟡 中 | 领域事件覆盖差异（其它子路径） | 各聚合 | 按需补全；`Project` 跟进记录已细分事件（见 §3.3） |
| 🟡 中 | 实体配置缺少 HasComment | ~350 处 | 批量补充 |
| 🟡 中 | Request/Response 缺少 XML 注释 | ~50+ 个端点 | 批量补充 |
| 🟡 中 | 端点缺少 Description 配置 | ~200 个端点 | 批量补充 |
| ~~🟡 中~~ | ~~子实体缺少 IEntity 标记~~ | ~~—~~ | ✅ **审查更正**：本仓库无领域层 `IEntity` 接口；子实体以 `Entity<TId>` 为准（见 §3.2） |
| ~~🟡 中~~ | ~~DomainEvents / ContractTypeOption 目录~~ | ~~Domain / 命名~~ | ✅ **已处理** |
| 🟡 中 | Commands 目录命名不统一 (6.1) | Web/Commands | 仍待统一 |
| 🟡 中 | CustomerShare / UserRole 等未继承 Entity (2.0.3) | ~2+ 个文件 | 视为值对象或补充 |
| 🟢 低 | 查询未使用 IQuery<T>/IQueryHandler 模式 | ~50 个查询 | 系统性架构，可保留并文档化 |
| ~~🟢 低~~ | ~~端点内 Claims 解析重复~~ | ~~Web~~ | ✅ **已处理**（扩展 + JWT 常量） |
| ~~🟢 低~~ | ~~OrderRepository 使用构造参数 context~~ | ~~1 个文件~~ | ✅ **已处理** |
| ⚪ 信息 | SeedData 使用 InvalidOperationException | 初始化代码 | 可保留 |

### 推荐修复顺序

1. **第一批**（~~已完成多项~~；余下为核对类）
   - ~~删除 Domain 残留仓储接口文件、Contract 强类型与软删、物流/客户联系记录聚合目录~~ → ✅ **已落实**
   - 对其余聚合 **Deleted / RowVersion** 做 **全量清单**（含主数据是否豁免）

2. **第二批**（结构调整与代码规范）
   - 统一 **Commands** 目录命名
   - **ErrorCodes.cs** 分区整理 + 补充缺失的 `<summary>` + 修正编号

3. **第三批**（文档与可读性）
   - 补充实体配置的 `HasComment`
   - 补充端点的 `Description`
   - 补充 Request/Response 的 XML 注释

4. **第四批**（架构优化，可选）
   - 评估是否迁移到 `IQuery<T>/IQueryHandler` 模式
   - ~~抽取端点中的 Claims 解析逻辑~~ → ✅ **已通过扩展方法与 JWT claim 常量落实**
   - 对仍缺领域事件的子实体/路径补充事件（~~`ProjectFollowUpRecord`~~ 已补充细分事件）
   - 明确 `CustomerShare`、`UserRole` 等无 ID 类型是值对象还是子实体

---

*报告生成时间：2026-04-10（初稿扫描） · **同步标记更新：2026-04-13**（与仓库当前实现对齐；抽样复核，非全量静态证明）*
