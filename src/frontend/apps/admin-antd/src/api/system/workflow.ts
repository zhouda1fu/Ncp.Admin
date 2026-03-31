import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 条件字段可选值（有 options 时前端用下拉框选值） */
export interface ConditionFieldOption {
  value: string;
  label: string;
}

/** 条件字段定义（按分类从后端获取，供条件分支下拉选择） */
export interface ConditionFieldDef {
  key: string;
  label: string;
  type: 'number' | 'string' | 'boolean' | 'enum';
  /** 有值时「条件值」用下拉框，value 须与流程 Variables JSON 一致 */
  options?: ConditionFieldOption[];
}

export namespace WorkflowApi {
  /** 流程定义：节点树存于 definitionJson，前端按需解析 */
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
    definitionJson: string;
  }

  export interface WorkflowInstance {
    [key: string]: any;
    id: string;
    workflowDefinitionId: string;
    workflowDefinitionName: string;
    businessKey: string;
    businessType: string;
    title: string;
    initiatorId: string;
    initiatorName: string;
    status: number;
    currentNodeName: string;
    startedAt: string;
    completedAt?: string;
    remark: string;
  }

  /** 后端按实例变量解析条件后的进度步骤（与引擎分支一致） */
  export interface WorkflowProgressStep {
    title: string;
    nodeKey?: string;
  }

  export interface WorkflowInstanceDetail extends WorkflowInstance {
    variables: string;
    /** 当前节点 nodeKey，与流程定义一致，用于进度条精确匹配 */
    currentNodeKey?: string;
    /** 条件分支仅展示命中路径上的节点（新接口；缺省则前端回退解析 definitionJson） */
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
  definitionJson: string;
}) {
  return requestClient.post('/workflow/definitions', data);
}

/**
 * 更新流程定义
 */
async function updateDefinition(data: {
  id: string;
  name: string;
  description: string;
  category: string;
  definitionJson: string;
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
    '/workflow/delegate',
    data,
  );
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
  publishDefinition,
  rejectTask,
  startWorkflow,
  transferTask,
  createDefinitionNewVersion,
  updateDefinition,
};
