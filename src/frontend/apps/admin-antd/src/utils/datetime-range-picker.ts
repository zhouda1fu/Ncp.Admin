import type { Dayjs } from 'dayjs';

type DisabledTimeConfig = {
  disabledHours?: () => number[];
  disabledMinutes?: (hour: number) => number[];
  disabledSeconds?: (hour: number, minute: number) => number[];
};

function range(from: number, to: number): number[] {
  if (to <= from) return [];
  return Array.from({ length: to - from }, (_, index) => from + index);
}

/** 结束日期不能早于开始日期（与售后/订单技术申请草稿一致）。 */
export function disableEndDateBeforeStart(start: Dayjs | null | undefined) {
  return (current: Dayjs) => {
    if (!start) return false;
    return current.endOf('day').isBefore(start.startOf('day'));
  };
}

/** 结束时刻不能早于开始时刻（同一天禁用开始前的时/分/秒）。 */
export function disableEndTimeBeforeStart(start: Dayjs | null | undefined) {
  return (current: Dayjs | null | undefined): DisabledTimeConfig => {
    if (!start || !current) return {};
    if (current.isAfter(start, 'day')) return {};
    if (current.isBefore(start, 'day')) {
      return {
        disabledHours: () => range(0, 24),
        disabledMinutes: () => range(0, 60),
        disabledSeconds: () => range(0, 60),
      };
    }
    const startHour = start.hour();
    const startMinute = start.minute();
    const startSecond = start.second();
    return {
      disabledHours: () => range(0, startHour),
      disabledMinutes: (hour: number) => {
        if (hour > startHour) return [];
        if (hour < startHour) return range(0, 60);
        return range(0, startMinute);
      },
      disabledSeconds: (hour: number, minute: number) => {
        if (hour > startHour || minute > startMinute) return [];
        if (hour < startHour || minute < startMinute) return range(0, 60);
        return range(0, startSecond);
      },
    };
  };
}
