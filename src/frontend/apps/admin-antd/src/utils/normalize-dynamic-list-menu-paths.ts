import type { MenuRecordRaw } from '@vben/types';

/** 路由 path 含动态段时 generateMenus 可能产出带 `:param` 的 path，与 activePath 无法精确匹配 */
const DYNAMIC_LIST_MENU_PATH_REWRITES = new Map<string, string>();

/**
 * 将侧栏动态列表菜单 path 规范为固定 path，与子页 meta.link / meta.activePath 一致。
 */
export function normalizeDynamicListMenuPaths(menus: MenuRecordRaw[]): MenuRecordRaw[] {
  return menus.map((menu) => {
    const path = menu.path?.split('?')[0] ?? '';
    const fixedPath = DYNAMIC_LIST_MENU_PATH_REWRITES.get(path);
    if (fixedPath) {
      return { ...menu, path: fixedPath };
    }
    return menu;
  });
}
