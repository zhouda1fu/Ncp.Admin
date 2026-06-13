import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 与后端 WorkflowBusinessTypes.CustomerCollaboration 一致 */
export const WORKFLOW_BUSINESS_TYPE_CUSTOMER_COLLABORATION = 'CustomerCollaboration';

/** 与后端 WorkflowDefinitionExportService 一致 */
export const WORKFLOW_DEFINITION_EXPORT_FORMAT = 'ncp-workflow-definition-export' as const;
export const WORKFLOW_DEFINITION_EXPORT_VERSION = 2;
/** 仍支持导入 v1 导出文件 */
export const WORKFLOW_DEFINITION_EXPORT_LEGACY_VERSION = 1;

export interface WorkflowDefinitionIdentityCatalogEntry {
  exportedId: string;
  name: string;
  accountName?: string;
}

export interface WorkflowDefinitionIdentityCatalogNodeEntry {
  nodeId: string;
  name: string;
  type: string;
}

/** 流程定义导出/导入 JSON 根结构（v2 含 identityCatalog，导入时按名称重映射 ID） */
export interface WorkflowDefinitionExportDocument {
  format: string;
  version: number;
  exportedAt: string;
  /** byName：导入时按用户/角色/部门名称匹配当前库 ID */
  remapStrategy?: string;
  definition: {
    name: string;
    description: string;
    category: string;
    designerSchemaJson: string;
  };
  identityCatalog?: {
    users: WorkflowDefinitionIdentityCatalogEntry[];
    roles: WorkflowDefinitionIdentityCatalogEntry[];
    depts: WorkflowDefinitionIdentityCatalogEntry[];
    nodes: WorkflowDefinitionIdentityCatalogNodeEntry[];
  };
}

export interface ImportWorkflowDefinitionResult {
  id: string;
  name: string;
  action: 'Created' | 'Updated';
  remapReport?: {
    usersRemapped: number;
    usersKept: number;
    usersUnresolved: number;
    rolesRemapped: number;
    rolesKept: number;
    rolesUnresolved: number;
    deptsRemapped: number;
    deptsKept: number;
    deptsUnresolved: number;
    warnings: string[];
  };
  warnings?: string[];
}

/** 条件字段可选值（有 options 时前端用下拉框选值） */
export interface ConditionFieldOption {
  value: string;
  label: string;
}

/** 条件字段定义（按分类从后端获取，供条件分支下拉选择） */
export interface ConditionFieldDef {
  key: string;
  label: string;
  type: 'number' | 'string' | 'boolean' | 'enum' | 'enumMulti';
  /** 有值时「条件值」用下拉框；enumMulti 为多选，存逗号分隔 value */
  options?: ConditionFieldOption[];
}

export namespace WorkflowApi {
  /** 流程定义：节点树存于 designerSchemaJson，前端按需解析 */
  export interface WorkflowDefinition {
    [key: string]: any;
    id: string;
    name: string;
    description: string;
    version: number;
    category: string;
    status: number;
    createdBy: string;
    createdAt: string;
    designerSchemaJson: string;
  }

  export interface WorkflowInstance {
    [key: string]: any;
    id: string;
    workflowDefinitionId: string;
    workflowDefinitionName: string;
    workflowDefinitionCategory?: string;
    businessKey: string;
    businessType: string;
    title: string;
    initiatorId: string;
    initiatorName: string;
    status: number;
    currentNodeName: string;
    startedAt: string;
    dueAt?: string;
    completedAt?: string;
    remark: string;
  }

  /** 后端按实例变量解析条件后的进度步骤（与引擎分支一致） */
  export interface WorkflowProgressStep {
    title: string;
    nodeKey?: string;
  }

  /** 退回时可勾选的业务字段 */
  export interface WorkflowReturnField {
    /** 后端业务白名单 key，退回提交和业务保存校验都按该值匹配 */
    key: string;
    /** 给审批人展示的字段名称 */
    label: string;
    /** 可选分组名称，仅用于弹窗分组展示 */
    group?: string | null;
  }

  /** 当前任务退回字段选择配置 */
  export interface WorkflowReturnOptions {
    /** Disabled 只填写退回说明；Required 必须勾选至少一个业务字段 */
    fieldMode: 'Disabled' | 'Required' | string;
    /** 业务字段方案编码，当前订单使用 orderApprovalReturnFields */
    fieldSetCode?: string | null;
    /** 后端按节点配置和业务适配器返回的可选字段白名单 */
    fields: WorkflowReturnField[];
  }

  /** 退回上下文，保存在被退回节点的新待办上 */
  export interface WorkflowTaskReturnContext {
    /** 退回发生时的字段选择模式，用于详情展示和业务编辑上下文判断 */
    fieldMode?: 'Disabled' | 'Required' | string;
    /** 退回发生时使用的业务字段方案编码 */
    fieldSetCode?: string | null;
    /** 本次退回审批人实际勾选的字段 */
    returnFields: WorkflowReturnField[];
    /** 审批人填写的退回说明 */
    comment: string;
    /** 执行退回的来源节点 key */
    returnFromNodeKey: string;
    /** 执行退回的来源节点名称 */
    returnFromNodeName: string;
    /** 退回目标节点 key */
    returnToNodeKey: string;
    /** 退回目标节点名称 */
    returnToNodeName: string;
    /** 退回发生时间 */
    returnedAt: string;
  }

  export interface WorkflowInstanceDetail extends WorkflowInstance {
    variables: string;
    /** 当前节点 nodeKey，与流程定义一致，用于进度条精确匹配 */
    currentNodeKey?: string;
    /** 最近一次挂起时间，用于详情时间线 */
    suspendedAt?: string | null;
    /** 最近一次恢复时间，用于详情时间线 */
    resumedAt?: string | null;
    /** 条件分支仅展示命中路径上的节点 */
    progressSteps?: WorkflowProgressStep[];
    tasks: WorkflowTask[];
  }

  export interface WorkflowTask {
    id: string;
    workflowInstanceId: string;
    nodeKey: string;
    nodeName: string;
    taskType: number;
    assigneeType: number;
    assigneeId: string;
    assigneeRoleId?: string;
    assigneeName: string;
    status: number;
    canOperate: boolean;
    comment: string;
    createdAt: string;
    completedAt?: string;
    /** 审批通过时的实际操作人（角色任务等） */
    completedByUserId?: string;
    completedByUserDisplayName?: string;
    /** 意见表展示用：实际操作人或待办处理人的部门 */
    actorDeptName?: string;
    /** 意见表展示用：实际操作人或待办处理人的角色（多角色以「、」连接） */
    actorRoleNames?: string;
    returnContext?: WorkflowTaskReturnContext | null;
  }

  export interface MyPendingTask {
    taskId: string;
    workflowInstanceId: string;
    workflowTitle: string;
    workflowDefinitionName: string;
    initiatorName: string;
    nodeName: string;
    taskType: number;
    createdAt: string;
  }

  export interface MyCompletedTask {
    taskId: string;
    workflowInstanceId: string;
    workflowTitle: string;
    workflowDefinitionName: string;
    initiatorName: string;
    nodeName: string;
    taskType: number;
    status: number;
    comment: string;
    createdAt: string;
    completedAt?: string;
  }

}

// ==================== 流程定义 API ====================

/**
 * 获取流程定义列表
 */
async function getDefinitionList(params: Recordable<any>) {
  return requestClient.get<{
    items: WorkflowApi.WorkflowDefinition[];
    total: number;
  }>('/workflow/definitions', { params });
}

/**
 * 获取流程定义详情
 */
async function getDefinition(id: string) {
  return requestClient.get<WorkflowApi.WorkflowDefinition>(
    `/workflow/definitions/${id}`,
  );
}

/**
 * 获取已发布的流程定义列表
 */
async function getPublishedDefinitions() {
  return requestClient.get<WorkflowApi.WorkflowDefinition[]>(
    '/workflow/definitions/published',
  );
}

/**
 * 创建流程定义
 */
async function createDefinition(data: {
  name: string;
  description: string;
  category: string;
  designerSchemaJson: string;
}) {
  return requestClient.post('/workflow/definitions', data);
}

/**
 * 导出流程定义 JSON（含身份名称目录，v2）
 */
async function exportWorkflowDefinition(id: string) {
  return requestClient.get<WorkflowDefinitionExportDocument>(
    `/workflow/definitions/${id}/export`,
  );
}

/**
 * 从导出 JSON 导入流程定义（按名称重映射 ID，默认 upsert 草稿）
 */
async function importWorkflowDefinition(
  body: WorkflowDefinitionExportDocument,
  options?: { upsertByName?: boolean },
) {
  return requestClient.post<ImportWorkflowDefinitionResult>(
    '/workflow/definitions/import',
    {
      ...body,
      upsertByName: options?.upsertByName ?? true,
    },
  );
}

/**
 * 更新流程定义
 */
async function updateDefinition(data: {
  id: string;
  name: string;
  description: string;
  category: string;
  designerSchemaJson: string;
}) {
  return requestClient.put('/workflow/definitions', data);
}

/**
 * 发布流程定义
 */
async function publishDefinition(id: string) {
  return requestClient.post(`/workflow/definitions/${id}/publish`, { id });
}

/**
 * 基于已有流程定义创建新版本
 */
async function createDefinitionNewVersion(id: string) {
  const data = await requestClient.post<{ id: string }>(
    `/workflow/definitions/${id}/new-version`,
    { id },
  );
  return data.id;
}

/**
 * 删除流程定义
 */
async function deleteDefinition(id: string) {
  return requestClient.delete(`/workflow/definitions/${id}`);
}

/**
 * 按流程分类获取条件分支可用字段（供结构化条件表单使用）
 */
async function getConditionFields(category: string) {
  return requestClient.get<ConditionFieldDef[]>(
    `/workflow/condition-fields/${encodeURIComponent(category)}`,
  );
}

// ==================== 流程实例 API ====================

/**
 * 发起流程
 */
async function startWorkflow(data: {
  workflowDefinitionId: string;
  businessKey: string;
  businessType: string;
  title: string;
  variables: string;
  remark: string;
}) {
  return requestClient.post('/workflow/instances', data);
}

/**
 * 获取流程实例列表
 */
async function getInstanceList(params: Recordable<any>) {
  return requestClient.get<{
    items: WorkflowApi.WorkflowInstance[];
    total: number;
  }>('/workflow/instances', { params });
}

/**
 * 获取流程实例详情
 */
async function getInstance(id: string) {
  return requestClient.get<WorkflowApi.WorkflowInstanceDetail>(
    `/workflow/instances/${id}`,
  );
}

/**
 * 撤销流程
 */
async function cancelWorkflow(id: string) {
  return requestClient.post(`/workflow/instances/${id}/cancel`, { id });
}

/**
 * 获取我发起的流程
 */
async function getMyWorkflows(params: Recordable<any>) {
  return requestClient.get<{
    items: WorkflowApi.WorkflowInstance[];
    total: number;
  }>('/workflow/my-workflows', { params });
}

// ==================== 工作流任务 API ====================

/**
 * 获取我的待办任务
 */
async function getMyPendingTasks(params: Recordable<any>) {
  return requestClient.get<{
    items: WorkflowApi.MyPendingTask[];
    total: number;
  }>('/workflow/tasks/pending', { params });
}

/**
 * 获取我的已办任务
 */
async function getMyCompletedTasks(params: Recordable<any>) {
  return requestClient.get<{
    items: WorkflowApi.MyCompletedTask[];
    total: number;
  }>('/workflow/tasks/completed', { params });
}

/**
 * 审批通过
 */
async function approveTask(data: {
  workflowInstanceId: string;
  taskId: string;
  comment: string;
  /** 审批动作扩展负载，由对应业务适配器解释 */
  actionPayload?: Record<string, unknown>;
}) {
  return requestClient.post(
    `/workflow/tasks/${data.taskId}/approve`,
    data,
  );
}

/**
 * 驳回
 */
async function rejectTask(data: {
  workflowInstanceId: string;
  taskId: string;
  comment: string;
}) {
  return requestClient.post(`/workflow/tasks/${data.taskId}/reject`, data);
}

/**
 * 获取任务退回字段
 */
async function getTaskReturnFields(data: {
  workflowInstanceId: string;
  taskId: string;
}) {
  return requestClient.get<WorkflowApi.WorkflowReturnOptions>(
    `/workflow/tasks/${data.taskId}/return-fields`,
    { params: data },
  );
}

/**
 * 退回上一审批节点
 */
async function returnTask(data: {
  workflowInstanceId: string;
  taskId: string;
  comment: string;
  returnFields: WorkflowApi.WorkflowReturnField[];
}) {
  return requestClient.post(`/workflow/tasks/${data.taskId}/return`, data);
}

/**
 * 转办
 */
async function transferTask(data: {
  workflowInstanceId: string;
  taskId: string;
  newAssigneeId: string;
  newAssigneeName: string;
  comment: string;
}) {
  return requestClient.post(
    `/workflow/tasks/${data.taskId}/transfer`,
    data,
  );
}

/**
 * 委托：将审批任务委托给他人处理
 */
async function delegateTask(data: {
  instanceId: string;
  taskId: string;
  delegateToUserId: string;
  delegateToUserName: string;
  comment: string;
}) {
  return requestClient.post<{ data: string }>(
    `/workflow/tasks/${data.taskId}/delegate`,
    data,
  );
}

/**
 * 抄送已读
 */
async function readTask(data: {
  workflowInstanceId: string;
  taskId: string;
  comment?: string;
}) {
  return requestClient.post(`/workflow/tasks/${data.taskId}/read`, data);
}

/**
 * 通知/确认任务完成
 */
async function completeTask(data: {
  workflowInstanceId: string;
  taskId: string;
  comment?: string;
}) {
  return requestClient.post(`/workflow/tasks/${data.taskId}/complete`, data);
}

export {
  approveTask,
  cancelWorkflow,
  createDefinition,
  delegateTask,
  deleteDefinition,
  getConditionFields,
  getDefinition,
  getDefinitionList,
  getInstance,
  getInstanceList,
  getMyCompletedTasks,
  getMyPendingTasks,
  getMyWorkflows,
  getPublishedDefinitions,
  importWorkflowDefinition,
  publishDefinition,
  readTask,
  rejectTask,
  getTaskReturnFields,
  returnTask,
  startWorkflow,
  transferTask,
  completeTask,
  createDefinitionNewVersion,
  exportWorkflowDefinition,
  updateDefinition,
};
