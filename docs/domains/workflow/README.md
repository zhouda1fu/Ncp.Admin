# 工作流管理领域文档

> 第一入口。本文记录当前工作流管理的领域结构、代码入口、核心流转和扩展方式。

## 模块定位

工作流模块是 OA 审批引擎，负责流程定义版本管理、流程实例运行、审批/抄送任务生成、任务处理、业务回写和流程监控。

运行期只识别发布版本中的 `WorkflowGraph` / `GraphSnapshotJson`。前端设计器 schema 只用于编辑和发布编译，不再作为运行期解释模型。

## 文档结构

| 文档 | 用途 |
| --- | --- |
| `README.md` | 模块结构、代码入口和核心概念 |
| `flows.md` | 发布、发起、审批、退回、驳回、转办、委托、挂起恢复、业务回写流程 |
| `api.md` | Endpoint、DTO、权限码、缓存和条件字段 |
| `dependencies.md` | 工作流与用户、角色、部门、业务模块的依赖关系 |
| `refactor-design.md` | 当前重构后的设计基线 |
| `refactor-implementation-plan.md` | 分阶段实施状态 |

## 核心领域模型

### WorkflowDefinition

文件：`src/Ncp.Admin.Domain/AggregatesModel/WorkflowDefinitionAggregate/WorkflowDefinition.cs`

`WorkflowDefinition` 是流程模板聚合根，管理草稿、发布、归档、新版本和软删除。发布时通过 `WorkflowDefinitionVersion.GraphSnapshotJson` 固化运行图。

关键约束：

- 已发布定义不可直接修改。
- 定义版本由定义聚合创建、更新和发布。
- 新版本发布后归档源定义由领域事件驱动。
- 运行期必须读取已发布版本的 `GraphSnapshotJson`。

### WorkflowInstance

文件：`src/Ncp.Admin.Domain/AggregatesModel/WorkflowInstanceAggregate/WorkflowInstance.cs`

`WorkflowInstance` 表示一次具体审批执行，保存业务类型、业务主键、发起人、发起部门、当前节点、变量 JSON、状态和任务集合。

状态包括：`Running`、`Suspended`、`Completed`、`Rejected`、`Cancelled`、`Faulted`。

关键行为：

- `CreateTask` / `CreateTaskForRole`：创建用户或角色任务。
- `ApproveTask` / `RejectTask` / `ReturnTask`：审批通过、驳回或退回目标节点。
- `TransferTask` / `DelegateTask`：转办或委托。
- `Complete` / `Cancel`：完成或撤销流程。
- `Suspend` / `Resume`：挂起和恢复，并记录 `SuspendedAt` / `ResumedAt`。
- `MarkFaulted`：业务回写失败时标记异常。

### WorkflowTask

文件：`src/Ncp.Admin.Domain/AggregatesModel/WorkflowInstanceAggregate/WorkflowTask.cs`

`WorkflowTask` 是实例聚合内的任务子实体，承载节点 key、节点名、任务类型、处理人、状态、意见、完成时间和实际处理人。

退回场景会使用 `ExtraDataJson` 保存退回上下文，包括退回来源节点、退回目标节点、退回说明和需要修改的业务字段。旧任务或普通审批任务没有退回上下文时按空 JSON 处理。

### WorkflowTaskAssignmentSnapshot

文件：`src/Ncp.Admin.Domain/AggregatesModel/WorkflowInstanceAggregate/WorkflowTaskAssignmentSnapshot.cs`

任务创建时会固化授权快照，记录处理人、授权来源、来源规则、可见性模式、是否绕过常规数据权限、发起部门范围和创建原因。

用途：

- 我的待办、已办和详情以快照判断任务归属。
- 审计可以解释“为什么这个人能处理这个任务”。
- 转办、委托、退回、空审批人兜底、自审重定向等场景都有明确来源。

## 后端入口

| 用途 | 文件 |
| --- | --- |
| 流程定义聚合 | `src/Ncp.Admin.Domain/AggregatesModel/WorkflowDefinitionAggregate/WorkflowDefinition.cs` |
| 流程实例聚合 | `src/Ncp.Admin.Domain/AggregatesModel/WorkflowInstanceAggregate/WorkflowInstance.cs` |
| 流程任务与快照 | `src/Ncp.Admin.Domain/AggregatesModel/WorkflowInstanceAggregate/WorkflowTask*.cs` |
| 流程定义 EF 配置 | `src/Ncp.Admin.Infrastructure/EntityConfigurations/WorkflowDefinitionEntityTypeConfiguration.cs` |
| 流程实例/任务 EF 配置 | `src/Ncp.Admin.Infrastructure/EntityConfigurations/WorkflowInstanceEntityTypeConfiguration.cs` |
| 流程定义仓储 | `src/Ncp.Admin.Infrastructure/Repositories/WorkflowDefinitionRepository.cs` |
| 流程实例仓储 | `src/Ncp.Admin.Infrastructure/Repositories/WorkflowInstanceRepository.cs` |
| 流程命令 | `src/Ncp.Admin.Web/Application/Commands/Workflows/` |
| 流程查询 | `src/Ncp.Admin.Web/Application/Queries/Workflow*.cs` |
| 流程服务 | `src/Ncp.Admin.Web/Application/Services/Workflow/` |
| 流程端点 | `src/Ncp.Admin.Web/Endpoints/Workflows/` |

## 核心服务

| 服务 | 职责 |
| --- | --- |
| `WorkflowGraphCompiler` | 发布时把设计器 schema 编译为 `GraphSnapshotJson` |
| `WorkflowGraphRuntimeService` | 运行时解析开始节点、首个任务节点、下一任务节点、上一审批节点和进度步骤 |
| `WorkflowOutgoingTaskService` | 审批通过后推进会签、或签、依次审批、抄送链和完成判定 |
| `WorkflowAssigneeResolverQuery` | 解析成员、部门负责人、角色、发起人、订单合同签订公司负责人等处理人来源，并填充授权元数据 |
| `WorkflowApprovalAssignmentService` | 应用数据权限、自审策略、空审批人策略 |
| `WorkflowRuntimeRecordService` | 将任务和处理人解析结果写入授权快照 |
| `WorkflowTaskVisibilityPolicy` | 任务创建前过滤无发起人数据范围权限的候选处理人 |
| `WorkflowVisibilityService` | 统一实例详情、任务展示和任务操作的可见性语义 |
| `WorkflowBusinessAdapterDispatcher` | 按 `BusinessType` 分发业务完成、驳回、取消、审批动作扩展和退回字段选项，并提供业务接入清单 |

## 设计器与运行图

前端保存的是设计器 schema；发布时后端编译出 `WorkflowGraph`：

- 节点类型：开始、审批、抄送、条件路由、结束、业务扩展。
- 审批模式：依次审批、会签、或签。
- 人员来源：指定成员、指定角色、部门负责人、部门负责人链、流程发起人、业务变量；订单审批节点还可按订单合同签订公司反查合同公司维护负责人。
- 节点级发起部门范围：角色和订单合同签订公司负责人支持沿用数据权限、不限部门、审批人权限 + 额外部门。
- 业务扩展通过 `ExtensionsJson` 承载，例如订单按钮展示和办公任务参与人配置。

## 并发与唯一性

发起流程时，应用层保留快速重复检查；数据库层通过 `(BusinessType, BusinessKey)` 在 `Running` / `Suspended` 状态下唯一的 partial index 作为最终兜底。上线迁移前需要先检查并清理已有重复运行中/挂起实例。

## 当前业务类型

文件：`src/Ncp.Admin.Web/Application/Commands/Workflows/WorkflowBusinessTypes.cs`

当前工作流通过 `IWorkflowBusinessAdapter` 接入用户创建、订单、人事申请、公积金/社保、办公任务、客户作废/协作、订餐取消等业务。新增业务时优先新增业务适配器，通用工作流命令只保留通用字段和 `ActionPayload`。

每个业务适配器可以通过 `Integration` 暴露接入清单：业务类型、条件字段、回调能力和审批动作 payload schema。业务 adapter 内部负责把 `ActionPayload` 解析为本业务强类型 DTO，并返回业务可读错误。

审批节点可以在设计器中配置退回字段选择：默认不选择字段，只填写退回说明；订单审批节点可选择“订单退回字段”方案，退回时必须从合同签订公司、发票类型、安装费、预计运费、无Logo、产品清单、上传合同、备货清单、项目联系人、项目联系方式、质保期和合同受托方中勾选至少一项。业务适配器实现 `GetReturnFieldOptionsAsync` 提供字段清单，订单保存逻辑根据退回上下文做二次白名单校验。订单退回待办属于 `WorkflowReturn` 编辑场景，虽然订单主状态仍是审核中，但当前处理人的勾选字段修改不套用普通审核中的分区冻结规则。

订单审批节点的审批人来源可以选择“合同签订公司负责人”。运行时根据订单 `ContractSigningCompany` 匹配合同公司维护（`ContractTypeOption.Name`），取维护的 `ResponsibleUserId` 作为审批人；该来源可配置节点级发起部门范围，负责人为空、已离职、合同公司为空或匹配不到时，按该节点现有“审批人为空时”的指定人员兜底策略处理。

退回目标按实际路径解析：当前节点之前有审批节点时回到上一审批节点的本轮实际处理人；当前节点是首个审批节点时回到开始节点，并把新待办分配给流程发起人。发起人处理该退回待办后，流程从开始节点继续推进并重新生成首个审批节点待办。

## 测试入口

| 层级 | 文件 | 覆盖点 |
| --- | --- | --- |
| 领域测试 | `test/Ncp.Admin.Domain.Tests/WorkflowDefinitionTests.cs` | 定义生命周期 |
| 领域测试 | `test/Ncp.Admin.Domain.Tests/WorkflowInstanceTests.cs` | 实例状态、任务权限、会签/或签、撤销 |
| Web 服务测试 | `test/Ncp.Admin.Web.Tests/WorkflowConditionEvaluatorTests.cs` | 条件分支表达式 |
| Web 服务测试 | `test/Ncp.Admin.Web.Tests/WorkflowDefinitionAssigneeConfigValidatorTests.cs` | 发布校验 |
| Web 服务测试 | `test/Ncp.Admin.Web.Tests/WorkflowOutgoingTaskServiceTests.cs` | 流程推进 |
| Web 服务测试 | `test/Ncp.Admin.Web.Tests/WorkflowRuntimeRecordServiceTests.cs` | 任务授权快照元数据 |
