# 工作流管理业务流程

> 修改工作流行为时优先看本文件。领域结构见 `README.md`，接口契约见 `api.md`。

## 流程定义生命周期

### 创建和编辑

1. 前端填写名称、分类、描述，并保存设计器 schema。
2. 后端创建 `WorkflowDefinition`，并由聚合根维护草稿版本。
3. 已发布定义不可直接编辑；新变化通过创建新版本承载。

### 发布

入口：`PublishDefinitionEndpoint` -> `PublishWorkflowDefinitionCommand`

1. 读取流程定义聚合及版本集合。
2. `WorkflowDefinitionAssigneeConfigValidator` 校验设计器 schema 和需要查库的处理人配置。
3. `WorkflowGraphCompiler` 编译出 `GraphSnapshotJson`。
4. `WorkflowDefinition.PublishLatestDraftVersion` 发布草稿版本。
5. 如果当前定义基于旧定义创建，发布事件会触发源定义归档。
6. 清理流程定义缓存。

## 发起流程

入口：`StartWorkflowEndpoint` -> `StartWorkflowCommand`

1. 应用层先检查同一 `BusinessType + BusinessKey` 是否已有 `Running` 实例。
2. 数据库 partial unique index 兜底限制同一业务只能有一个 `Running` / `Suspended` 实例。
3. 读取已发布定义版本和 `GraphSnapshotJson`。
4. 创建 `WorkflowInstance`，记录发起人、发起部门和变量 JSON。
5. `WorkflowGraphRuntimeService` 从开始节点解析首个审批或抄送节点。
6. `WorkflowApprovalAssignmentService` 解析候选处理人、应用数据权限、自审策略和空审批人策略。
7. `WorkflowDesignerTaskHelper` 按审批模式创建任务。
8. `WorkflowRuntimeRecordService` 按解析结果写入授权快照。
9. 抄送节点会连续创建并继续向后推进，直到遇到审批节点或结束。
10. 无任务且流程未显式允许自动完成时抛出业务异常。

## 审批通过

入口：`ApproveTaskEndpoint` -> `ApproveTaskCommand`

1. 读取流程实例及任务。
2. 业务适配器可在审批前把 `ActionPayload` 解析为本业务强类型 DTO，并做业务校验或准备。
3. `WorkflowInstance.ApproveTask` 校验任务待处理且当前用户命中任务归属。
4. 任务变为 `Approved`，记录意见、完成时间和实际处理人。
5. `WorkflowOutgoingTaskService.AdvanceAfterTaskApprovedAsync` 推进流程。

推进规则：

- 或签：任一人通过后取消同节点其他待办。
- 会签：当前节点全部通过后继续。
- 依次审批：同节点候选人仍有未审批者时只创建下一人任务。
- 抄送节点：创建抄送任务并继续寻找下一个任务节点。
- 无后续审批任务时实例完成，触发业务完成回写。

## 驳回

入口：`RejectTaskEndpoint` -> `RejectTaskCommand`

1. 校验实例运行中、任务待处理、当前用户有权处理。
2. 当前任务变为 `Rejected`。
3. 取消实例内其他待办。
4. 实例状态变为 `Rejected`，触发业务驳回回写和通知。

## 退回

入口：`ReturnTaskEndpoint` -> `ReturnTaskCommand`

退回和驳回不是同一语义：驳回会结束当前流程并进入业务驳回状态；退回把当前审批任务退回到实际路径上的上一审批节点，若当前任务已经是首个审批节点则退回发起人，流程实例继续保持 `Running`。

1. 校验实例运行中、当前任务为审批任务、当前用户有权处理。
2. 读取发布版本 `GraphSnapshotJson`，按实例变量命中的实际路径查找当前节点的上一审批节点；没有上一审批节点时使用开始节点作为退回目标。
3. 从当前审批节点运行图扩展读取 `workflowReturn.fieldMode` 和 `fieldSetCode`；未配置时默认为 `Disabled`。
4. `fieldMode=Required` 时，通过 `IWorkflowBusinessAdapter.GetReturnFieldOptionsAsync` 获取当前业务允许选择的退回字段，并用字段 key 做白名单校验；`fieldMode=Disabled` 时允许字段为空。
5. 有上一审批节点时，根据该节点本轮已通过任务找到实际审批人，角色任务优先使用任务通过时记录的实际操作人；首个审批节点退回时，目标处理人为流程发起人。
6. 当前任务标记为 `Returned`，同节点其他待办标记为 `Cancelled`，实例仍为运行中。
7. 在退回目标节点创建新的审批待办，授权快照来源为 `Returned`，可见性为明确指定用户。
8. 新待办的 `ExtraDataJson` 写入退回上下文：字段模式、字段方案、退回字段、退回说明、来源节点、目标节点和退回时间。
9. 退回待办被通过后，推进服务只把目标节点仍存在 `Pending` 待办视为已创建；历史 `Returned` 任务不会阻止重新生成后续审批节点待办。

订单审批的退回字段会影响订单详情编辑范围：

- 被退回节点的当前处理人打开订单详情时，前端通过订单工作流编辑上下文识别是否为退回编辑。
- 页面只放开退回时勾选的细字段，并展示退回说明。
- 退回待办属于订单的 `WorkflowReturn` 编辑场景，订单主状态可以仍是审核中，但勾选字段不再按普通审核中分区冻结处理。
- 保存订单时后端再次读取退回上下文并做字段差异校验，未勾选字段发生变化会拒绝保存。
- 未开启字段选择的节点只退回说明，不进入订单字段白名单限制。

## 转办

入口：`TransferTaskEndpoint` -> `TransferTaskCommand`

1. 校验原任务仍为待处理且当前操作者有权处理。
2. 原任务标记为 `Transferred`。
3. 创建同一节点的新用户任务。
4. 新任务授权快照来源为 `Transferred`，可见性为明确指定用户。

## 委托

入口：`DelegateTaskEndpoint` -> `DelegateWorkflowTaskCommand`

1. 校验原任务仍为待处理且当前操作者有权处理。
2. 原任务标记为 `Delegated`，备注记录委托对象。
3. 创建同一节点的新用户任务。
4. 新任务授权快照来源为 `Delegated`，可见性为明确指定用户。

## 撤销、挂起和恢复

撤销入口：`CancelWorkflowEndpoint` -> `CancelWorkflowCommand`

- 只有发起人可撤销运行中流程。
- 撤销会取消所有待办并触发业务取消回调。

挂起/恢复入口：

- `SuspendWorkflowEndpoint` -> `SuspendWorkflowCommand`
- `ResumeWorkflowEndpoint` -> `ResumeWorkflowCommand`

运行中实例可挂起并记录 `SuspendedAt`；挂起实例可恢复并记录 `ResumedAt`。待办列表只展示运行中实例的待办。

## 处理人解析

`WorkflowAssigneeResolverQuery` 基于运行图节点解析处理人：

- 指定成员：解析为明确用户，默认绕过数据权限过滤。
- 角色：展开为用户，按节点发起部门范围决定是否过滤数据权限，并在结果中记录来源规则和可见性模式。
- 部门负责人：以上一审批节点实际处理人作为锚点；无上一审批节点时回退发起人。
- 流程发起人：解析为发起人。
- 订单合同签订公司负责人：订单审批节点按订单 `ContractSigningCompany` 匹配合同公司维护，取 `ResponsibleUserId`；可按节点发起部门范围决定是否过滤数据权限；未解析到负责人时返回空候选，继续走节点的空审批人兜底策略。
- 空审批人兜底和自审重定向会覆盖授权来源，便于审计。

## 条件分支

条件分支由 `WorkflowGraphRuntimeService` 基于 `GraphSnapshotJson` 和 `WorkflowConditionEvaluator` 处理。变量来自实例 `Variables`，业务发起流程时必须保证变量字段和条件字段定义一致。

订单审批中若允许修改会影响条件分支的字段（如按产品分类的优惠点数），保存业务数据时必须同步刷新运行中实例的 `Variables`；流程推进和进度展示都读取实例变量，不会实时回查订单业务表。

订单审批节点可配置「进入节点订单状态」和「允许操作订单状态」。这些配置只改变流程运行期间的订单主状态，不代表流程结束；订单领域方法 `Order.IsWorkflowRunningStatus` 统一认定 `PendingAudit`、`Ordered`、`Unpaid` 为订单审批运行态。应用层判断订单是否仍在审批流程中时必须复用 `Order.IsInRunningWorkflow()` 或 `Order.IsWorkflowRunningStatus(...)`，不要单独写 `Status == PendingAudit`。

订单列表和订单详情的编辑权限按当前运行节点计算：未到达某节点时，用户即使拥有该节点角色也不能编辑；到达当前节点后，仍需满足该节点配置的「允许操作订单状态」。该规则既影响列表行的 `CanCurrentUserEditAndSubmit` 展示，也影响订单各分组保存接口的后端校验。

新增参与条件分支的订单字段时，需要同时补齐三件事：发起审批变量构建、运行中保存后的变量同步、覆盖 `PendingAudit` / `Ordered` / `Unpaid` 的回归测试。

## 业务回写

流程完成、驳回和取消后触发领域事件；业务回写统一由 `WorkflowBusinessAdapterDispatcher` 分发到对应 `IWorkflowBusinessAdapter`。业务回写失败时，可通过 `MarkWorkflowInstanceFaultedCommand` 将实例标记为 `Faulted` 并记录失败原因。

流程完成后的业务回写由完成事件处理器统一兜底：当 `OnCompletedAsync` 抛出异常时，流程实例会被标记为 `Faulted` 并记录失败原因，避免通用审批状态和业务回写失败混在一起。驳回、取消回写仍按具体业务语义处理，默认不改变实例终态。

## 办公任务参与人解析

办公任务使用业务类型 `OfficeTask`。创建办公任务前会读取已发布流程定义，按变量匹配路径上的办公任务参与人节点。运行时发起和推进会跳过该占位节点，不为主接收人生成工作流待办。
