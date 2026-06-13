# 工作流模块重构设计方案

> 当前设计基线：运行期统一使用 `WorkflowGraph` / `GraphSnapshotJson`，任务授权以 `WorkflowTaskAssignmentSnapshot` 为准，业务扩展通过 `IWorkflowBusinessAdapter` 接入。

## 1. 目标

工作流模块需要同时满足 OA 审批的灵活配置和后端领域边界清晰：

- 流程定义可创建、编辑、发布、归档、新版本和软删除。
- 发布后生成稳定运行图快照，运行期不读取前端编辑结构。
- 审批、抄送、条件路由、会签、或签、依次审批可运行。
- 审批任务支持退回到实际路径上的上一审批节点；首个审批节点退回发起人，且不结束流程实例。
- 节点级发起部门范围、自审策略、空审批人策略可审计。
- 任务创建时写入授权快照，后续查询以快照判断任务归属。
- 业务特例通过 adapter 和 action payload 扩展，不污染通用命令。
- 数据库层防止同一业务重复发起运行中或挂起流程。

## 2. 领域模型

### WorkflowDefinition

流程定义聚合根，负责定义生命周期和版本管理。发布时将当前设计器 schema 编译为 `GraphSnapshotJson` 并写入最新草稿版本。

### WorkflowDefinitionVersion

流程定义版本子实体，保存 `DesignerSchemaJson` 和 `GraphSnapshotJson`。运行中的实例只绑定版本 ID 和运行图快照。

### WorkflowInstance

流程实例聚合根，保存业务类型、业务主键、发起人、发起部门、变量、当前节点、状态和任务集合。状态包括运行中、挂起、完成、驳回、取消和异常。

### WorkflowTask

流程任务子实体，表示审批、抄送或通知任务。任务负责状态变化、意见、完成时间和实际处理人记录。退回场景通过 `ExtraDataJson` 保存退回字段、退回说明和退回来源/目标节点。

### WorkflowTaskAssignmentSnapshot

任务授权快照，记录：

- 处理人类型、用户、角色、显示名。
- 授权来源：成员、角色、部门负责人、订单合同签订公司负责人、发起人、空审批人兜底、自审重定向、业务变量、转办、委托、退回。
- 来源规则 ID。
- 可见性模式：明确用户、角色数据权限、绕过数据权限、审批人权限 + 额外部门。
- 发起部门范围和创建原因。

## 3. 运行图

`WorkflowGraph` 是发布后的唯一运行期模型。

节点类型：

- `Start`
- `Approval`
- `CarbonCopy`
- `ConditionRoute`
- `End`
- `BusinessExtension`

处理人来源：

- `Member`
- `Role`
- `DeptResponsibleUser`
- `DeptResponsibleUserChain`
- `Initiator`
- `BusinessVariable`
- `OrderContractSigningCompanyResponsibleUser`

业务扩展字段进入节点 `ExtensionsJson`，例如订单申请技术按钮和办公任务参与人配置。

`OrderContractSigningCompanyResponsibleUser` 只用于订单审批节点。运行时按订单 `ContractSigningCompany` 匹配合同公司维护（`ContractTypeOption.Name`），取维护的 `ResponsibleUserId` 作为审批人；该来源支持节点级发起部门范围；负责人为空或不可用时不直接失败，而是返回空候选并交给 `WorkflowApprovalAssignmentService` 执行空审批人兜底策略。

## 4. 关键服务

| 服务 | 职责 |
| --- | --- |
| `WorkflowGraphCompiler` | 将设计器 schema 编译为运行图快照 |
| `WorkflowGraphRuntimeService` | 解析开始节点、首个任务节点、下一任务节点、上一审批节点和进度步骤 |
| `WorkflowAssigneeResolverQuery` | 解析处理人并填充授权元数据 |
| `WorkflowApprovalAssignmentService` | 应用数据权限、自审策略和空审批人策略 |
| `WorkflowDesignerTaskHelper` | 根据节点和审批模式创建任务 |
| `WorkflowRuntimeRecordService` | 按处理人解析结果写入授权快照 |
| `WorkflowOutgoingTaskService` | 审批通过后推进流程 |
| `WorkflowVisibilityService` | 统一实例详情、任务展示和任务操作的可见性判断 |
| `WorkflowBusinessAdapterDispatcher` | 分发业务回写、审批动作扩展、退回字段选项和业务接入描述 |

## 5. 发起唯一性

应用层先查是否存在相同 `BusinessType + BusinessKey` 的运行中实例，提供友好错误。数据库层再通过 partial unique index 限制 `Running` / `Suspended` 状态，作为并发兜底。

上线迁移前需要先执行重复数据检查：

```sql
SELECT "BusinessType", "BusinessKey", COUNT(*)
FROM workflow_instance
WHERE "Status" IN (0, 1)
GROUP BY "BusinessType", "BusinessKey"
HAVING COUNT(*) > 1;
```

## 6. 任务授权设计

任务创建流程：

1. 运行图节点解析处理人。
2. 处理人解析结果携带来源规则、可见性模式和发起部门范围。
3. 数据权限策略过滤不可处理候选人。
4. 创建 `WorkflowTask`。
5. `WorkflowRuntimeRecordService` 将 `WorkflowTask + WorkflowAssigneeResult` 配对写入 snapshot。

转办、委托和退回创建的新任务也写入明确来源，便于后续审计。

## 7. 业务扩展

通用命令保持业务无关。审批动作上的业务数据通过 `ActionPayload` 传入，由对应业务 adapter 在 `OnBeforeTaskApprovedAsync` 中解释。业务 adapter 内部应先解析为本业务强类型 DTO，再执行业务校验或准备动作。

示例：

```json
{
  "comment": "同意",
  "actionPayload": {
    "personnelBenefit": {
      "purchaserUserId": "731309885584572416"
    }
  }
}
```

业务接入描述由 `IWorkflowBusinessAdapter.Integration` 暴露，包含 `BusinessType`、条件字段、回调名称和 payload schema。它是新增业务的清单入口，避免只靠复制已有 adapter。

退回字段属于“节点配置 + 业务适配器”共同完成的扩展点。审批节点通过 `extensions.workflowReturn.fieldMode` 决定是否要求选择字段，通过 `fieldSetCode` 指向业务字段方案；`IWorkflowBusinessAdapter.GetReturnFieldOptionsAsync` 只负责按方案返回业务字段清单。通用工作流只保存字段模式、方案编码、字段 key、label、group 和退回上下文，不解释业务字段含义；具体业务如订单在自身保存逻辑中读取退回上下文并校验字段修改范围。

## 8. 退回设计

退回只支持退回固定目标，不支持任意节点退回。运行时使用 `WorkflowGraphRuntimeService.FindPreviousApprovalNodeKey` 按实例变量命中的条件路径查找上一审批节点，避免退回到未实际经过的分支；当前节点没有上一审批节点时，使用运行图开始节点作为目标并退回流程发起人。

退回执行规则：

1. 当前实例必须为 `Running`，当前任务必须为待处理审批任务。
2. 操作人必须命中任务授权快照或角色授权。
3. 未配置退回字段的审批节点默认为 `Disabled`，只要求退回说明。
4. 配置为 `Required` 的审批节点必须选择至少一个业务适配器返回的白名单字段。
5. 有上一审批节点时，目标处理人取本轮实际完成审批的人，角色审批取 `CompletedByUserId`；首个审批节点退回时，目标处理人取流程发起人。
6. 原任务状态为 `Returned`，同节点其他待办取消；新任务回到退回目标节点，授权来源为 `Returned`。
7. 退回上下文写入新任务 `ExtraDataJson`，用于前端展示和业务保存校验。
8. 退回待办再次通过后，已 `Returned` 的历史任务不阻止后续节点重新创建待办。

订单业务在退回编辑时只允许修改勾选细字段，当前字段方案为 `orderApprovalReturnFields`，包含合同签订公司、发票类型、安装费、预计运费、无Logo、产品清单、上传合同、备货清单、项目联系人、项目联系方式、质保期和合同受托方。退回待办按订单 `WorkflowReturn` 编辑场景处理，不等同于普通审核中审批任务；订单主状态仍可保持审核中，但当前处理人的勾选字段修改不套普通审核中分区冻结规则。

## 9. 当前完成情况

- 定义版本和运行图快照已落地。
- 运行期已统一为 `WorkflowGraph` / `GraphSnapshotJson`。
- 发起重复并发兜底已加入应用层异常翻译和 EF 索引配置。
- 任务授权快照已改为读取处理人解析结果元数据。
- 退回目标节点、退回字段上下文和订单退回编辑白名单已落地。
- 文档已按当前运行期结构对齐。
