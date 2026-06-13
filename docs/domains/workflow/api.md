# 工作流管理接口与契约

> 修改接口、DTO、权限、缓存、条件字段时看本文件。

## 前端 API

文件：`src/frontend/apps/admin-antd/src/api/system/workflow.ts`

### 流程定义

| 场景 | 方法 | 后端路径 |
| --- | --- | --- |
| 定义列表 | `getDefinitionList` | `GET /workflow/definitions` |
| 定义详情 | `getDefinition` | `GET /workflow/definitions/{id}` |
| 已发布定义 | `getPublishedDefinitions` | `GET /workflow/definitions/published` |
| 创建定义 | `createDefinition` | `POST /workflow/definitions` |
| 更新定义 | `updateDefinition` | `PUT /workflow/definitions` |
| 发布定义 | `publishDefinition` | `POST /workflow/definitions/{id}/publish` |
| 创建新版本 | `createDefinitionNewVersion` | `POST /workflow/definitions/{id}/new-version` |
| 删除草稿定义 | `deleteDraftDefinition` | `DELETE /workflow/definitions/{id}/draft` |
| 删除已发布/归档定义 | `deletePublishedDefinition` | `DELETE /workflow/definitions/{id}/published` |
| 条件字段 | `getConditionFields` | `GET /workflow/condition-fields/{category}` |

### 流程实例

| 场景 | 方法 | 后端路径 |
| --- | --- | --- |
| 发起流程 | `startWorkflow` | `POST /workflow/instances` |
| 实例列表 | `getInstanceList` | `GET /workflow/instances` |
| 实例详情 | `getInstance` | `GET /workflow/instances/{id}` |
| 撤销流程 | `cancelWorkflow` | `POST /workflow/instances/{id}/cancel` |
| 我发起的流程 | `getMyWorkflows` | `GET /workflow/my-workflows` |

### 流程任务

| 场景 | 方法 | 后端路径 |
| --- | --- | --- |
| 我的待办 | `getMyPendingTasks` | `GET /workflow/tasks/pending` |
| 我的已办 | `getMyCompletedTasks` | `GET /workflow/tasks/completed` |
| 审批通过 | `approveTask` | `POST /workflow/tasks/{taskId}/approve` |
| 获取退回字段 | `getTaskReturnFields` | `GET /workflow/tasks/{taskId}/return-fields` |
| 退回 | `returnTask` | `POST /workflow/tasks/{taskId}/return` |
| 驳回 | `rejectTask` | `POST /workflow/tasks/{taskId}/reject` |
| 抄送已读 | `readTask` | `POST /workflow/tasks/{taskId}/read` |
| 通知/确认完成 | `completeTask` | `POST /workflow/tasks/{taskId}/complete` |
| 转办 | `transferTask` | `POST /workflow/tasks/{taskId}/transfer` |
| 委托 | `delegateTask` | `POST /workflow/tasks/{taskId}/delegate` |

### 归档流程（人事申请）

文件：`src/frontend/apps/admin-antd/src/api/system/workflow.ts`  
后端：`src/Ncp.Admin.Web/Endpoints/Workflows/Archive/`

| 场景 | 方法 | 后端路径 | 权限（节选） |
| --- | --- | --- | --- |
| 分页列表 | `getArchivedPersonnelWorkflows` | `GET /workflow/archived-personnel-applications` | `WorkflowArchiveView` + 分类型 Tab 权限 |
| 全量列表（打印） | `getArchivedPersonnelWorkflowsAll` | `GET /workflow/archived-personnel-applications/all` | 同上 |
| 导出 Excel | `exportArchivedPersonnelWorkflowsExcel` | `GET /workflow/archived-personnel-applications/excel/export` | 同上 |

查询参数含 `applicationType`（0 请假、1 调休、2 存休、3 出差、4 加班、5 外出）、时间范围、关键字等。列表按申请类型校验 Tab 权限（`WorkflowArchiveLeave`、`WorkflowArchiveOvertime` 等）；仅有 `WorkflowArchiveView` 时可查看全部类型。

### 流程报表（人事汇总）

| 场景 | 方法 | 后端路径 | 权限 |
| --- | --- | --- | --- |
| 按员工+年月汇总 | `getPersonnelWorkflowReport` | `GET /workflows/personnel-report` | `WorkflowReportView` |
| 可选员工（数据权限内） | `getPersonnelWorkflowReportScopedUsers` | `GET /workflows/personnel-report/scoped-users` | `WorkflowReportView` |

查询参数：`userId`（**雪花 ID 字符串**，避免前端 `Number` 精度丢失）、`year`、`month`（`year=0` 或前端 `-1` 表示 2020 年起全部年份；`month=0` 为全年/全范围，1–12 为单月；选全部年份时不可再筛月份）。

数据来源：与流程归档口径一致，按 `PersonnelApplication` 中**已提交且审批中/已通过**的申请汇总；按日与 `PersonnelApplicationAttendanceSyncService.BuildDaySlices` 分摊天数/小时，**不依赖** `AttendanceWorkflowRecord` 是否已补同步。响应含汇总卡片、请假/调休饼图、按日考勤表与请假明细表；`userId` / `userIdText` 均以字符串返回。

员工下拉须调用 `scoped-users`（项含 `userIdText`），与报表查询的数据权限一致；前端选人与请求参数应优先使用 `userIdText`。越权查看他人报表返回 `WorkflowReportAccessDenied`（165025）。

### 人事申请扩展（撤回 / 外出报销）

文件：`src/frontend/apps/admin-antd/src/api/system/personnel-application.ts`  
后端：`PersonnelApplicationEndpoints.cs`

| 场景 | 方法 | 后端路径 | 权限 |
| --- | --- | --- | --- |
| 新增外出报销说明 | `addOutingReimbursementNote` | `POST /personnel-applications/{id}/outing-reimbursement-notes` | `PersonnelApplicationView` |
| 审核外出报销 | `approveOutingReimbursement` | `POST /personnel-applications/{id}/outing-reimbursement/approve` | `PersonnelApplicationView` |
| 申请撤回（已通过） | `requestWithdrawalRecall` | `POST /personnel-applications/{id}/withdrawal-recall/request` | `PersonnelApplicationCancel` |
| 同意撤回 | `approveWithdrawalRecall` | `POST /personnel-applications/{id}/withdrawal-recall/approve` | `PersonnelApplicationView` |
| 拒绝撤回 | `rejectWithdrawalRecall` | `POST /personnel-applications/{id}/withdrawal-recall/reject` | `PersonnelApplicationView` |

`approveTask` 的扩展业务数据统一放入 `actionPayload`，通用工作流命令不承载具体业务字段。例如人事公积金/社保指定购买执行人：

```json
{
  "workflowInstanceId": "instance-id",
  "taskId": "task-id",
  "comment": "同意",
  "actionPayload": {
    "personnelBenefit": {
      "purchaserUserId": "731309885584572416"
    }
  }
}
```

前端请求会通过统一 request client 加上 `/api/admin` 前缀。

`returnTask` 用于退回上一审批节点；如果当前任务是首个审批节点，则退回发起人。请求体包含：

```json
{
  "workflowInstanceId": "instance-id",
  "taskId": "task-id",
  "comment": "请修改合同附件",
  "returnFields": [
    {
      "key": "contractFiles",
      "label": "上传合同",
      "group": "合同信息"
    }
  ]
}
```

`getTaskReturnFields` 查询参数包含 `workflowInstanceId`。后端会校验当前用户必须能操作该待办，避免把业务字段白名单暴露给无关用户。响应体为当前节点的退回字段配置：

```json
{
  "fieldMode": "Required",
  "fieldSetCode": "orderApprovalReturnFields",
  "fields": [
    {
      "key": "contractFiles",
      "label": "上传合同",
      "group": "合同信息"
    }
  ]
}
```

`fieldMode=Disabled` 表示该审批节点退回时只需要填写退回说明，`returnFields` 可为空；`fieldMode=Required` 表示必须从 `fields` 中至少选择一个字段。

## 后端 Endpoint

主要目录：

- `src/Ncp.Admin.Web/Endpoints/Workflows/Definition/`
- `src/Ncp.Admin.Web/Endpoints/Workflows/Instance/`
- `src/Ncp.Admin.Web/Endpoints/Workflows/TaskEndpoints/`

端点使用 FastEndpoints，认证为 JWT，权限码由 `PermissionCodes` 控制。

## 核心 DTO

### WorkflowDefinitionQueryDto

文件：`WorkflowDefinitionQuery.cs`

字段：

- `Id`
- `Name`
- `Description`
- `Version`
- `Category`
- `Status`
- `CreatedBy`
- `CreatedAt`
- `DesignerSchemaJson`

### WorkflowInstanceQueryDto

文件：`WorkflowInstanceQuery.cs`

字段：

- `Id`
- `WorkflowDefinitionId`
- `WorkflowDefinitionName`
- `BusinessKey`
- `BusinessType`
- `Title`
- `InitiatorId`
- `InitiatorName`
- `Status`
- `CurrentNodeName`
- `StartedAt`
- `CompletedAt`
- `Remark`

### WorkflowInstanceDetailQueryDto

在实例列表基础上增加：

- `CurrentNodeKey`
- `Variables`
- `ProgressSteps`
- `Tasks`

`ProgressSteps` 由后端按实例变量解析条件分支后生成，避免前端与后端分支路径不一致。

### WorkflowTaskQueryDto

字段：

- `Id`
- `WorkflowInstanceId`
- `NodeKey`
- `NodeName`
- `TaskType`
- `AssigneeType`
- `AssigneeId`
- `AssigneeRoleId`
- `AssigneeName`
- `Status`
- `CanOperate`
- `Comment`
- `CreatedAt`
- `CompletedAt`
- `CompletedByUserId`（审批通过时的实际操作人；按用户指派时与处理人一致，按角色指派时记录点通过的用户）
- `CompletedByUserDisplayName`（详情查询时由后端解析的展示名，可为空）
- `ActorDeptName`（意见表等展示用：有实际操作人时取其部门，否则取待办处理人部门）
- `ActorRoleNames`（同上，取角色名称，多角色以「、」连接）
- `ReturnContext`（退回任务上下文；仅被退回节点的新待办或相关记录有值）

`CanOperate` 在详情页用于判断当前用户是否可操作该任务。

### WorkflowReturnFieldDto

退回字段由业务适配器返回，通用结构为：

- `Key`：字段 key，保存和校验都以它为准。
- `Label`：前端展示名称。
- `Group`：前端分组展示名称，可为空。

当前订单工作流退回字段：

| Key | Label | Group | 说明 |
| --- | --- | --- | --- |
| `contractSigningCompany` | 合同签订公司 | 合同信息 | 合同签订主体 |
| `invoiceTypeId` | 发票类型 | 财务信息 | 发票类型选择 |
| `installationFee` | 安装费 | 费用信息 | 安装费用 |
| `estimatedFreight` | 预计运费 | 费用信息 | 预计运费 |
| `isNoLogo` | 无Logo | 产品信息 | 是否无 Logo |
| `items` | 产品清单 | 产品信息 | 订单产品明细 |
| `contractFiles` | 上传合同 | 合同信息 | 合同上传附件 |
| `stockFiles` | 备货清单 | 附件资料 | 备货单上传附件 |
| `projectContactName` | 项目联系人 | 项目信息 | 项目联系人 |
| `projectContactPhone` | 项目联系方式 | 项目信息 | 项目联系方式 |
| `warranty` | 质保期 | 合同信息 | 销售质保期 |
| `contractTrustee` | 合同受托方 | 合同信息 | 合同受托方 |

历史退回上下文中的 `basic`、`customer`、`productItems`、`payment`、`contract`、`invoice`、`logistics`、`attachments`、`discountPoints` 分组 key 仍由订单保存校验兼容。

### WorkflowTaskReturnContextDto

退回上下文保存在 `WorkflowTask.ExtraDataJson` 的 `returnContext` 字段中：

- `FieldMode`：退回字段选择模式，`Disabled` 或 `Required`。
- `FieldSetCode`：退回字段方案编码，如 `orderApprovalReturnFields`。
- `ReturnFields`：本次退回勾选的字段。
- `Comment`：退回说明。
- `ReturnFromNodeKey` / `ReturnFromNodeName`：执行退回的来源节点。
- `ReturnToNodeKey` / `ReturnToNodeName`：退回目标节点；可能是上一审批节点，也可能是首个审批节点退回时的开始节点。
- `ReturnedAt`：退回时间。

旧任务或普通待办没有退回上下文时，`ExtraDataJson` 默认为 `{}`。

## 权限码

后端：

- `src/Ncp.Admin.Web/AppPermissions/PermissionCodes.cs`
- `src/Ncp.Admin.Web/AppPermissions/PermissionDefinitionContext.cs`

前端：

- `src/frontend/apps/admin-antd/src/constants/permission-codes.ts`
- `src/frontend/apps/admin-antd/src/router/routes/modules/workflow.ts`

工作流权限码：

- `WorkflowManagement`
- `WorkflowDefinitionView`
- `WorkflowDefinitionCreate`
- `WorkflowDefinitionEdit`
- `WorkflowDefinitionDelete`
- `WorkflowDefinitionPublish`
- `WorkflowStart`
- `WorkflowCancel`
- `WorkflowTaskApprove`
- `WorkflowInstanceView`
- `WorkflowMonitor`

注意：待办、已办、我发起的流程处在 `WorkflowManagement` 菜单下，但路由子项没有单独 authority。若后续区分普通用户和管理员入口，可再拆分更细权限。

## 缓存契约

文件：

- `WorkflowDefinitionQuery.cs`
- `WorkflowCacheKeys.cs`
- `WorkflowDefinitionCacheInvalidator.cs`

缓存项：

- 单条定义详情：10 分钟。
- 已发布定义列表：3 分钟。

清理时机：

- 创建定义：清理已发布列表缓存。
- 更新定义：清理当前定义缓存和已发布列表缓存。
- 删除定义：清理当前定义缓存和已发布列表缓存。
- 发布定义：清理当前定义缓存和已发布列表缓存。
- 发布新版本并归档源定义：额外清理源定义缓存。
- 创建新版本：清理已发布列表缓存。

## 条件字段契约

文件：

- `WorkflowConditionFieldsProvider.cs`
- `IWorkflowBusinessAdapter.cs`
- `src/Ncp.Admin.Web/Application/Services/Workflow/BusinessAdapters/`

条件字段由各业务适配器提供，再由 `WorkflowConditionFieldsProvider` 聚合返回：

- `Order`：到款情况、合同非公司模板、是否无 logo。
- `PersonnelApplication`：申请类型（`enumMulti`）、模板名称、天数；`ApplicantDeptId`（申请人部门）由接口按全部部门注入 `enumMulti`（多选，与路由角色一致）；不含请假类型、调休来源、小时数、是否产生费用。
- `CreateUser`：用户名、邮箱、真实姓名、手机号、状态、性别、部门 ID、部门名称。
- `CustomerSeaVoid`：当前返回空，由接口按角色表注入或业务侧另行处理。

条件字段的 `Key` 必须和实例 `Variables` JSON 中的属性名一致。

## 变量 JSON 契约

`DesignerSchemaJson` 中审批节点以 `approverConfigs` 作为审批人配置契约：

- 每个配置块包含 `setType`、`examineLevel`、`nodeAssigneeList`。
- 当 `setType = 3`（角色）或 `setType = 7`（订单合同签订公司负责人）时，配置块可包含 `initiatorDeptScopeMode` 与 `initiatorDeptList`，用于声明该节点上的发起部门授权范围。
  - `initiatorDeptScopeMode = 0`：沿用审批人自身数据权限，缺省值。
  - `initiatorDeptScopeMode = 1`：不限发起部门。
  - `initiatorDeptScopeMode = 2`：审批人自身权限 + 额外部门，必须配置至少一个 `initiatorDeptList` 部门；发起人部门未命中额外部门时，仍按审批人自身数据权限过滤。
  - `initiatorDeptList` 使用和 `nodeAssigneeList` 相同的选择项结构，`id` 为部门 ID，展示字段用于设计器回显。
- `setType = 7` 表示订单合同签订公司负责人，保存到后端 schema 时转换为 `source = orderContractSigningCompanyResponsibleUser`；该来源仅允许用于 `Order` 分类审批节点，可使用与角色相同的发起部门范围配置。
- 审批节点还包含 `emptyApproverPolicy`、`emptyApproverAssigneeList`、`selfApprovalPolicy`。
- 使用订单合同签订公司负责人来源时，`emptyApproverPolicy` 必须为指定人员审批，并且 `emptyApproverAssigneeList` 至少包含一个有效用户；负责人为空或不可用时由这组兜底人员审批。
- 订单审批节点可以包含 `orderApplyTechnologyVisible = true`，用于在订单审批当前节点到达该节点时展示订单列表“申请技术”按钮。
- `orderApplyTechnologyVisible` 只控制按钮展示，不参与审批任务处理，也不会推进流程。
- 该标记只允许配置在 `Order` 分类流程的审批节点上；保存/发布时会校验分类。
- 设计器支持在当前会话内复制审批节点的名称和通用人员配置，并在另一个审批节点中套用；该操作复制 `nodeName`、`approverConfigs`、空审批人策略、自审策略和多人审批方式，不复制节点 Key、后续链路或订单/办公任务/退回字段等业务扩展。
- 设计器支持复制审批/抄送节点开始的流程片段。复制时会默认选中当前节点及其后续节点，用户可取消不需要的节点；套用时会把片段插入到目标节点和目标节点原 `childNode` 之间，并为片段内所有节点重新生成 `nodeKey`。
- 条件分支抽屉也支持复制/套用流程片段，但只复制当前条件分支下面的流程节点，不复制条件表达式本身；套用时会插入到目标条件分支已有流程前面。

节点级发起部门范围是工作流专属契约，只影响当前流程节点的候选审批人、待办/已办/详情可见性和审批处理，不改变系统角色或用户自身的数据权限。`initiatorDeptScopeMode = 2` 表示在审批人原有可见范围上追加指定部门，不要求设计器同时选择审核人的本部门。

`DesignerSchemaJson` 中抄送节点以 `copyConfigs` 作为新增抄送人配置契约：

- 每个配置块包含 `setType`、`examineLevel`、`nodeAssigneeList`。
- 支持指定成员、部门负责人、角色、流程发起人。
- 抄送节点基础字段 `setType`、`nodeAssigneeList`、`examineLevel`、`userSelectFlag` 仅用于节点默认配置；多来源抄送统一使用 `copyConfigs`。
- `userSelectFlag = true` 且没有 `copyConfigs` / `nodeAssigneeList` / `officeTaskCarbonCopyList` 时，表示抄送人来自业务变量，例如办公任务新建页手动选择的抄送人。
- 设计器支持在当前会话内复制抄送节点的名称和抄送人配置，并在另一个抄送节点中套用；该操作同步 `nodeName`、`copyConfigs` 及旧兼容字段，不复制节点 Key 或后续链路。

`Variables` 是工作流和业务模块之间的主要契约：

- 条件分支读取它。
- 实例详情展示它。
- 完成事件处理器反序列化它并回写业务。

## 办公任务：流程参与人预览与解析（新建页）

### 接口

- **GET** `/api/admin/tasks/types/{TypeDefinitionId}/workflow-participants`
- **后端实现**：`GetOfficeTaskWorkflowParticipantConfigEndpoint` -> `OfficeTaskWorkflowParticipantService.GetConfigAsync`
- **前端页面**：`src/frontend/apps/admin-antd/src/views/task/tasks/form.vue`

查询参数：

- `orderId`：可选，关联订单 GUID。开票申请等带 `orderId` 跳转新建页时必须传入，用于匹配流程条件「订单ID不为空」并解析预设主接收人/抄送人（与 CAD 等仅按 `TypeCode` 分支的类型不同）。
- `receiverUserIds`：可选，重复 query 参数形式传递，例如 `?receiverUserIds=731309885584572416&receiverUserIds=...`。仅当主接收人模式为 `createPage` 且需要预览抄送部门负责人时使用。
- `receiverUserIds` 必须按字符串传递，不要在前端转成 `number`。用户 ID 可能超过 JavaScript 安全整数范围，转成 `number` 会导致低位精度丢失。

返回 `OfficeTaskWorkflowParticipantConfig`：

- **receiverMode**：`preset | createPage`
- **carbonCopyMode**：`preset | createPage`
- **receivers**：主接收人（展示用），每个用户包含 `userId`、`userIdText`、`displayName`
- **carbonCopies**：抄送人（展示用），每个用户包含 `userId`、`userIdText`、`displayName`

> 前端应优先使用 `userIdText` 或字符串形式的 `userId` 作为选择器 value。办公任务新建页内部的 `receiverUserIds`、`carbonCopyUserIds`、`approverUserIds` 都按 `string[]` 处理。

### 规则

- **参与人节点**：流程设计器中勾选「标识为办公任务主接收人/抄送人节点」的审批节点（type=1）。该选项只在 `OfficeTask` 分类展示。该节点解析结果或占位位置映射为新建页的“主接收人”。
- **紧跟抄送节点**：参与人节点之后必须紧跟一个抄送节点（type=2）。该节点解析结果映射为新建页的“抄送人”。
- **主接收人预设模式**：`officeTaskReceiverConfigMode = preset` 时，主接收人由审批节点配置解析，新建页展示结果且不允许手选。
- **主接收人新建页模式**：`officeTaskReceiverConfigMode = createPage` 时，审批节点在流程中占位，新建页必须手选主接收人；创建任务时后端校验至少一名主接收人。
- **抄送-部门负责人的锚点语义（重要）**：
  - 新建页预览阶段没有“上一审批任务”上下文。
  - 当紧跟抄送节点配置为 **部门负责人（setType=2）** 时，负责人链的锚点用户按 **主接收人** 计算（而不是发起人）。
  - 部门负责人解析逻辑依赖 `dept_responsible_user` 以及部门 `ParentId` 链；一个部门可配置多人负责人，解析结果按 `SortOrder` 去重返回。
- **抄送新建页模式**：`officeTaskCarbonCopyConfigMode = createPage`，或抄送节点 `userSelectFlag = true` 且没有静态抄送配置时，新建页允许手选抄送人。
- **抄送静态配置优先**：如果抄送节点已经配置了 `officeTaskCarbonCopyList` 或 `nodeAssigneeList`，即使存在自选标记，也按预设抄送解析，避免误判成新建页配置。

### 创建办公任务接口的参与人契约

`POST /api/admin/tasks` 中：

- `receiverUserIds`
- `carbonCopyUserIds`
- `approverUserIds`

都按字符串 ID 列表提交。端点 `CreateOfficeTaskEndpoint` 会用 `long.TryParse(..., InvariantCulture)` 显式解析成 `UserId`。前端禁止把这些 ID 转成 `number` 后再提交。

### 常见问题排查

- **抄送部门负责人为空**：
  - 检查主接收人所属部门及父级部门是否配置了部门负责人；或检查抄送节点 `examineLevel` 是否配置为预期层级（level=1 为本部门负责人，level=2 为父部门负责人）。
- **URL 中的用户 ID 尾数变成 00**：
  - 这是 JavaScript `number` 精度丢失，不是数据库或后端改写。检查前端是否把用户 ID 用 `Number(...)`、`parseInt(...)` 或 `number[]` 承载。应使用 `userIdText` 和 `string[]`。
- **新建页抄送显示为空**：
  - 如果还未选择主接收人，页面提示“选择主接收人后，将自动解析其部门负责人作为抄送人”。
  - 如果已选择主接收人但仍为空，页面提示“未解析到抄送人，可能所选主接收人所在部门及上级部门均未配置部门负责人”。

当前常见业务：

- `CreateUserVariables`
- 订单流程变量，由 `OrderWorkflowVariablesBuilder` 构建。
- 请假流程变量。
- 客户作废流程变量。

新增业务类型或字段时，要同时检查发起方、条件字段、业务适配器和完成/驳回/取消回调。流程定义保存/发布阶段会校验 JSON 格式、节点 key、条件路由和处理人配置；业务变量本身仍由具体业务发起方负责构建。

## 订单：节点驱动按钮展示

订单列表 DTO `OrderQueryDto` 包含 `canApplyTechnologyByWorkflow`。

返回规则：

- 订单必须关联运行中的 `Order` 工作流实例。
- 流程实例当前节点必须是审批节点。
- 流程定义当前节点必须配置 `orderApplyTechnologyVisible = true`。
- 后端只返回流程节点状态，不判断当前用户是否为该节点待办处理人。
- 前端按钮展示还需要叠加 `OrderTechnologyCreate` 权限和订单只读状态。

申请技术提交接口保持原业务契约，不携带工作流任务 ID，不自动审批或推进订单审批流程。

## 订单：退回编辑上下文

订单详情页通过 `GET /api/admin/orders/{id}/workflow-edit-context` 获取当前用户对订单的工作流编辑上下文。前端方法为 `getOrderWorkflowEditContext`。

返回字段：

- `canEdit`：当前用户是否处于可编辑的工作流任务上下文。
- `isReturnEdit`：是否为退回编辑。
- `returnFieldMode` / `returnFieldSetCode`：退回字段模式和字段方案。
- `returnFields`：退回时勾选的字段。
- `returnComment`：退回说明。
- `returnFromNodeName`：退回来源节点。
- `returnToNodeName`：退回目标节点。

前端根据 `returnFields` 放开对应表单控件；后端在订单分区保存命令中通过 `OrderReturnFieldEditGuard` 做二次校验，防止绕过前端只读状态提交未授权字段。`isReturnEdit=true` 时，订单保存按 `WorkflowReturn` 编辑场景处理，订单主状态可以仍是审核中，但当前退回待办处理人修改勾选字段时，不套普通审核中分区冻结规则。

## 并发契约

`WorkflowTask.Version` 是行版本。审批通过端点捕获 `DbUpdateConcurrencyException`，转换为：

```text
该任务已被处理，请刷新后重试
```

这可以防止同一待办被重复审批。涉及任务状态更新的端点也应保持同样的并发处理策略。退回端点同样会捕获并发冲突并返回该提示。
