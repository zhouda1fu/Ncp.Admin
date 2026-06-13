/** 将生日查询参数规范为 MM-DD（兼容历史 YYYY-MM-DD）。 */
export function normalizeBirthdayMonthDayQuery(v: unknown): string {
  const raw = String(v ?? '').trim();
  if (!raw) return '';
  if (/^\d{1,2}-\d{1,2}$/.test(raw)) {
    const [m, d] = raw.split('-');
    if (!m || !d) return raw;
    return `${m.padStart(2, '0')}-${d.padStart(2, '0')}`;
  }
  const legacy = raw.match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/);
  if (legacy?.[2] && legacy[3]) {
    return `${legacy[2].padStart(2, '0')}-${legacy[3].padStart(2, '0')}`;
  }
  return raw;
}

/** 员工生日等场景：仅展示月日（不含年），无效/空返回「-」。 */
export function formatBirthdayMonthDay(v: unknown): string {
  const raw = String(v ?? '').trim();
  if (!raw || raw === '-') return '-';
  if (raw.startsWith('0001-01-01')) return '-';
  const d = new Date(raw);
  if (Number.isNaN(d.getTime()) || d.getFullYear() <= 1900) return '-';
  return `${d.getMonth() + 1}月${d.getDate()}日`;
}
