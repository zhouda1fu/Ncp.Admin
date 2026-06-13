import { idempotencyRequestConfig } from '#/api/idempotency';
import { requestClient } from '#/api/request';

export namespace BackgroundJobApi {
  export interface RecurringJob {
    id: string;
    displayName: string;
    description: string;
    cron: string;
    queue: string;
    timeZoneId: string;
    lastExecution?: string | null;
    nextExecution?: string | null;
    lastJobId?: string | null;
    lastJobState?: string | null;
    error?: string | null;
    isKnown: boolean;
    settingsPath?: string | null;
  }

  export interface KnownRecurringJob {
    id: string;
    displayName: string;
    description: string;
    configuredCron: string;
    settingsPath?: string | null;
  }
}

export function getRecurringJobs() {
  return requestClient.get<BackgroundJobApi.RecurringJob[]>('/background-jobs/recurring');
}

export function getKnownRecurringJobs() {
  return requestClient.get<BackgroundJobApi.KnownRecurringJob[]>('/background-jobs/recurring/known');
}

export function triggerRecurringJob(id: string) {
  return requestClient.post<boolean>(
    `/background-jobs/recurring/${encodeURIComponent(id)}/trigger`,
    {},
    idempotencyRequestConfig(),
  );
}

export function removeRecurringJob(id: string) {
  return requestClient.delete<boolean>(
    `/background-jobs/recurring/${encodeURIComponent(id)}`,
    idempotencyRequestConfig(),
  );
}

export function upsertKnownRecurringJob(id: string, cron: string) {
  return requestClient.put<boolean>(
    `/background-jobs/recurring/${encodeURIComponent(id)}`,
    { cron, id },
    idempotencyRequestConfig(),
  );
}
