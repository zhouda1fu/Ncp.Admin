/** 中国大陆手机号（11 位，1 开头） */
export const CHINA_MOBILE_REGEX = /^1[3-9]\d{9}$/;

/** QQ：5–11 位数字，首位非 0 */
export const QQ_REGEX = /^[1-9]\d{4,10}$/;

export function hasAtLeastOneContactChannel(
  mobile: unknown,
  phone: unknown,
  qq: unknown,
  wechat: unknown,
): boolean {
  const t = (v: unknown) => String(v ?? '').trim();
  return [mobile, phone, qq, wechat].some((v) => t(v).length > 0);
}

export function isValidMobileIfPresent(mobile: unknown): boolean {
  const m = String(mobile ?? '').trim();
  return m === '' || CHINA_MOBILE_REGEX.test(m);
}

export function isValidQqIfPresent(qq: unknown): boolean {
  const q = String(qq ?? '').trim();
  return q === '' || QQ_REGEX.test(q);
}
