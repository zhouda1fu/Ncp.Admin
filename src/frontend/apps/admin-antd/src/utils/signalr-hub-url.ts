/**
 * 解析 SignalR Hub 地址（与 VITE_GLOB_API_URL 同源，如 http://host:5511/notification）。
 * 开发/生产均直连后端；后端 CORS 已放行前端源，无需经 Vite 代理（避免 proxy ECONNREFUSED）。
 */
export function resolveSignalRHubUrl(
  hubPath: 'chat' | 'notification',
  apiURL: string,
): string {
  const baseUrl =
    apiURL.replace(/\/api\/admin\/?$/i, '') || apiURL.split('/api')[0] || apiURL;
  return `${baseUrl.replace(/\/$/, '')}/${hubPath}`;
}
