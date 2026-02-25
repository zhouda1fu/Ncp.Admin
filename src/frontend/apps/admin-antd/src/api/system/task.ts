import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace TaskApi {
  /** 任务状态：0 待办 1 进行中 2 已完成 3 已取消 */
  export type TaskStatus = 0 | 1 | 2 | 3;

  export interface TaskCommentItem {
    id: string;
    content: string;
    authorId: string;
    createdAt: string;
  }

  export interface TaskItem {
    id: string;
    projectId: string;
    title: string;
    description?: string;
    assigneeId?: string;
    dueDate?: string;
    status: TaskStatus;
    sortOrder: number;
    createdAt: string;
    comments: TaskCommentItem[];
  }
}

/**
 * 获取任务列表（分页）
 */
async function getTaskList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: TaskApi.TaskItem[];
    total: number;
  }>('/tasks', { params });
  return res;
}

/**
 * 创建任务
 */
async function createTask(data: {
  projectId: string;
  title: string;
  description?: string;
  assigneeId?: number;
  dueDate?: string;
  sortOrder?: number;
}) {
  return requestClient.post<{ id: string }>('/tasks', data);
}

/**
 * 更新任务
 */
async function updateTask(
  id: string,
  data: {
    title: string;
    description?: string;
    assigneeId?: number;
    dueDate?: string;
  },
) {
  return requestClient.put(`/tasks/${id}`, data);
}

/**
 * 设置任务状态
 */
async function setTaskStatus(id: string, status: number) {
  return requestClient.put(`/tasks/${id}/status`, { status });
}

/**
 * 添加任务评论
 */
async function addTaskComment(taskId: string, content: string) {
  return requestClient.post(`/tasks/${taskId}/comments`, { content });
}

export {
  addTaskComment,
  createTask,
  getTaskList,
  setTaskStatus,
  updateTask,
};
