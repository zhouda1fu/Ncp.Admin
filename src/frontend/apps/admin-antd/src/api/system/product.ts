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
 * 获取产品列表（分页，用于订单明细产品下拉等）
 */
async function getProductList(params: Recordable<any>) {
  const res = await requestClient.get<{
    items: ProductApi.ProductItem[];
    total: number;
  }>('/products', { params });
  return res;
}

export { getProductList };
