/** 接入列表 return 的列表路由：分页/搜索 query 变化时合并 tab，并启用 keepAlive */
export const listReturnListRouteMeta = {
  keepAlive: true,
  fullPathKey: false as const,
};
