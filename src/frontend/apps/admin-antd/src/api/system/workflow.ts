import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace WorkflowApi {
  export interface WorkflowNode {
    id?: string;
    nodeName: string;
    nodeType: number;
    assigneeType: number;
    assigneeValue: string;
    sortOrder: number;
    description: string;
  }

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
    nodes: WorkflowNode[];
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

  export interface WorkflowInstanceDetail extends WorkflowInstance {
    variables: string;
    tasks: WorkflowTask[];
  }

  export interface WorkflowTask {
    id: string;
    workflowInstanceId: string;
    nodeName: string;
    taskType: number;
    assigneeId: string;
    assigneeName: string;
    status: number;
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
  nodes: WorkflowApi.WorkflowNode[];
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
  nodes: WorkflowApi.WorkflowNode[];
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
 * 删除流程定义
 */
async function deleteDefinition(id: string) {
  return requestClient.delete(`/workflow/definitions/${id}`);
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

export {
  approveTask,
  cancelWorkflow,
  createDefinition,
  deleteDefinition,
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
  updateDefinition,
};
