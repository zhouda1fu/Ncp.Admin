import { defineOverridesPreferences } from '@vben/preferences';

import { MODULE_MENU_DEFAULT_EXPANDED_PATHS } from '#/constants/module-menu-categories';

/**
 * @description 项目配置文件
 * 只需要覆盖项目中的一部分配置，不需要的配置不用覆盖，会自动使用默认配置
 * !!! 更改配置后请清空缓存，否则可能不生效
 */
export const overridesPreferences = defineOverridesPreferences({
  // overrides
  app: {
    name: import.meta.env.VITE_APP_TITLE,
    accessMode: 'frontend', // 设置为前端访问控制模式，使用 PermissionCodes 控制权限
    layout: 'sidebar-nav',
  },
  navigation: {
    accordion: true,
    split: false,
    /** 行政 / CRM / 系统 三大模块默认展开 */
    defaultOpeneds: [...MODULE_MENU_DEFAULT_EXPANDED_PATHS] as string[],
  },
});
