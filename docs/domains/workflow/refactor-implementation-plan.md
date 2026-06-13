# 工作流模块重构实施拆解

> 本文承接 `refactor-design.md`，记录当前重构的实施状态和剩余上线注意事项。

## 1. 已完成基线

- [x] 独立 `workflow_definition_version` 表。
- [x] 发布版本保存 `GraphSnapshotJson`。
- [x] 运行期只读取 `WorkflowGraph` / `GraphSnapshotJson`。
- [x] 任务授权快照模型落地。
- [x] 业务回写通过 `IWorkflowBusinessAdapter` 分发。
- [x] 审批动作业务扩展通过 `ActionPayload` 传递。
- [x] 退回目标节点、退回字段和任务扩展上下文落地。

## 2. Phase 1

- [x] 移除流程级事件日志表。
- [x] 流程实例补充 `SuspendedAt` / `ResumedAt`。
- [x] 挂起/恢复行为记录时间。

## 3. Phase 2

- [x] 删除运行期旧定义 JSON 链路。
- [x] 运行期服务切换为 `WorkflowGraphNode`。
- [x] 发布校验改为基于 `WorkflowGraphCompiler` 输出。
- [x] 运行期只依赖 `GraphSnapshotJson`。
- [x] 命令处理器移除运行期 fallback。
- [x] Workflow 相关测试已按运行图模型重写。

## 4. Phase 3

- [x] `workflow_instance` 增加 `(BusinessType, BusinessKey)` active partial unique index 配置。
- [x] `StartWorkflowCommand` 专用管道行为按 active business index 翻译并发唯一约束冲突。
- [x] 补充并发唯一约束翻译测试。
- [x] 生成 EF migration（`20260525021033_InitDb` 已包含 `ix_workflow_instance_active_business`）。

上线前需要检查重复 active 实例：

```sql
SELECT "BusinessType", "BusinessKey", COUNT(*)
FROM workflow_instance
WHERE "Status" IN (0, 1)
GROUP BY "BusinessType", "BusinessKey"
HAVING COUNT(*) > 1;
```

## 5. Phase 4

- [x] `WorkflowAssigneeResult` 扩展授权来源、来源规则、可见性模式和发起部门范围。
- [x] `WorkflowAssigneeResolverQuery` 按成员、角色、部门负责人、发起人、订单合同签订公司负责人等分支填充元数据。
- [x] 空审批人兜底和自审重定向覆盖授权来源。
- [x] `WorkflowRuntimeRecordService` 从解析结果创建 snapshot。
- [x] 发起、推进、依次审批、转办、委托调用方同步为任务和解析结果配对。
- [x] 补充授权快照元数据测试。
- [ ] 历史 snapshot 回填 migration（若部署环境已有旧流程数据，仍需单独执行回填；全新库不需要）。

推荐历史回填策略：

- 已有用户任务回填为 `Member` / `ExplicitUser`。
- 已有角色任务回填为 `Role` / `RoleDataPermission`。
- 其余细粒度字段保持默认值，文档说明上线前历史数据不具备完整审计语义。

## 6. Phase 5

- [x] 更新 `README.md`。
- [x] 更新 `refactor-design.md`。
- [x] 更新 `refactor-implementation-plan.md`。
- [x] 更新 `dependencies.md`。
- [x] 更新 `flows.md`。
- [x] 删除施工蓝图。

## 7. Phase 6

- [x] 新增 `ReturnTaskCommand` 与 `POST /workflow/tasks/{taskId}/return`。
- [x] 新增 `GET /workflow/tasks/{taskId}/return-fields`，返回当前节点退回字段模式和业务适配器字段清单。
- [x] `WorkflowTask` 增加 `ExtraDataJson`，用于保存退回上下文。
- [x] `WorkflowTaskStatus` 增加 `Returned`，`WorkflowAssignmentSource` 增加 `Returned`。
- [x] 退回时按实际路径查找上一审批节点，并把新待办创建给上一节点本轮实际审批人；首个审批节点退回时创建给流程发起人。
- [x] 订单业务返回退回细字段，并在订单保存时校验只允许修改勾选字段。
- [x] 订单详情增加工作流编辑上下文，前端按退回字段控制可编辑区域。
- [x] 流程设计器审批节点支持配置退回字段选择模式和订单退回字段方案。
- [x] 待办页、实例详情和订单审批弹窗增加退回按钮与退回字段弹窗。

上线注意事项：

- 部署前必须执行 migration `20260525090000_WorkflowTaskReturnExtraData`，否则保存任务时会因 `workflow_task.ExtraDataJson` 缺列失败。
- 历史任务没有退回上下文时按 `{}` 处理，不需要补数据。

## 8. 验证顺序

统一代码完成后执行：

1. `dotnet build src/Ncp.Admin.Web/Ncp.Admin.Web.csproj`
2. `dotnet test test/Ncp.Admin.Web.Tests/Ncp.Admin.Web.Tests.csproj --filter Workflow`
3. 文档 grep 验证无过时运行期引用。

## 9. 交付标准

- 工作流定义可发布并生成运行图快照。
- 发起流程创建任务和 assignment snapshot。
- 待办、已办和详情以 snapshot 判断任务归属。
- 审批、退回、驳回、转办、委托、挂起、恢复可用。
- 订单退回编辑只允许修改退回时勾选的细字段。
- 同一业务重复 active 流程由数据库唯一约束兜底。
- Workflow 相关测试和 build 通过。
