import { requestClient } from '#/api/request';

export namespace HomeDashboardApi {
  export interface CalendarMemoDay {
    date: string;
    hasContent: boolean;
  }

  export interface Calendar {
    today: string;
    year: number;
    month: number;
    birthdayMonthDay: string | null;
    memoDays: CalendarMemoDay[];
  }

  /** 平台精简版首页工作台 */
  export interface HomeDashboard {
    workflowPendingTaskCount: number;
    unreadNotificationCount: number;
    calendar: Calendar;
    cardOrder: string[];
  }

  export interface CalendarMemo {
    memoDate: string;
    content: string;
  }
}

async function getHomeDashboard(params?: {
  calendarYear?: number;
  calendarMonth?: number;
}) {
  return requestClient.get<HomeDashboardApi.HomeDashboard>('/dashboard/home', {
    params,
  });
}

async function saveHomeDashboardLayout(cardOrder: string[]) {
  return requestClient.put<boolean>('/dashboard/home-layout', { cardOrder });
}

async function getCalendarMemo(date: string) {
  return requestClient.get<HomeDashboardApi.CalendarMemo | null>(
    '/dashboard/calendar-memo',
    {
      params: { date },
    },
  );
}

async function saveCalendarMemo(date: string, content: string) {
  return requestClient.put<boolean>('/dashboard/calendar-memo', { date, content });
}

export {
  getCalendarMemo,
  getHomeDashboard,
  saveCalendarMemo,
  saveHomeDashboardLayout,
};
