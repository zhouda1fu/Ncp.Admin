# 工作流管理依赖关系

> 判断工作流改动会影响哪些业务时看本文件。

## 依赖总览

| 模块 | 依赖点 |
| --- | --- |
| 用户模块 | 发起人、审批人、待办归属、角色查询、发起人部门、当前用户权限 |
| 角色模块 | 审批人按角色配置、角色展开用户、角色数据权限 |
| 部门模块 | 部门负责人审批锚点、发起人部门、指定部门及下级范围 |
| 合同公司维护 | 订单审批节点按合同签订公司解析负责人时读取 `ContractTypeOption.ResponsibleUserId` |
| 权限系统 | 工作流菜单、定义、发起、审批、监控权限 |
| 通知模块 | 任务创建、流程完成、驳回、取消时发送通知 |
| 业务模块 | 通过 `BusinessType` 和 `IWorkflowBusinessAdapter` 完成回写，并提供退回字段选项 |
| 数据权限 | 任务创建前过滤候选人；任务查询以授权快照为任务归属依据 |

## 用户、角色、部门

工作流依赖组织权限体系：

- `UserQuery.GetUserByIdAsync`：读取发起人、处理人显示名和部门。
- `UserQuery.GetRoleIdsByUserIdAsync`：判断当前用户是否命中角色任务。
- `UserQuery.GetUserAssigneesByRoleIdAsync`：角色配置展开用户。
- `DeptQuery.GetDeptByIdAsync`：部门负责人审批按部门链解析负责人。
- `DeptQuery.GetAllChildDeptIdsAsync`：判断指定部门及下级范围。
- `RoleQuery.GetAdminRolesForAssignmentAsync`：读取角色数据权限范围。

## 数据权限与快照

任务创建时，`WorkflowApprovalAssignmentService` 会调用 `WorkflowTaskVisibilityPolicy` 对候选审批人做数据权限过滤。解析结果会携带授权来源、规则 ID、可见性模式和节点级发起部门范围，再由 `WorkflowRuntimeRecordService` 写入 `WorkflowTaskAssignmentSnapshot`。

查询我的待办、已办和详情时，任务归属以 snapshot 为准，避免后续角色/部门关系变化破坏已有任务归属。节点级“审批人权限 + 额外部门”只影响当前工作流节点，不扩大角色或用户在其他业务页面的数据权限。

可见性语义集中在 `WorkflowVisibilityService`：

- 管理端实例列表继续使用 `WorkflowInstance` 全局数据权限过滤。
- 我的待办、我的已办使用 `IgnoreQueryFilters()` 读取流程实例，再按授权快照和快照上的 `BypassDataPermission` 判断任务展示。
- 实例详情允许管理数据范围命中，或工作流任务授权/已处理记录命中。
- 任务操作先通过授权快照判断当前用户是否命中待办，再进入聚合执行业务状态变更。

## 业务模块回写

工作流核心不直接写具体业务状态。业务回写统一通过 `IWorkflowBusinessAdapter` 接入，并由 `WorkflowBusinessAdapterDispatcher` 按 `BusinessType` 分发。业务接入清单通过 `IWorkflowBusinessAdapter.Integration` 暴露，包含条件字段、回调能力和动作 payload schema。

退回字段由审批节点配置和业务适配器共同提供：节点扩展 `workflowReturn.fieldMode` 决定是否要求选择字段，`fieldSetCode` 指向业务字段方案；`GetReturnFieldOptionsAsync` 返回当前业务允许在退回时勾选的细字段。工作流只保存退回上下文，不解释字段含义；业务保存逻辑负责根据上下文限制可修改字段。当前订单业务通过 `OrderWorkflowEditContextService` 和 `OrderReturnFieldEditGuard` 实现退回编辑白名单。

订单审批还可以把审批人来源配置为“合同签订公司负责人”。该解析依赖订单 `ContractSigningCompany` 和合同公司维护 `ContractTypeOption.Name` 的名称匹配，并读取 `ResponsibleUserId`。该来源支持节点级发起部门范围，决定负责人是否按自身数据权限过滤或在额外发起部门范围内放行。如果合同公司负责人为空、离职、订单合同签订公司为空或匹配不到，解析结果为空，由该审批节点已有的“审批人为空时”策略指定兜底审批人。

新增业务接入工作流通常需要：

1. 在 `WorkflowBusinessTypes` 增加业务类型常量。
2. 在前端流程分类中增加选项。
3. 提供发起流程入口，构建 `BusinessKey`、`BusinessType`、`Title`、`Variables`。
4. 新增一个 `IWorkflowBusinessAdapter` 实现，提供条件字段、审批动作扩展、退回字段、完成/驳回/取消回调和接入描述。
5. 为重复发起、业务状态回写和异常处理补测试。

## 前端设计器

前端保存设计器 schema，后端发布时编译为运行图快照。运行期代码只读取 `GraphSnapshotJson`，业务扩展配置统一进入运行图节点扩展字段。前端传递用户、部门、角色 ID 时继续使用字符串，避免长整型 ID 精度丢失。
