# 客户公海片区分配模块实践：从表结构到业务落地

> 本文介绍在 **Ncp.Admin**（Clean Architecture + DDD）中，如何实现「客户管理 → 客户公海片区分配」能力：营销中心人员与行政地区（全国-省-市-区）的多对多绑定、树形多选保存、自动展开子级、差量更新与审计留痕，以及 **以被分配人角色数据权限为准** 的片区并集与抽屉回显规则。  
> 文档结构参照 `blog-workflow-system.md` 的实践写法，便于团队内统一阅读。

## 一、项目背景与技术栈

**Ncp.Admin** 采用 DDD 分层：**Web（FastEndpoints + CQRS）→ Infrastructure（EF Core）→ Domain（聚合根）**。本模块与现有客户、地区、用户、角色、部门体系集成。

| 层级 | 技术选型 |
|------|---------|
| 前端 | Vue 3 + Vite + Ant Design Vue（Vben Admin） |
| API 层 | ASP.NET Core + FastEndpoints |
| 应用层 | MediatR、FluentValidation |
| 领域层 | DDD 聚合根、强类型 ID |
| 基础设施 | EF Core + PostgreSQL（迁移以仓库内实际为准） |

**业务目标**：在固定 **营销中心及其下级部门** 范围内，为人员维护「公海片区」（行政地区）绑定；支持多选、保存时自动包含所选节点的下级地区；每次变更记录 **新增/删除** 的地区名称快照，便于审计。

---

## 二、需求要点与边界

### 2.1 功能范围

- **人员列表**：默认展示营销中心（及下级）在职人员；列：部门、角色、姓名、操作（分配片区）。
- **分配抽屉**：地区树（全国→省→市→区）多选；保存后写入**当前被点击行**对应用户与地区的绑定。
- **片区变动提示**：对比「被分配人数据权限范围内」多人的片区并集与目标本人当前片区，提示可能发生的变动（仅展示/提示，保存仍只作用于当前目标用户）。
- **审计**：每次保存一条审计头 + 多条明细（新增/删除 + 地区名称快照）。

### 2.2 与「工作流」类模块的差异

本模块**无长流程状态机**，核心是 **绑定关系持久化 + 审计追加 + 读侧汇总**；领域层重点在 **差量更新绑定** 与 **审计不可变追加**，而非节点流转。

---

## 三、数据库表结构设计

迁移类：`AddCustomerSeaRegionAssignment`（表名 snake_case，与项目约定一致）。

### 3.1 `customer_sea_region_assignment`（绑定主表）

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | uuid | 聚合根主键（`CustomerSeaRegionAssignmentId`） |
| `TargetUserId` | bigint | 被分配用户（**唯一**） |
| `CreatedAt` / `ModifiedAt` | timestamptz | 创建/更新时间（UTC） |

**约束**：`TargetUserId` **唯一索引** —— 每个用户至多一条绑定聚合（一对多地区通过子表表达）。

### 3.2 `customer_sea_region_assignment_region`（绑定明细）

| 字段 | 类型 | 说明 |
|------|------|------|
| `AssignmentId` | uuid | 外键 → 绑定主表 |
| `RegionId` | bigint | 地区 ID（`RegionId`） |

**主键**：`(AssignmentId, RegionId)`。  
**语义**：保存的是**展开后的叶子集合**（含用户勾选后算法展开的所有子地区），便于查询与 diff。

### 3.3 `customer_sea_region_assignment_audit`（审计头）

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | uuid | 审计聚合根 ID |
| `TargetUserId` | bigint | 被修改用户 |
| `OperatorUserId` | bigint | 操作人 |
| `OperatorUserName` | varchar(100) | 操作人姓名（冗余展示） |
| `CreatedAt` | timestamptz | 操作时间 |

### 3.4 `customer_sea_region_assignment_audit_detail`（审计明细）

| 字段 | 类型 | 说明 |
|------|------|------|
| `AuditId` | uuid | 外键 → 审计头 |
| `RegionId` | bigint | 地区 |
| `ChangeType` | int | 0=新增，1=删除 |
| `RegionNameSnapshot` | varchar(200) | 当时地区名称快照 |

**主键**：`(AuditId, ChangeType, RegionId)` —— 同一审计下可区分新增/删除维度，避免 ORM 仅 `Clear`+`Add` 导致复合主键冲突。

---

## 四、领域模型设计

### 4.1 聚合划分

```
CustomerSeaRegionAssignment（绑定聚合）
├── CustomerSeaRegionAssignmentId
├── TargetUserId
├── Regions: ICollection<CustomerSeaRegionAssignmentRegion>
└── UpdateRegionIds(expandedRegionIds)  // 差量增删

CustomerSeaRegionAssignmentAudit（审计聚合，仅追加）
├── CustomerSeaRegionAssignmentAuditId
├── TargetUserId / OperatorUserId / OperatorUserName / CreatedAt
├── Details: ICollection<CustomerSeaRegionAssignmentAuditDetail>
└── Create(...)  // 一次写入头+明细
```

### 4.2 强类型 ID

与项目一致：`CustomerSeaRegionAssignmentId`、`CustomerSeaRegionAssignmentAuditId` 为 GUID 强类型；`UserId`、`RegionId`、`DeptId` 等沿用现有类型。

### 4.3 绑定聚合：差量更新而非 Clear + 全量 Add

`CustomerSeaRegionAssignment.UpdateRegionIds` 对集合做 **差集删除 + 新增缺失项**，避免：

- 复合主键在 EF 变更跟踪下出现重复插入/冲突；
- 不必要的状态抖动。

### 4.4 审计聚合：只追加

每次保存若存在新增/删除地区，则 **新建** 一条 `CustomerSeaRegionAssignmentAudit` 及明细；明细中 **RegionNameSnapshot** 固化当时名称，避免地区主数据改名后历史不可读。

---

## 五、CQRS：命令与查询

### 5.1 保存命令 `AssignCustomerSeaRegionsCommand`

- **入参**：`TargetUserId`、`SelectedRegionIds`（前端选中的节点，可为多选）、`OperatorUserId`（从 JWT 取）。
- **Handler 职责（编排）**：
  1. 校验目标用户属于 **营销中心及其下级部门**（业务边界）。
  2. 加载全地区 `Id/ParentId` 列表，在内存中 **展开所选节点及其所有子地区**（BFS/递归）。
  3. 与当前绑定做 **集合差分**，得到 `added` / `removed`。
  4. 调用聚合 `UpdateRegionIds` 写绑定；若有变更则 **追加** 审计聚合。
- **约定**：Handler **不调用 `SaveChanges`**（由框架/工作单元统一提交）；命令带 **FluentValidation** 校验器。

### 5.2 查询 `CustomerSeaRegionAssignmentQuery`

典型查询包括：

| 能力 | 说明 |
|------|------|
| 营销中心人员分页 | 列表数据源，可按关键字过滤 |
| 目标用户已绑定地区 | 抽屉回显（读侧） |
| 目标用户审计分页 | 抽屉历史记录 |
| 授权片区汇总 | 计算「被分配人」角色数据权限下的可见人员、片区并集、与目标本人差异等，供前端提示 |

### 5.3 地区展开算法（应用层）

从全量 `Region` 父子关系构建邻接表，对选中 `RegionId` 集合做 **后代展开**，得到最终持久化集合。  
**产品规则**：用户勾选父节点时，保存结果包含其**全部子级地区**，避免只选父级却漏掉下级业务数据。

---

## 六、数据权限与展示规则（本模块核心）

### 6.1 为何不完全依赖 JWT `data_scope`

登录 JWT 的 `data_scope` 取多角色中的 **最宽松** 范围；若同时存在 `All` 与 `CustomDeptAndSub`，会出现 **All + 空 authorized_dept_ids** 等组合，导致读侧误把「营销中心全员」并入片区并集。

### 6.2 以「被分配人」为锚的可见用户集合

读侧通过 **被分配人（目标用户）的 UserRoles → Role.DataScope / CustomDeptIds**，在库中按与登录逻辑一致的方式展开部门（`DeptQuery.GetAllChildDeptIdsAsync`），合并 **非 All** 角色范围，得到营销中心内 **可见用户 ID 列表**；再对这些用户的绑定做 **片区并集**。

- **若目标用户全部角色均为 `All`**：在营销中心内等价于「全员可见」并集（与其角色一致）。
- **若存在「仅本人」角色**：在汇总逻辑中可将并集 **收缩为仅目标本人**，避免同部门其他业务经理片区误入提示。

### 6.3 与「菜单权限」的关系

- **菜单/接口权限**（如 `CustomerSeaRegionAssignView/Edit`）：控制谁能进入页面、调用保存接口。
- **片区数据展示**（回显、并集、提示）：按 **被分配人** 角色数据权限计算，与当前操作者是否「更宽」或「更窄」解耦。

### 6.4 临时调试日志（可选）

开发/联调阶段可将关键上下文写入系统临时目录（如 `%TEMP%\sea-region-assign-permission-debug.log`），便于核对 `assigneeUserId`、合并模式、结果数量等。

---

## 七、API 与权限

- **REST 风格**：用户列表、目标片区、保存、审计、授权汇总等独立 Endpoint。
- **权限码**：`CustomerSeaRegionAssignView`、`CustomerSeaRegionAssignEdit`（及 `PermissionDefinitionContext` / `PermissionMapper` / 种子数据中的同步）。
- **错误码**：`ErrorCodes` 中增加营销中心未找到、目标不在营销中心等业务错误，统一使用 `KnownException`。

---

## 八、前端实现要点

### 8.1 路由与菜单

- 独立路由：`/customer/sea/region-assign`（示例路径以项目为准）。
- 挂在 **客户管理** 下；`client-contact-record` 等菜单结构调整与 i18n 同步更新。

### 8.2 列表页

- `Page` + `VbenVxeGrid`；列宽与「操作列」居中；可选 `_flex` 列吸收多余横向空间，避免表格内容挤在左侧。

### 8.3 分配抽屉

- `TreeSelect` 或 `Tree` 多选，勾选策略与后端 **展开子级** 一致展示预期。
- 打开抽屉时并行请求：地区树、目标片区、审计、授权汇总；根据汇总结果设置 **默认选中** 与 **提示文案**。
- API 路径统一走项目 `requestClient` 前缀，避免重复 `/api/admin`。

---

## 九、操作指南（使用侧）

1. **分配权限**：在角色管理中为相应岗位勾选「客户公海片区分配」查看/编辑权限。
2. **进入列表**：客户管理 → 客户公海片区分配；列表为营销中心（及下级）人员。
3. **分配片区**：点击某一行的「分配片区」，在树中选择地区（可多选），保存。
4. **查看变更**：同抽屉内可查看审计历史（新增/删除的地区名称）。
5. **理解提示**：若存在「授权范围内片区并集与当前人员不一致」类提示，其口径来自 **被分配人** 的数据权限范围，而非操作者本人。

---

## 十、架构亮点总结

| 维度 | 实践 |
|------|------|
| 聚合边界 | 绑定与审计分聚合；绑定差量更新、审计只追加 |
| 强类型 ID | 聚合根主键与业务 ID 类型明确，减少混用 |
| CQRS | 命令负责写与编排、查询负责读与汇总 |
| 数据权限 | 读侧以被分配人角色为准，规避 JWT 多角色合并歧义 |
| 可观测 | 审计明细带地区名称快照，便于历史追溯 |

---

## 十一、后续可演进方向

1. **性能**：授权汇总对高频打开抽屉做短时缓存或合并查询（需权衡一致性）。
2. **规则配置化**：营销中心部门名称、是否强制展开子级等改为配置项。
3. **操作者可见性**：若产品需要「操作者 ∩ 被分配人」双重校验，可在现有 Query 上叠加一层策略。
4. **批量分配**：同一命令模型扩展为批量目标（需谨慎事务与审计体量）。

---

## 十二、结语

客户公海片区分配是典型的 **「多对多绑定 + 审计」** 子域：表结构清晰、聚合职责单一、读写规则分离后，即可在现有 DDD 基础设施上快速落地。  
本模块的额外价值在于：**把「片区并集与回显」的口径与数据权限对齐到「被分配人」**，避免登录用户身份与业务对象身份混淆，减少线上「看错片区」类问题。

**与是否引入重量级组件无关，把领域边界与权限语义说清楚，比堆功能更重要。**

---

*文档版本：与仓库实现一致；若表名或权限码变更，请以代码与迁移为准。*
