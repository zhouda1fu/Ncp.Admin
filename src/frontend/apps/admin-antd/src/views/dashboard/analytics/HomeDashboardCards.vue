<script lang="ts" setup>
import type { HomeDashboardApi } from '#/api/system/home-dashboard';

import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import { useSortable } from '@vben/hooks';
import { IconifyIcon } from '@vben/icons';
import { useAccessStore } from '@vben/stores';

import { Button, Input, message, Spin } from 'ant-design-vue';
import dayjs, { type Dayjs } from 'dayjs';

import {
  getCalendarMemo,
  saveCalendarMemo,
  saveHomeDashboardLayout,
} from '#/api/system/home-dashboard';
import { $t } from '#/locales';

import {
  HOME_DASHBOARD_CARD_META_MAP,
  HOME_DASHBOARD_PINNED_CARD_KEY_SET,
  HOME_DASHBOARD_PINNED_CARD_KEYS,
  isHomeDashboardCardVisible,
  type HomeDashboardCardKey,
} from './home-dashboard-config';

const props = defineProps<{
  data: HomeDashboardApi.HomeDashboard | null;
  loading?: boolean;
}>();

const emit = defineEmits<{
  refresh: [calendar?: { year: number; month: number }];
}>();

const router = useRouter();
const accessStore = useAccessStore();
const gridRef = ref<HTMLElement | null>(null);
const sortableSaving = ref(false);

const calendarMonth = ref<Dayjs>(dayjs());
const selectedDate = ref<Dayjs>(dayjs());
const memoDraft = ref('');
const memoLoading = ref(false);
const memoSaving = ref(false);
const memoVisible = ref(false);

const accessCodes = computed(() => accessStore.accessCodes ?? []);

const pinnedVisibleCardKeys = computed(() => [...HOME_DASHBOARD_PINNED_CARD_KEYS]);

const sortableVisibleCardKeys = computed(() => {
  const order = props.data?.cardOrder ?? [];
  return order.filter(
    (key) =>
      !HOME_DASHBOARD_PINNED_CARD_KEY_SET.has(key as HomeDashboardCardKey) &&
      isHomeDashboardCardVisible(key as HomeDashboardCardKey, accessCodes.value),
  );
});

const localSortableCardKeys = ref<string[]>([]);

watch(
  sortableVisibleCardKeys,
  (keys) => {
    localSortableCardKeys.value = [...keys];
  },
  { immediate: true },
);

const displayCardKeys = computed(() => [
  ...pinnedVisibleCardKeys.value,
  ...localSortableCardKeys.value,
]);

const workflowPendingCount = computed(
  () => props.data?.workflowPendingTaskCount ?? 0,
);
const unreadNotificationCount = computed(
  () => props.data?.unreadNotificationCount ?? 0,
);

const weekdayLabels = ['日', '一', '二', '三', '四', '五', '六'];

const calendarWeeks = computed(() => {
  const start = calendarMonth.value.startOf('month').startOf('week');
  const end = calendarMonth.value.endOf('month').endOf('week');
  const weeks: Dayjs[][] = [];
  let cursor = start;
  while (cursor.isBefore(end) || cursor.isSame(end, 'day')) {
    const week: Dayjs[] = [];
    for (let i = 0; i < 7; i += 1) {
      week.push(cursor);
      cursor = cursor.add(1, 'day');
    }
    weeks.push(week);
  }
  return weeks;
});

const memoDaySet = computed(() => {
  const set = new Set<string>();
  for (const d of props.data?.calendar.memoDays ?? []) {
    if (d.hasContent) {
      set.add(d.date);
    }
  }
  return set;
});

const birthdayMd = computed(() => {
  const raw = props.data?.calendar.birthdayMonthDay;
  if (!raw) {
    return null;
  }
  const d = dayjs(raw);
  return d.isValid() ? { month: d.month() + 1, day: d.date() } : null;
});

function isBirthdayCell(cell: Dayjs) {
  if (!birthdayMd.value) {
    return false;
  }
  return cell.month() + 1 === birthdayMd.value.month && cell.date() === birthdayMd.value.day;
}

function isTodayCell(cell: Dayjs) {
  const today = props.data?.calendar.today;
  return today ? cell.format('YYYY-MM-DD') === today : cell.isSame(dayjs(), 'day');
}

function isMemoEditableDate(cell: Dayjs) {
  const today = props.data?.calendar.today ?? dayjs().format('YYYY-MM-DD');
  return cell.format('YYYY-MM-DD') >= today;
}

function isCurrentMonth(cell: Dayjs) {
  return cell.month() === calendarMonth.value.month();
}

function navigateCard(key: HomeDashboardCardKey) {
  const route = HOME_DASHBOARD_CARD_META_MAP[key]?.route;
  if (route) {
    router.push(route);
  }
}

async function persistCardOrder(keys: string[]) {
  sortableSaving.value = true;
  const previous = [...localSortableCardKeys.value];
  localSortableCardKeys.value = keys;
  try {
    await saveHomeDashboardLayout(keys);
    emit('refresh');
  } catch {
    localSortableCardKeys.value = previous;
    message.error($t('page.dashboard.home.layoutSaveFailed'));
  } finally {
    sortableSaving.value = false;
  }
}

onMounted(async () => {
  await nextTick();
  if (!gridRef.value) {
    return;
  }
  const { initializeSortable } = useSortable(gridRef.value, {
    animation: 200,
    delay: 0,
    handle: '.home-dashboard-card__drag-handle',
    draggable: '.home-dashboard-card-wrap--sortable',
    grid: true,
    onEnd: async (evt: any) => {
      const container = gridRef.value;
      const item = evt.item as HTMLElement | undefined;
      if (!container || !item) {
        return;
      }
      const sortableEls = [
        ...container.querySelectorAll<HTMLElement>('.home-dashboard-card-wrap--sortable'),
      ];
      const newIndex = sortableEls.indexOf(item);
      const movedKey = item.dataset.cardKey;
      if (!movedKey || newIndex < 0) {
        return;
      }
      const keys = [...localSortableCardKeys.value];
      const oldIndex = keys.indexOf(movedKey);
      if (oldIndex < 0 || oldIndex === newIndex) {
        return;
      }
      keys.splice(oldIndex, 1);
      keys.splice(newIndex, 0, movedKey);
      await persistCardOrder(keys);
    },
  } as any);
  await initializeSortable();
});

watch(
  () => props.data?.calendar,
  (cal) => {
    if (cal?.year && cal?.month) {
      calendarMonth.value = dayjs(`${cal.year}-${String(cal.month).padStart(2, '0')}-01`);
    }
  },
  { immediate: true },
);

function changeCalendarMonth(delta: number) {
  memoVisible.value = false;
  calendarMonth.value = calendarMonth.value.add(delta, 'month');
  emit('refresh', {
    year: calendarMonth.value.year(),
    month: calendarMonth.value.month() + 1,
  });
}

async function selectCalendarDate(cell: Dayjs) {
  if (!isMemoEditableDate(cell)) {
    message.warning($t('page.dashboard.home.memoPastDateNotAllowed'));
    return;
  }
  selectedDate.value = cell;
  memoVisible.value = true;
  memoLoading.value = true;
  try {
    const memo = await getCalendarMemo(cell.format('YYYY-MM-DD'));
    memoDraft.value = memo?.content ?? '';
  } finally {
    memoLoading.value = false;
  }
}

async function confirmMemo() {
  memoSaving.value = true;
  try {
    await saveCalendarMemo(selectedDate.value.format('YYYY-MM-DD'), memoDraft.value);
    emit('refresh');
  } finally {
    memoSaving.value = false;
    memoVisible.value = false;
  }
}
</script>

<template>
  <Spin :spinning="loading || sortableSaving">
    <div class="home-dashboard-summary">
      <div
        class="home-dashboard-card home-dashboard-card--workflow home-dashboard-card--clickable"
        role="button"
        tabindex="0"
        @click="navigateCard('process')"
        @keydown.enter="navigateCard('process')"
      >
        <div class="home-dashboard-card__body">
          <div class="home-dashboard-card__main">
            {{ $t('page.dashboard.home.workflowPendingTitle', { count: workflowPendingCount }) }}
          </div>
          <div class="home-dashboard-card__sub">
            {{ $t('page.dashboard.home.workflowPendingSubtitle') }}
          </div>
        </div>
        <IconifyIcon class="home-dashboard-card__watermark" icon="mdi:clipboard-text-clock" />
      </div>

      <div class="home-dashboard-card home-dashboard-card--notify">
        <div class="home-dashboard-card__body">
          <div class="home-dashboard-card__main">
            {{ $t('page.dashboard.home.unreadNotificationsTitle', { count: unreadNotificationCount }) }}
          </div>
          <div class="home-dashboard-card__sub">
            {{ $t('page.dashboard.home.unreadNotificationsSubtitle') }}
          </div>
        </div>
        <IconifyIcon class="home-dashboard-card__watermark" icon="mdi:bell-outline" />
      </div>
    </div>

    <div ref="gridRef" class="home-dashboard-grid">
      <div
        v-for="cardKey in displayCardKeys"
        :key="cardKey"
        :data-card-key="cardKey"
        class="home-dashboard-card-wrap"
        :class="{
          'home-dashboard-card-wrap--pinned': HOME_DASHBOARD_PINNED_CARD_KEY_SET.has(
            cardKey as HomeDashboardCardKey,
          ),
          'home-dashboard-card-wrap--sortable': !HOME_DASHBOARD_PINNED_CARD_KEY_SET.has(
            cardKey as HomeDashboardCardKey,
          ),
        }"
      >
        <div v-if="cardKey === 'process'" class="home-dashboard-panel">
          <div class="home-dashboard-panel__head">
            <span class="home-dashboard-panel__title">
              {{ $t(HOME_DASHBOARD_CARD_META_MAP.process.titleKey) }}
            </span>
            <button class="home-dashboard-panel__nav" type="button" @click="navigateCard('process')">
              <IconifyIcon icon="mdi:menu" />
            </button>
            <span class="home-dashboard-card__drag-handle" title="拖动排序">
              <IconifyIcon icon="mdi:drag" />
            </span>
          </div>
          <div class="home-dashboard-panel__hint">
            {{ $t('page.dashboard.home.workflowPendingTitle', { count: workflowPendingCount }) }}
          </div>
        </div>

        <div v-else-if="cardKey === 'calendar'" class="home-dashboard-panel">
          <div class="home-dashboard-panel__head">
            <span class="home-dashboard-panel__title">
              <IconifyIcon class="mr-1" icon="mdi:calendar-month-outline" />
              {{ $t('page.dashboard.home.cards.calendar') }}
            </span>
            <span class="home-dashboard-card__drag-handle" title="拖动排序">
              <IconifyIcon icon="mdi:drag" />
            </span>
          </div>
          <div class="home-dashboard-calendar">
            <div class="home-dashboard-calendar__nav">
              <button type="button" @click="changeCalendarMonth(-1)">
                <IconifyIcon icon="mdi:chevron-left" />
              </button>
              <span>{{ calendarMonth.format('YYYY年M月') }}</span>
              <button type="button" @click="changeCalendarMonth(1)">
                <IconifyIcon icon="mdi:chevron-right" />
              </button>
            </div>
            <div class="home-dashboard-calendar__body">
              <div class="home-dashboard-calendar__weekdays">
                <span v-for="w in weekdayLabels" :key="w">{{ w }}</span>
              </div>
              <div class="home-dashboard-calendar__grid">
                <template v-for="(week, wi) in calendarWeeks" :key="wi">
                  <button
                    v-for="cell in week"
                    :key="cell.format('YYYY-MM-DD')"
                    type="button"
                    class="home-dashboard-calendar__day"
                    :class="{
                      'is-other-month': !isCurrentMonth(cell),
                      'is-today': isTodayCell(cell),
                      'is-past': !isMemoEditableDate(cell),
                      'is-selected': cell.isSame(selectedDate, 'day'),
                      'has-memo': memoDaySet.has(cell.format('YYYY-MM-DD')),
                    }"
                    @click="selectCalendarDate(cell)"
                  >
                    <span class="home-dashboard-calendar__day-num">{{ cell.date() }}</span>
                    <IconifyIcon
                      v-if="isBirthdayCell(cell)"
                      class="home-dashboard-calendar__birthday"
                      icon="mdi:cake-variant"
                    />
                  </button>
                </template>
              </div>
              <div v-if="memoVisible" class="home-dashboard-memo-overlay">
                <div class="home-dashboard-memo">
                  <div class="home-dashboard-memo__title">
                    {{ $t('page.dashboard.home.memoTitle', { date: selectedDate.format('M月D日') }) }}
                  </div>
                  <Spin :spinning="memoLoading" class="home-dashboard-memo__spin">
                    <Input.TextArea
                      v-model:value="memoDraft"
                      :rows="6"
                      class="home-dashboard-memo__input"
                      :placeholder="$t('page.dashboard.home.memoPlaceholder')"
                    />
                  </Spin>
                  <div class="home-dashboard-memo__actions">
                    <Button type="primary" size="small" :loading="memoSaving" @click="confirmMemo">
                      {{ $t('common.confirm') }}
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Spin>
</template>

<style scoped>
.home-dashboard-summary {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 16px;
}

.home-dashboard-grid {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: 16px;
}

.home-dashboard-card-wrap {
  min-width: 0;
}

.home-dashboard-card-wrap--pinned,
.home-dashboard-card-wrap--sortable {
  grid-column: span 12;
}

@media (min-width: 1200px) {
  .home-dashboard-card-wrap--sortable {
    grid-column: span 6;
  }
}

.home-dashboard-panel {
  position: relative;
  padding: 16px;
  color: hsl(var(--foreground));
  background: hsl(var(--card));
  border: 1px solid hsl(var(--border));
  border-radius: 8px;
  box-shadow: 0 1px 4px hsl(var(--foreground) / 6%);
}

.home-dashboard-panel__head {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 12px;
}

.home-dashboard-panel__title {
  display: flex;
  flex: 1;
  align-items: center;
  font-size: 15px;
  font-weight: 600;
}

.home-dashboard-panel__nav {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  color: #fff;
  cursor: pointer;
  background: #52c41a;
  border: none;
  border-radius: 4px;
}

.home-dashboard-panel__hint {
  font-size: 14px;
  color: hsl(var(--muted-foreground));
}

.home-dashboard-card__drag-handle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  color: hsl(var(--muted-foreground));
  cursor: grab;
}

.home-dashboard-calendar__nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
  font-weight: 600;
}

.home-dashboard-calendar__nav button {
  cursor: pointer;
  background: transparent;
  border: none;
}

.home-dashboard-calendar__weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  margin-bottom: 4px;
  font-size: 12px;
  text-align: center;
}

.home-dashboard-calendar__body {
  position: relative;
}

.home-dashboard-calendar__grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 2px;
}

.home-dashboard-calendar__day {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 36px;
  cursor: pointer;
  background: transparent;
  border: none;
  border-radius: 4px;
}

.home-dashboard-calendar__day.is-past {
  cursor: not-allowed;
  opacity: 0.45;
}

.home-dashboard-calendar__day.is-today .home-dashboard-calendar__day-num {
  font-weight: 700;
  color: #52c41a;
}

.home-dashboard-calendar__day.has-memo::after {
  position: absolute;
  bottom: 2px;
  width: 4px;
  height: 4px;
  content: '';
  background: #1890ff;
  border-radius: 50%;
}

.home-dashboard-memo-overlay {
  position: absolute;
  inset: 0;
  z-index: 2;
  padding: 2px;
  background: hsl(var(--card));
}

.home-dashboard-memo {
  display: flex;
  flex-direction: column;
  height: 100%;
  padding: 12px;
  border: 1px solid hsl(var(--primary) / 0.45);
  border-radius: 4px;
}

.home-dashboard-memo__title {
  margin-bottom: 8px;
  font-weight: 600;
}

.home-dashboard-memo__actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 8px;
}

.home-dashboard-card {
  position: relative;
  min-height: 120px;
  overflow: hidden;
  border-radius: 4px;
  color: #fff;
}

.home-dashboard-card--clickable {
  cursor: pointer;
}

.home-dashboard-card--workflow {
  background: #1890ff;
}

.home-dashboard-card--notify {
  background: #faad14;
}

.home-dashboard-card__body {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 100%;
  min-height: 120px;
  padding: 20px 16px 16px;
}

.home-dashboard-card__main {
  font-size: clamp(16px, 1.6vw, 24px);
  font-weight: 600;
}

.home-dashboard-card__sub {
  margin-top: auto;
  padding-top: 10px;
  font-size: 13px;
}

.home-dashboard-card__watermark {
  position: absolute;
  right: 12px;
  bottom: 8px;
  width: 72px;
  height: 72px;
  opacity: 0.22;
}
</style>
