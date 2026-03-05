import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace ProductApi {
  export interface ProductItem {
    id: string;
    name: string;
    code: string;
    model: string;
    unit: string;
  }
}

/**
 * 获取产品列表（分页，用于订单明细产品下拉及产品维护列表）
 */
async function getProductList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ProductApi.ProductItem[];
    total: number;
  }>('/products', { params });
  return res;
}

/**
 * 获取产品详情（用于编辑回填）
 */
async function getProduct(id: string) {
  return requestClient.get<ProductApi.ProductItem>(`/products/${id}`);
}

/**
 * 创建产品
 */
async function createProduct(data: {
  name: string;
  code: string;
  model: string;
  unit: string;
}) {
  return requestClient.post<{ id: string }>('/products', data);
}

/**
 * 更新产品
 */
async function updateProduct(
  id: string,
  data: { name: string; code: string; model: string; unit: string },
) {
  return requestClient.put(`/products/${id}`, data);
}

/**
 * 删除产品
 */
async function deleteProduct(id: string) {
  return requestClient.delete(`/products/${id}`);
}

export { getProductList, getProduct, createProduct, updateProduct, deleteProduct };
