import { defineConfig } from '@vben/vite-config';

export default defineConfig(async () => {
  const { readFile } = await import('node:fs/promises');
  const packageJson = JSON.parse(
    await readFile(new URL('./package.json', import.meta.url), 'utf-8'),
  );
  const buildTime = new Date().toISOString();
  const buildVersion = `${packageJson.version}-${buildTime}`;

  return {
    application: {},
    vite: {
      plugins: [
        {
          generateBundle() {
            this.emitFile({
              fileName: 'version.json',
              source: `${JSON.stringify(
                {
                  buildTime,
                  name: packageJson.name,
                  version: buildVersion,
                },
                null,
                2,
              )}\n`,
              type: 'asset',
            });
          },
          name: 'ncp:version-file',
        },
      ],
      server: {
        // 缓解 Chromium 开发时出现 net::ERR_CACHE_READ_FAILURE（读本地 HTTP 缓存失败）：
        // 不强缓存模块响应，避免浏览器反复从损坏/锁定的磁盘缓存读 lodash/vue 等预构建依赖。
        headers: {
          'Cache-Control': 'no-store',
        },
        proxy: {
          '/api': {
            changeOrigin: true,
            rewrite: (path) => path.replace(/^\/api/, ''),
            // mock代理目标地址
            target: 'http://localhost:5320/api',
            ws: true,
          },
          // SignalR 不走 Vite 代理，见 utils/signalr-hub-url.ts（直连 VITE_GLOB_API_URL 同源）
        },
      },
    },
  };
});
