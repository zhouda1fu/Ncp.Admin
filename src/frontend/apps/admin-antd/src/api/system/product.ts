import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

/** 产品类型项（下拉、列表、维护） */
export interface ProductTypeItem {
  id: string;
  name: string;
  sortOrder: number;
  visible: boolean;
}

export namespace ProductApi {
  export interface ProductItem {
    id: string;
    productTypeId?: string;
    status?: boolean;
    name: string;
    code: string;
    model: string;
    unit: string;
    barcode?: string;
    activationCode?: string;
    priceStandard?: string;
    marketSales?: string;
    description?: string;
    costPrice?: number;
    customerPrice?: number;
    qty?: number;
    tags?: string;
    feature?: string;
    configuration?: string;
    instructions?: string;
    installProcess?: string;
    operationProcessResources?: string;
    introduction?: string;
    introductionResources?: string;
    imagePath?: string;
    categoryId?: string | null;
    /** 列表/详情查询联表返回 */
    categoryName?: string;
    supplierId?: string | null;
  }

  /** 创建/更新产品请求 */
  export interface ProductCreateUpdate {
    productTypeId: string;
    status: boolean;
    name: string;
    code: string;
    model: string;
    unit: string;
    barcode: string;
    activationCode: string;
    priceStandard: string;
    marketSales: string;
    description: string;
    costPrice: number;
    customerPrice: number;
    qty: number;
    tags: string;
    feature: string;
    configuration: string;
    instructions: string;
    installProcess: string;
    operationProcessResources: string;
    introduction: string;
    introductionResources: string;
    imagePath: string;
    categoryId?: string | null;
    supplierId?: string | null;
  }
}

/** 产品分类树节点（用于 TreeSelect） */
export interface ProductCategoryTreeItem {
  id: string;
  name: string;
  remark: string;
  parentId: string | null;
  sortOrder: number;
  visible: boolean;
  isDiscount: boolean;
  children: ProductCategoryTreeItem[];
}

/** 产品分类单条（用于编辑回填） */
export interface ProductCategoryItem {
  id: string;
  name: string;
  remark: string;
  parentId: string | null;
  sortOrder: number;
  visible: boolean;
  isDiscount: boolean;
}

/** 供应商项（用于下拉、列表） */
export interface SupplierItem {
  id: string;
  fullName: string;
  shortName: string;
  contact: string;
  phone: string;
  email?: string;
  address?: string;
  remark?: string;
}

/** 产品参数项 */
export interface ProductParameterItem {
  id: string;
  productId: string;
  year: string;
  description: string;
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
async function createProduct(data: ProductApi.ProductCreateUpdate) {
  return requestClient.post<{ id: string }>('/products', data);
}

/**
 * 更新产品
 */
async function updateProduct(id: string, data: ProductApi.ProductCreateUpdate) {
  return requestClient.put(`/products/${id}`, data);
}

/**
 * 删除产品
 */
async function deleteProduct(id: string) {
  return requestClient.delete(`/products/${id}`);
}

/**
 * 获取产品类型列表（用于产品表单、产品类型维护）
 */
async function getProductTypeList(includeInvisible = false) {
  return requestClient.get<ProductTypeItem[]>('/product-types', {
    params: { includeInvisible },
  });
}

/**
 * 获取产品类型详情（用于编辑回填）
 */
async function getProductType(id: string) {
  return requestClient.get<ProductTypeItem>(`/product-types/${id}`);
}

/**
 * 创建产品类型
 */
async function createProductType(data: { name: string; sortOrder?: number; visible?: boolean }) {
  return requestClient.post<{ id: string }>('/product-types', data);
}

/**
 * 更新产品类型
 */
async function updateProductType(
  id: string,
  data: { name: string; sortOrder: number; visible: boolean },
) {
  return requestClient.put(`/product-types/${id}`, data);
}

/**
 * 删除产品类型
 */
async function deleteProductType(id: string) {
  return requestClient.delete(`/product-types/${id}`);
}

/**
 * 获取产品分类树（用于产品表单分类选择、分类管理）
 */
async function getProductCategoryTree(includeInvisible = false, cacheBust = false) {
  return requestClient.get<ProductCategoryTreeItem[]>('/product-categories/tree', {
    params: { includeInvisible, ...(cacheBust ? { _t: Date.now() } : {}) },
  });
}

/**
 * 获取产品分类详情（用于编辑回填）
 */
async function getProductCategory(id: string) {
  return requestClient.get<ProductCategoryItem>(`/product-categories/${id}`);
}

/**
 * 创建产品分类
 */
async function createProductCategory(data: {
  name: string;
  remark: string;
  parentId?: string | null;
  sortOrder?: number;
  visible?: boolean;
  isDiscount?: boolean;
}) {
  return requestClient.post<{ id: string }>('/product-categories', data);
}

/**
 * 更新产品分类
 */
async function updateProductCategory(
  id: string,
  data: {
    name: string;
    remark: string;
    parentId?: string | null;
    sortOrder?: number;
    visible?: boolean;
    isDiscount?: boolean;
  },
) {
  return requestClient.put(`/product-categories/${id}`, data);
}

/**
 * 删除产品分类
 */
async function deleteProductCategory(id: string) {
  return requestClient.delete(`/product-categories/${id}`);
}

/**
 * 获取供应商列表（用于产品表单供应商选择、供应商管理）
 */
async function getSupplierList(keyword?: string) {
  return requestClient.get<SupplierItem[]>('/suppliers', {
    params: { keyword: keyword ?? undefined },
  });
}

/**
 * 获取供应商详情（用于编辑回填）
 */
async function getSupplier(id: string) {
  return requestClient.get<SupplierItem>(`/suppliers/${id}`);
}

/**
 * 创建供应商
 */
async function createSupplier(data: {
  fullName: string;
  shortName: string;
  contact: string;
  phone: string;
  email?: string;
  address?: string;
  remark?: string;
}) {
  return requestClient.post<{ id: string }>('/suppliers', data);
}

/**
 * 更新供应商
 */
async function updateSupplier(
  id: string,
  data: {
    fullName: string;
    shortName: string;
    contact: string;
    phone: string;
    email?: string;
    address?: string;
    remark?: string;
  },
) {
  return requestClient.put(`/suppliers/${id}`, data);
}

/**
 * 删除供应商
 */
async function deleteSupplier(id: string) {
  return requestClient.delete(`/suppliers/${id}`);
}

/**
 * 获取某产品的参数列表
 */
async function getProductParameterList(productId: string) {
  return requestClient.get<ProductParameterItem[]>(`/products/${productId}/parameters`);
}

/**
 * 创建产品参数
 */
async function createProductParameter(
  productId: string,
  data: { year: string; description: string },
) {
  return requestClient.post<{ id: string }>(`/products/${productId}/parameters`, data);
}

/**
 * 更新产品参数
 */
async function updateProductParameter(
  id: string,
  data: { year: string; description: string },
) {
  return requestClient.put(`/product-parameters/${id}`, data);
}

/**
 * 删除产品参数
 */
async function deleteProductParameter(id: string) {
  return requestClient.delete(`/product-parameters/${id}`);
}

export {
  getProductList,
  getProduct,
  createProduct,
  updateProduct,
  deleteProduct,
  getProductTypeList,
  getProductType,
  createProductType,
  updateProductType,
  deleteProductType,
  getProductCategoryTree,
  getProductCategory,
  createProductCategory,
  updateProductCategory,
  deleteProductCategory,
  getSupplierList,
  getSupplier,
  createSupplier,
  updateSupplier,
  deleteSupplier,
  getProductParameterList,
  createProductParameter,
  updateProductParameter,
  deleteProductParameter,
};
