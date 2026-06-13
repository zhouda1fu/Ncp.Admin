import { PermissionCodes } from '#/constants/permission-codes';
import { hasExplicitAppPermission } from '#/utils/app-permission';

export type HomeDashboardCardKey = 'process' | 'calendar';

export interface HomeDashboardCardMeta {
  key: HomeDashboardCardKey;
  titleKey: string;
  route?: string;
  permission?: string;
}

export const HOME_DASHBOARD_CARD_METAS: HomeDashboardCardMeta[] = [
  {
    key: 'process',
    titleKey: 'page.dashboard.home.cards.process',
    route: '/workflow/pending',
    permission: PermissionCodes.HomeDashboard,
  },
  {
    key: 'calendar',
    titleKey: 'page.dashboard.home.cards.calendar',
  },
];

export const HOME_DASHBOARD_CARD_META_MAP = Object.fromEntries(
  HOME_DASHBOARD_CARD_METAS.map((m) => [m.key, m]),
) as Record<HomeDashboardCardKey, HomeDashboardCardMeta>;

/** 置顶卡片：行事历固定展示 */
export const HOME_DASHBOARD_PINNED_CARD_KEYS: HomeDashboardCardKey[] = ['calendar'];

export const HOME_DASHBOARD_PINNED_CARD_KEY_SET = new Set<HomeDashboardCardKey>(
  HOME_DASHBOARD_PINNED_CARD_KEYS,
);

export function isHomeDashboardCardVisible(
  cardKey: HomeDashboardCardKey,
  accessCodes: string[] | undefined,
): boolean {
  if (cardKey === 'calendar') {
    return true;
  }
  const meta = HOME_DASHBOARD_CARD_META_MAP[cardKey];
  if (!meta?.permission) {
    return true;
  }
  return hasExplicitAppPermission(accessCodes, meta.permission);
}
