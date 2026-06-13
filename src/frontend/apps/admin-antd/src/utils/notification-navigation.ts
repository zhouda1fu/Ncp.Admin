import { isAssignedWorkflowInstanceId } from '#/utils/workflow-instance-id';

/** 通知项（含后端按角色解析的跳转路径，优先使用）。 */
export type NotificationLinkSource = {
  businessId?: string;
  businessType?: string;
  linkPath?: string | null;
  linkQuery?: Record<string, string> | null;
};

/** 根据通知项解析站内跳转（优先后端 linkPath，否则静态规则）。 */
export function notificationLinkFromItem(
  item: NotificationLinkSource,
): { link?: string; query?: Record<string, string> } {
  if (item.linkPath) {
    if (/^https?:\/\//i.test(item.linkPath)) {
      return { link: item.linkPath };
    }

    const query =
      item.linkQuery && Object.keys(item.linkQuery).length > 0
        ? item.linkQuery
        : undefined;

    return query ? { link: item.linkPath, query } : { link: item.linkPath };
  }

  return notificationLinkFromBusiness(item.businessId, item.businessType);
}

/** 根据通知业务类型解析站内跳转路径（平台脚手架静态兜底）。 */
export function notificationLinkFromBusiness(
  businessId?: string,
  businessType?: string,
): { link?: string; query?: Record<string, string> } {
  if (!businessType) {
    return {};
  }

  if (businessType === 'WorkflowInstance') {
    if (!isAssignedWorkflowInstanceId(businessId)) {
      return { link: '/workflow/pending' };
    }

    return { link: `/workflow/instance/${businessId}` };
  }

  return {};
}
