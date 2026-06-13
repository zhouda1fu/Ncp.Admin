import type { Router } from 'vue-router';

import { $t } from '#/locales';

/** 将路由路径解析为可读的页面名称，识别不到时回退为路径。 */
export function resolveRoutePageLabel(router: Router, path: string) {
  const target = (path?.trim() || '/').split('?')[0] ?? '/';
  try {
    const resolved = router.resolve(target);
    const matched = resolved.matched;
    if (matched.length === 0) {
      return target;
    }

    const leaf = matched.at(-1);
    const leafTitle = leaf?.meta?.title;
    if (typeof leafTitle === 'string' && leafTitle.trim()) {
      return $t(leafTitle).trim();
    }

    const menuRecord = [...matched]
      .reverse()
      .find((route) => !route.meta?.hideInMenu);
    const menuTitle = menuRecord?.meta?.title;
    if (typeof menuTitle === 'string' && menuTitle.trim()) {
      return $t(menuTitle).trim();
    }
  } catch {
    // ignore resolve errors
  }

  return target;
}
