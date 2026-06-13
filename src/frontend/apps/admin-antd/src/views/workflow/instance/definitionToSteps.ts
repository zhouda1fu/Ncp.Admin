import { workflowSchemaToDesignerTree } from '../designer/utils/workflow-schema';

/**
 * 将流程定义 JSON 展平为步骤列表，供 Steps 组件展示。
 * type: 0=发起人, 1=审批, 2=抄送, 3=条件分支项（壳）, 4=条件路由
 * 不展示路由/条件壳；条件路由在无变量上下文时仅展开第一条分支（实例详情应使用后端 progressSteps）。
 */

export interface StepItem {
  title: string;
  /** 设计器 nodeKey，用于与实例 currentNodeKey 对齐 */
  nodeKey?: string;
}

interface NodeConfig {
  nodeKey?: string;
  nodeName?: string;
  type?: number;
  childNode?: NodeConfig | null;
  conditionNodes?: NodeConfig[] | null;
}

/** 设计器节点 type=2 为抄送 */
const DESIGNER_NODE_TYPE_CARBON_COPY = 2;

/** 任务 type=2 为抄送（与后端 WorkflowTaskType.CarbonCopy 一致） */
const WORKFLOW_TASK_TYPE_CARBON_COPY = 2;

function collectSteps(node: NodeConfig | null | undefined, steps: StepItem[]): void {
  if (!node) return;

  const type = node.type ?? 0;

  if (type === 4) {
    const conditions = node.conditionNodes;
    if (conditions?.length) {
      collectSteps(conditions[0], steps);
    } else {
      collectSteps(node.childNode, steps);
    }
    return;
  }

  if (type === 3) {
    collectSteps(node.childNode, steps);
    return;
  }

  if (type === 1 || type === 2) {
    const raw = node.nodeName?.trim();
    const title =
      raw ||
      (type === 1 ? '审批' : '抄送');
    steps.push({
      title,
      nodeKey: node.nodeKey?.trim() || undefined,
    });
    collectSteps(node.childNode, steps);
    return;
  }

  if (type === 0) {
    const raw = node.nodeName?.trim();
    if (raw) {
      steps.push({
        title: raw,
        nodeKey: node.nodeKey?.trim() || undefined,
      });
    }
    collectSteps(node.childNode, steps);
    return;
  }

  collectSteps(node.childNode, steps);
}

/**
 * 从流程定义 JSON 解析步骤（fallback；实例页优先使用接口返回的 progressSteps）
 */
export function designerSchemaJsonToStepList(designerSchemaJson: string | null | undefined): StepItem[] {
  if (!designerSchemaJson?.trim()) return [];
  try {
    const parsed = JSON.parse(designerSchemaJson);
    const root = workflowSchemaToDesignerTree(parsed) as NodeConfig | null;
    if (!root) return [];
    const steps: StepItem[] = [];
    collectSteps(root, steps);
    return steps;
  } catch {
    return [];
  }
}

/**
 * 根据当前节点 key（优先）或名称匹配步骤索引（用于 Steps 的 current）
 */
export function findCurrentStepIndex(
  stepList: StepItem[],
  currentNodeName: string | null | undefined,
  currentNodeKey?: string | null,
): number {
  if (stepList.length === 0) return 0;
  const key = currentNodeKey?.trim();
  if (key) {
    let byKey = -1;
    stepList.forEach((s, i) => {
      if (s.nodeKey === key) byKey = i;
    });
    if (byKey >= 0) return byKey;
  }
  if (!currentNodeName?.trim()) return 0;
  const name = currentNodeName.trim();
  let index = -1;
  stepList.forEach((s, i) => {
    if (s.title === name) index = i;
  });
  if (index >= 0) return index;
  return 0;
}

function walkDesignerNodesForCarbonCopyKeys(
  node: NodeConfig | null | undefined,
  keys: Set<string>,
): void {
  if (!node) return;
  const type = node.type ?? 0;
  if (type === 4) {
    node.conditionNodes?.forEach((c) => walkDesignerNodesForCarbonCopyKeys(c, keys));
    walkDesignerNodesForCarbonCopyKeys(node.childNode, keys);
    return;
  }
  if (type === 3) {
    walkDesignerNodesForCarbonCopyKeys(node.childNode, keys);
    return;
  }
  if (type === DESIGNER_NODE_TYPE_CARBON_COPY) {
    const key = node.nodeKey?.trim();
    if (key) keys.add(key);
    walkDesignerNodesForCarbonCopyKeys(node.childNode, keys);
    return;
  }
  walkDesignerNodesForCarbonCopyKeys(node.childNode, keys);
}

/** 从流程定义 JSON 收集抄送节点 nodeKey */
export function collectCarbonCopyNodeKeysFromDefinition(
  definitionJson: string | null | undefined,
): Set<string> {
  const keys = new Set<string>();
  if (!definitionJson?.trim()) return keys;
  try {
    const root = JSON.parse(definitionJson) as NodeConfig | null;
    walkDesignerNodesForCarbonCopyKeys(root, keys);
  } catch {
    /* ignore */
  }
  return keys;
}

/** 从实例任务列表收集抄送节点 nodeKey */
export function collectCarbonCopyNodeKeysFromTasks(
  tasks: { nodeKey?: string; taskType?: number }[] | undefined,
): Set<string> {
  const keys = new Set<string>();
  for (const t of tasks ?? []) {
    if (t.taskType === WORKFLOW_TASK_TYPE_CARBON_COPY) {
      const key = t.nodeKey?.trim();
      if (key) keys.add(key);
    }
  }
  return keys;
}

/** 进度条不展示抄送节点 */
export function filterOutCarbonCopySteps(steps: StepItem[], ccKeys: Set<string>): StepItem[] {
  return steps.filter((s) => {
    const key = s.nodeKey?.trim();
    return !(key && ccKeys.has(key));
  });
}

/**
 * 当前节点落在抄送节点上时，映射到进度条中上一个非抄送步骤的索引。
 */
export function resolveVisibleCurrentStepIndex(
  visibleSteps: StepItem[],
  allSteps: StepItem[],
  currentNodeName: string | null | undefined,
  currentNodeKey: string | null | undefined,
  ccKeys: Set<string>,
): number {
  const key = currentNodeKey?.trim();
  if (key && ccKeys.has(key)) {
    const allIdx = allSteps.findIndex((s) => s.nodeKey === key);
    for (let i = allIdx - 1; i >= 0; i--) {
      const prev = allSteps[i];
      if (!prev) continue;
      const prevKey = prev.nodeKey?.trim();
      if (prevKey && ccKeys.has(prevKey)) continue;
      const vi = visibleSteps.findIndex(
        (s) => (prevKey && s.nodeKey === prevKey) || s.title === prev.title,
      );
      if (vi >= 0) return vi;
    }
    return Math.max(0, visibleSteps.length - 1);
  }
  return findCurrentStepIndex(visibleSteps, currentNodeName, currentNodeKey);
}

/**
 * 汇总抄送节点 nodeKey（设计器定义、实例任务、步骤标题兜底）。
 * 用于进度条过滤抄送节点。
 */
export function resolveWorkflowProgressCarbonCopyKeys(input: {
  designerSchemaJson?: string | null;
  tasks?: { nodeKey?: string; taskType?: number }[];
  rawSteps?: StepItem[];
}): Set<string> {
  const keys = new Set<string>();
  for (const k of collectCarbonCopyNodeKeysFromDefinition(input.designerSchemaJson)) {
    keys.add(k);
  }
  for (const k of collectCarbonCopyNodeKeysFromTasks(input.tasks)) {
    keys.add(k);
  }
  for (const s of input.rawSteps ?? []) {
    const title = s.title.trim();
    if (title.includes('抄送')) {
      const key = s.nodeKey?.trim();
      if (key) keys.add(key);
    }
  }
  return keys;
}

export function mapProgressStepsToStepItems(
  steps: { title: string; nodeKey?: string | null }[],
): StepItem[] {
  return steps.map((s) => ({
    title: s.title,
    nodeKey: s.nodeKey?.trim() || undefined,
  }));
}

/** 根据是否过滤抄送，生成可见步骤与抄送 key 集合 */
export function buildWorkflowProgressStepLists(input: {
  rawSteps: StepItem[];
  filterCarbonCopy: boolean;
  designerSchemaJson?: string | null;
  tasks?: { nodeKey?: string; taskType?: number }[];
}): { visibleSteps: StepItem[]; carbonCopyNodeKeys: Set<string> } {
  if (!input.filterCarbonCopy) {
    return { visibleSteps: [...input.rawSteps], carbonCopyNodeKeys: new Set() };
  }
  const carbonCopyNodeKeys = resolveWorkflowProgressCarbonCopyKeys({
    designerSchemaJson: input.designerSchemaJson,
    tasks: input.tasks,
    rawSteps: input.rawSteps,
  });
  return {
    visibleSteps: filterOutCarbonCopySteps(input.rawSteps, carbonCopyNodeKeys),
    carbonCopyNodeKeys,
  };
}
