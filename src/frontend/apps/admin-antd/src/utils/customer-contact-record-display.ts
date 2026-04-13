const typeLabels: Record<number, string> = {
  1: '电话',
  2: '出差',
  3: '微信',
  4: '其他',
};

const legacyTypeString: Record<string, number> = {
  电话: 1,
  上门拜访: 2,
  微信: 3,
  其他: 4,
};

/** 联络记录类型展示（支持枚举值 1–4 与历史字符串） */
export function formatCustomerContactRecordType(type?: string | number | null): string {
  if (type === null || type === undefined || type === '') return '';
  if (typeof type === 'number') {
    if (type === 2) return '出差';
    return typeLabels[type] ?? String(type);
  }
  const t = String(type).trim();
  if (t === '上门拜访') return '出差';
  const n = legacyTypeString[t];
  if (n != null) return n === 2 ? '出差' : typeLabels[n] ?? t;
  return t;
}

/** 联络记录状态：0 待选择 1 有效联系 2 无效联系 */
export function formatCustomerContactRecordStatus(status?: number | null): string {
  if (status === 1) return '有效联系';
  if (status === 2) return '无效联系';
  return '待选择';
}

/** 将历史字符串类型转为 API 用的 recordType 枚举值 */
export function coerceRecordTypeToId(type?: string | number | null): number {
  if (typeof type === 'number' && type >= 1 && type <= 4) return type;
  const t = String(type ?? '').trim();
  return legacyTypeString[t] ?? 4;
}
