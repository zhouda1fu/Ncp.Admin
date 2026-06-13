export type WorkflowDesignerNodeType =
  | 'approval'
  | 'businessExtension'
  | 'carbonCopy'
  | 'conditionRoute'
  | 'end'
  | 'start';

export interface WorkflowDesignerOption {
  id: string;
  name: string;
}

export interface WorkflowAssigneeRule {
  ruleId: string;
  /** 审批人来源，保持为业务语义而不是旧设计器的数字 setType。 */
  source:
    | 'businessVariable'
    | 'deptResponsibleUser'
    | 'deptResponsibleUserChain'
    | 'initiator'
    | 'member'
    | 'orderContractSigningCompanyResponsibleUser'
    | 'role';
  users?: WorkflowDesignerOption[];
  roles?: WorkflowDesignerOption[];
  depts?: WorkflowDesignerOption[];
  level?: number;
  /** 部门负责人链规则中需要排除的指定成员。 */
  excludeUsers?: WorkflowDesignerOption[];
  /** 部门负责人链规则中额外追加的指定成员。 */
  extraUsers?: WorkflowDesignerOption[];
  initiatorDeptScope?: {
    mode: 'all' | 'dataPermission' | 'specifiedDeptAndSub';
    depts?: WorkflowDesignerOption[];
  };
}

export interface WorkflowConditionBranch {
  branchId: string;
  name: string;
  priority: number;
  conditionGroups: any[][];
  firstNodeId?: string;
  isFallback?: boolean;
}

export interface WorkflowDesignerNode {
  nodeId: string;
  name: string;
  type: WorkflowDesignerNodeType;
  nextNodeId?: string;
  approvalMode?: 'all' | 'any' | 'sequential';
  assigneeRules?: WorkflowAssigneeRule[];
  copyRules?: WorkflowAssigneeRule[];
  emptyApproverPolicy?: {
    mode: 'autoPass' | 'specifiedMembers' | 'workflowAdmin';
    users?: WorkflowDesignerOption[];
  };
  selfApprovalPolicy?: {
    mode: 'allow' | 'autoSkip' | 'deptResponsibleUser' | 'directResponsibleUser';
  };
  branches?: WorkflowConditionBranch[];
  mergeNodeId?: string;
  extensions?: Record<string, any>;
}

export interface WorkflowDesignerSchema {
  allowAutoCompleteWithoutTasks?: boolean;
  nodes: WorkflowDesignerNode[];
  startNodeId: string;
}

/** 创建只有发起节点的默认流程结构。 */
export function createDefaultWorkflowSchema(nodeId: string): WorkflowDesignerSchema {
  return {
    startNodeId: nodeId,
    nodes: [
      {
        nodeId,
        name: '发起人',
        type: 'start',
      },
    ],
  };
}

function stableRuleId(prefix: string, nodeKey: string, index: number) {
  return `${prefix}_${nodeKey}_${index}`;
}

function toStringId(id: unknown): string {
  return id == null ? '' : String(id);
}

function toOption(item: any): WorkflowDesignerOption {
  return {
    id: toStringId(item?.id ?? item?.value ?? item?.userId),
    name: String(item?.name ?? item?.label ?? item?.displayName ?? ''),
  };
}

function approvalModeFromDesignerTree(mode: number | undefined) {
  if (mode === 2) return 'all';
  if (mode === 3) return 'any';
  return 'sequential';
}

function approvalModeToDesignerTree(mode: string | undefined) {
  if (mode === 'all') return 2;
  if (mode === 'any') return 3;
  return 1;
}

function emptyPolicyFromDesignerTree(policy: number | undefined) {
  if (policy === 2) return 'specifiedMembers';
  if (policy === 3) return 'workflowAdmin';
  return 'autoPass';
}

function emptyPolicyToDesignerTree(mode: string | undefined) {
  if (mode === 'specifiedMembers') return 2;
  if (mode === 'workflowAdmin') return 3;
  return 1;
}

function selfPolicyFromDesignerTree(policy: number | undefined) {
  if (policy === 2) return 'autoSkip';
  if (policy === 3) return 'directResponsibleUser';
  if (policy === 4) return 'deptResponsibleUser';
  return 'allow';
}

function selfPolicyToDesignerTree(mode: string | undefined) {
  if (mode === 'autoSkip') return 2;
  if (mode === 'directResponsibleUser') return 3;
  if (mode === 'deptResponsibleUser') return 4;
  return 1;
}

function initiatorDeptScopeFromDesignerTree(config: any): NonNullable<WorkflowAssigneeRule['initiatorDeptScope']> {
  const mode =
    config?.initiatorDeptScopeMode === 1
      ? 'all'
      : config?.initiatorDeptScopeMode === 2
        ? 'specifiedDeptAndSub'
        : 'dataPermission';
  return {
    mode,
    depts: (config?.initiatorDeptList ?? []).map(toOption),
  };
}

/** 将旧设计器节点上的审批/抄送配置转换为后端 schema 规则。 */
function assigneeRulesFromDesignerTree(
  configs: any[] | undefined,
  nodeKey: string,
  prefix: string,
): WorkflowAssigneeRule[] {
  return (configs ?? []).map((config, index) => {
    const setType = Number(config?.setType ?? 1);
    const base = {
      ruleId: String(config?.ruleId ?? stableRuleId(prefix, nodeKey, index)),
      level: Number(config?.examineLevel ?? 1) || 1,
    };
    if (setType === 2) return { ...base, source: 'deptResponsibleUser' as const };
    if (setType === 6) {
      // setType=6 是设计器树中的「部门负责人链」配置。
      return {
        ...base,
        source: 'deptResponsibleUserChain' as const,
        excludeUsers: (config?.excludeAssigneeList ?? []).map(toOption),
        extraUsers: (config?.extraAssigneeList ?? []).map(toOption),
      };
    }
    if (setType === 3) {
      return {
        ...base,
        source: 'role' as const,
        roles: (config?.nodeAssigneeList ?? []).map(toOption),
        initiatorDeptScope: initiatorDeptScopeFromDesignerTree(config),
      };
    }
    if (setType === 5) return { ...base, source: 'initiator' as const };
    if (setType === 7) {
      return {
        ...base,
        source: 'orderContractSigningCompanyResponsibleUser' as const,
        initiatorDeptScope: initiatorDeptScopeFromDesignerTree(config),
      };
    }
    return {
      ...base,
      source: 'member' as const,
      users: (config?.nodeAssigneeList ?? []).map(toOption),
    };
  });
}

/** 将后端 schema 规则还原为旧设计器节点能够直接渲染的配置。 */
function assigneeRulesToDesignerTree(rules: WorkflowAssigneeRule[] | undefined) {
  return (rules ?? []).map((rule) => {
    const setType =
      rule.source === 'deptResponsibleUser'
        ? 2
        : rule.source === 'deptResponsibleUserChain'
          ? 6
        : rule.source === 'orderContractSigningCompanyResponsibleUser'
          ? 7
        : rule.source === 'role'
          ? 3
          : rule.source === 'initiator'
            ? 5
            : 1;
    return {
      setType,
      examineLevel: rule.level ?? 1,
      nodeAssigneeList:
        rule.source === 'role'
          ? (rule.roles ?? [])
          : rule.source === 'member'
            ? (rule.users ?? [])
            : [],
      excludeAssigneeList: rule.excludeUsers ?? [],
      extraAssigneeList: rule.extraUsers ?? [],
      initiatorDeptScopeMode:
        rule.initiatorDeptScope?.mode === 'all'
          ? 1
          : rule.initiatorDeptScope?.mode === 'specifiedDeptAndSub'
            ? 2
            : 0,
      initiatorDeptList: rule.initiatorDeptScope?.depts ?? [],
    };
  });
}

function designerTreeTypeToSchemaType(type: number): WorkflowDesignerNodeType {
  if (type === 1) return 'approval';
  if (type === 2) return 'carbonCopy';
  if (type === 4) return 'conditionRoute';
  if (type === 99) return 'end';
  return 'start';
}

function buildExtensionsFromDesignerTree(node: any) {
  const extensions: Record<string, any> = {};
  const orderExtension: Record<string, any> = {};
  // 订单审批节点的业务扩展统一序列化到 extensions.order，后端发布校验会按分类兜底。
  if (node?.orderApplyTechnologyVisible) {
    orderExtension.applyTechnologyVisible = true;
  }
  if (node?.orderStatusOnEnter !== undefined && node?.orderStatusOnEnter !== null) {
    orderExtension.statusOnEnter = node.orderStatusOnEnter;
  }
  if (Array.isArray(node?.orderRequiredStatusesToOperate) && node.orderRequiredStatusesToOperate.length > 0) {
    orderExtension.requiredStatusesToOperate = node.orderRequiredStatusesToOperate;
  }
  if (Object.keys(orderExtension).length > 0) {
    extensions.order = orderExtension;
  }
  if (node?.officeTaskParticipantNode) {
    extensions.officeTask = {
      participantNode: true,
      receiverMode: node.officeTaskReceiverConfigMode,
      carbonCopyMode: node.officeTaskCarbonCopyConfigMode,
    };
  }
  if (node?.returnFieldMode === 'Required') {
    extensions.workflowReturn = {
      fieldMode: 'Required',
      fieldSetCode: node.returnFieldSetCode || undefined,
    };
  }
  return Object.keys(extensions).length ? extensions : undefined;
}

function applyExtensionsToDesignerTree(source: WorkflowDesignerNode, target: any) {
  const order = source.extensions?.order;
  if (order?.applyTechnologyVisible) target.orderApplyTechnologyVisible = true;
  // 回显历史流程定义时，没有配置的节点保持空值/空数组，避免误保存成限制规则。
  target.orderStatusOnEnter = order?.statusOnEnter;
  target.orderRequiredStatusesToOperate = Array.isArray(order?.requiredStatusesToOperate)
    ? order.requiredStatusesToOperate
    : [];
  const officeTask = source.extensions?.officeTask;
  if (officeTask?.participantNode) {
    target.officeTaskParticipantNode = true;
    target.officeTaskReceiverConfigMode = officeTask.receiverMode;
    target.officeTaskCarbonCopyConfigMode = officeTask.carbonCopyMode;
  }
  const workflowReturn = source.extensions?.workflowReturn;
  target.returnFieldMode = workflowReturn?.fieldMode === 'Required' ? 'Required' : 'Disabled';
  target.returnFieldSetCode = workflowReturn?.fieldSetCode;
}

/** 将画布组件使用的树结构展平为后端保存的流程 schema。 */
export function designerTreeToWorkflowSchema(root: any): WorkflowDesignerSchema {
  const nodes: WorkflowDesignerNode[] = [];
  const seen = new Set<string>();

  function visit(node: any, fallbackNextNodeId?: string): string | undefined {
    if (!node) return fallbackNextNodeId;
    const nodeId = toStringId(node.nodeKey);
    if (!nodeId) return fallbackNextNodeId;
    if (seen.has(nodeId)) return nodeId;
    seen.add(nodeId);

    const type = designerTreeTypeToSchemaType(Number(node.type ?? 0));
    // 普通节点没有子节点时，需要接回外层传入的后续节点；条件分支尾节点靠这里回到汇总后的流程。
    const nextNodeId = visit(node.childNode, fallbackNextNodeId);
    const item: WorkflowDesignerNode = {
      nodeId,
      name: String(node.nodeName ?? ''),
      type,
      nextNodeId,
      extensions: buildExtensionsFromDesignerTree(node),
    };
    if (type === 'approval') {
      item.approvalMode = approvalModeFromDesignerTree(node.examineMode);
      item.assigneeRules = assigneeRulesFromDesignerTree(node.approverConfigs, nodeId, 'approver');
      item.emptyApproverPolicy = {
        mode: emptyPolicyFromDesignerTree(node.emptyApproverPolicy),
        users: (node.emptyApproverAssigneeList ?? []).map(toOption),
      };
      item.selfApprovalPolicy = { mode: selfPolicyFromDesignerTree(node.selfApprovalPolicy) };
    }
    if (type === 'carbonCopy') {
      item.copyRules = assigneeRulesFromDesignerTree(
        node.copyConfigs ??
          (node.setType
            ? [
                {
                  setType: node.setType,
                  examineLevel: node.examineLevel,
                  nodeAssigneeList: node.nodeAssigneeList,
                },
              ]
            : []),
        nodeId,
        'copy',
      );
    }
    if (type === 'conditionRoute') {
      item.mergeNodeId = nextNodeId;
      item.branches = (node.conditionNodes ?? []).map((branch: any, index: number) => ({
        branchId: toStringId(branch.nodeKey),
        name: String(branch.nodeName ?? ''),
        priority: Number(branch.priorityLevel ?? index + 1),
        conditionGroups: branch.conditionList ?? [],
        // 分支内部流程走完后必须回到 mergeNodeId，否则运行进度会在分支尾节点提前结束。
        firstNodeId: branch.childNode ? visit(branch.childNode, item.mergeNodeId) : undefined,
        isFallback: !branch.conditionList || branch.conditionList.length === 0,
      }));
    }
    nodes.push(item);
    return nodeId;
  }

  const startNodeId = visit(root) ?? '';
  return {
    startNodeId,
    nodes,
  };
}

/** 将后端流程 schema 还原为画布组件仍在使用的树结构。 */
export function workflowSchemaToDesignerTree(schema: WorkflowDesignerSchema | any): any {
  if (!schema || !Array.isArray(schema.nodes)) {
    return null;
  }
  const map = new Map<string, WorkflowDesignerNode>(
    schema.nodes.map((node: WorkflowDesignerNode) => [node.nodeId, node]),
  );
  const building = new Set<string>();

  function build(nodeId?: string, stopNodeId?: string): any {
    if (!nodeId) return null;
    if (stopNodeId && nodeId === stopNodeId) return null;
    const node = map.get(nodeId);
    if (!node) return null;
    if (building.has(nodeId)) return null;
    building.add(nodeId);
    const designerNode: any = {
      nodeName: node.name,
      nodeKey: node.nodeId,
      type:
        node.type === 'approval'
          ? 1
          : node.type === 'carbonCopy'
            ? 2
            : node.type === 'conditionRoute'
              ? 4
              : node.type === 'end'
                ? 99
                : 0,
      childNode:
        node.type === 'conditionRoute'
          ? build(node.mergeNodeId ?? node.nextNodeId, stopNodeId)
          : build(node.nextNodeId, stopNodeId),
    };
    if (node.type === 'approval') {
      designerNode.examineMode = approvalModeToDesignerTree(node.approvalMode);
      designerNode.approverConfigs = assigneeRulesToDesignerTree(node.assigneeRules);
      designerNode.emptyApproverPolicy = emptyPolicyToDesignerTree(node.emptyApproverPolicy?.mode);
      designerNode.emptyApproverAssigneeList = node.emptyApproverPolicy?.users ?? [];
      designerNode.selfApprovalPolicy = selfPolicyToDesignerTree(node.selfApprovalPolicy?.mode);
    }
    if (node.type === 'carbonCopy') {
      designerNode.copyConfigs = assigneeRulesToDesignerTree(node.copyRules);
      const first = designerNode.copyConfigs[0];
      if (first) {
        designerNode.setType = first.setType;
        designerNode.nodeAssigneeList = first.nodeAssigneeList;
        designerNode.examineLevel = first.examineLevel;
      }
    }
    if (node.type === 'conditionRoute') {
      designerNode.conditionNodes = (node.branches ?? []).map((branch) => ({
        nodeName: branch.name,
        nodeKey: branch.branchId,
        type: 3,
        priorityLevel: branch.priority,
        conditionMode: 1,
        conditionList: branch.isFallback ? [] : branch.conditionGroups,
        // 分支运行图会接回 mergeNodeId，画布回显时在汇总节点前停止，避免重复渲染后续流程。
        childNode: build(branch.firstNodeId, node.mergeNodeId ?? node.nextNodeId),
      }));
    }
    applyExtensionsToDesignerTree(node, designerNode);
    building.delete(nodeId);
    return designerNode;
  }

  return build(schema.startNodeId);
}

/** 解析流程定义快照，同时兼容历史树结构和新的 schema 结构。 */
export function parseWorkflowDefinitionSchema(
  raw: string | undefined,
): WorkflowDesignerSchema | null {
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw);
    if (!parsed) return null;
    if (Array.isArray(parsed.nodes) && parsed.startNodeId) {
      return parsed as WorkflowDesignerSchema;
    }
    if (parsed.nodeKey) {
      return designerTreeToWorkflowSchema(parsed);
    }
    return null;
  } catch {
    return null;
  }
}

/** 序列化当前设计器 schema，作为流程定义版本的运行图来源。 */
export function serializeWorkflowSchema(schema: WorkflowDesignerSchema): string {
  return JSON.stringify(schema);
}
