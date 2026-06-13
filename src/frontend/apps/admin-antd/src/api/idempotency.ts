/** 生成幂等键：优先 crypto.randomUUID，HTTP 环境回退兼容实现 */
export function getRandomIdempotencyKey(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

export function idempotencyRequestConfig(options?: { idempotencyKey?: string }) {
  return {
    headers: {
      'Idempotency-Key': options?.idempotencyKey ?? getRandomIdempotencyKey(),
    },
  };
}
