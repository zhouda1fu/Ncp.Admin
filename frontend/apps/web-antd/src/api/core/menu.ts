import type { RouteRecordStringComponent } from '@vben/types';

import { requestClient } from '#/api/request';

/**
 * 获取用户所有菜单
 * 注意：如果后端没有此接口，将返回空数组
 */
export async function getAllMenusApi() {
  try {
    return await requestClient.get<RouteRecordStringComponent[]>('/menu/all');
  } catch (error) {
    // 如果后端没有菜单接口，返回空数组，使用静态路由
    console.warn('菜单接口不可用，使用静态路由:', error);
    return [];
  }
}
