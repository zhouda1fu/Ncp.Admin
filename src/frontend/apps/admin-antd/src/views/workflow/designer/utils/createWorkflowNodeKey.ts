/**
 * 生成流程设计器节点唯一 nodeKey。
 * 不使用 Date.now() 单独拼接：同一毫秒内多次调用会得到重复 key，后端校验会失败。
 */
function randomIdSegment(): string {
  const c = globalThis.crypto;
  if (c && typeof c.randomUUID === 'function') {
    return c.randomUUID().replace(/-/g, '');
  }
  return `${Date.now()}_${Math.random().toString(36).slice(2, 11)}`;
}

export function createWorkflowNodeKey(): string {
  return `flk${randomIdSegment()}`;
}

/** 发起人根节点 nodeKey */
export function createWorkflowRootNodeKey(): string {
  return `root_${randomIdSegment()}`;
}
