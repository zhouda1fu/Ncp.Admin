/**
 * 取产品分类名称的首字符（用于「合同优惠-」等短标签；按 Unicode 码位迭代，兼容常见汉字与 emoji）
 */
export function productCategoryNameInitial(name: string | undefined | null): string {
  const s = String(name ?? '').trim();
  if (!s) return '—';
  const first = [...s][0];
  return first ?? '—';
}
