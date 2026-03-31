/**
 * 将流程定义 definitionJson（设计器树形结构）展平为步骤列表，供 Steps 组件展示。
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
export function definitionJsonToStepList(definitionJson: string | null | undefined): StepItem[] {
  if (!definitionJson?.trim()) return [];
  try {
    const root = JSON.parse(definitionJson) as NodeConfig | null;
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
