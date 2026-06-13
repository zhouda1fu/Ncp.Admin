/** 与后端 WorkflowInstanceId.Unassigned / Guid.Empty 一致 */
export const EMPTY_WORKFLOW_INSTANCE_ID =
  '00000000-0000-0000-0000-000000000000';

/** 是否为已绑定的流程实例 ID（排除空 GUID） */
export function isAssignedWorkflowInstanceId(value: unknown): boolean {
  const s = String(value ?? '').trim();
  return !!s && s.toLowerCase() !== EMPTY_WORKFLOW_INSTANCE_ID;
}
