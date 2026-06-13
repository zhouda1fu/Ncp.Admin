import { describe, expect, it } from 'vitest';

import {
  createDefaultWorkflowSchema,
  designerTreeToWorkflowSchema,
  parseWorkflowDefinitionSchema,
  serializeWorkflowSchema,
  workflowSchemaToDesignerTree,
} from './workflow-schema';

describe('workflow-schema', () => {
  it('creates a minimal start schema', () => {
    const schema = createDefaultWorkflowSchema('start_1');

    expect(schema).toEqual({
      startNodeId: 'start_1',
      nodes: [{ nodeId: 'start_1', name: '发起人', type: 'start' }],
    });
  });

  it('round-trips approval role scope and business extensions', () => {
    const designerTree = {
      nodeName: '发起人',
      nodeKey: 'start_1',
      type: 0,
      childNode: {
        nodeName: '部门负责人审批',
        nodeKey: 'approval_1',
        type: 1,
        examineMode: 2,
        approverConfigs: [
          {
            setType: 3,
            examineLevel: 1,
            nodeAssigneeList: [{ id: 'role_1', name: '财务' }],
            initiatorDeptScopeMode: 2,
            initiatorDeptList: [{ id: 'dept_1', name: '销售部' }],
          },
        ],
        emptyApproverPolicy: 2,
        emptyApproverAssigneeList: [{ id: 'user_1', name: '张三' }],
        selfApprovalPolicy: 4,
        orderApplyTechnologyVisible: true,
        officeTaskParticipantNode: true,
        officeTaskReceiverConfigMode: 'preset',
        officeTaskCarbonCopyConfigMode: 'manager',
        childNode: null,
      },
    };

    const schema = designerTreeToWorkflowSchema(designerTree);
    const approval = schema.nodes.find((node) => node.nodeId === 'approval_1');

    expect(approval?.approvalMode).toBe('all');
    expect(approval?.assigneeRules?.[0]).toMatchObject({
      source: 'role',
      roles: [{ id: 'role_1', name: '财务' }],
      initiatorDeptScope: {
        mode: 'specifiedDeptAndSub',
        depts: [{ id: 'dept_1', name: '销售部' }],
      },
    });
    expect(approval?.emptyApproverPolicy).toEqual({
      mode: 'specifiedMembers',
      users: [{ id: 'user_1', name: '张三' }],
    });
    expect(approval?.selfApprovalPolicy).toEqual({ mode: 'deptResponsibleUser' });
    expect(approval?.extensions).toEqual({
      officeTask: {
        carbonCopyMode: 'manager',
        participantNode: true,
        receiverMode: 'preset',
      },
      order: { applyTechnologyVisible: true },
    });

    expect(workflowSchemaToDesignerTree(schema).childNode).toMatchObject({
      nodeKey: 'approval_1',
      examineMode: 2,
      approverConfigs: [
        {
          setType: 3,
          initiatorDeptScopeMode: 2,
          initiatorDeptList: [{ id: 'dept_1', name: '销售部' }],
          nodeAssigneeList: [{ id: 'role_1', name: '财务' }],
        },
      ],
      emptyApproverPolicy: 2,
      selfApprovalPolicy: 4,
      orderApplyTechnologyVisible: true,
      officeTaskParticipantNode: true,
      officeTaskReceiverConfigMode: 'preset',
      officeTaskCarbonCopyConfigMode: 'manager',
    });
  });

  it('round-trips manager chain exclusions and extra users', () => {
    const designerTree = {
      nodeName: '发起人',
      nodeKey: 'start_1',
      type: 0,
      childNode: {
        nodeName: '部门负责人链审批',
        nodeKey: 'approval_1',
        type: 1,
        approverConfigs: [
          {
            setType: 6,
            excludeAssigneeList: [{ id: '3', name: 'C' }],
            extraAssigneeList: [{ id: '9', name: '额外审批人' }],
          },
        ],
        childNode: null,
      },
    };

    const schema = designerTreeToWorkflowSchema(designerTree);
    const approval = schema.nodes.find((node) => node.nodeId === 'approval_1');

    expect(approval?.assigneeRules?.[0]).toMatchObject({
      source: 'deptResponsibleUserChain',
      excludeUsers: [{ id: '3', name: 'C' }],
      extraUsers: [{ id: '9', name: '额外审批人' }],
    });

    expect(workflowSchemaToDesignerTree(schema).childNode.approverConfigs[0]).toMatchObject({
      setType: 6,
      excludeAssigneeList: [{ id: '3', name: 'C' }],
      extraAssigneeList: [{ id: '9', name: '额外审批人' }],
    });
  });

  it('round-trips order contract signing company responsible user assignee', () => {
    const designerTree = {
      nodeName: '发起人',
      nodeKey: 'start_1',
      type: 0,
      childNode: {
        nodeName: '合同公司负责人审批',
        nodeKey: 'approval_1',
        type: 1,
        approverConfigs: [
          {
            setType: 7,
            initiatorDeptScopeMode: 2,
            initiatorDeptList: [{ id: 'dept_2', name: '华东销售部' }],
          },
        ],
        emptyApproverPolicy: 2,
        emptyApproverAssigneeList: [{ id: '8', name: '兜底审批人' }],
        childNode: null,
      },
    };

    const schema = designerTreeToWorkflowSchema(designerTree);
    const approval = schema.nodes.find((node) => node.nodeId === 'approval_1');

    expect(approval?.assigneeRules?.[0]).toMatchObject({
      source: 'orderContractSigningCompanyResponsibleUser',
      initiatorDeptScope: {
        mode: 'specifiedDeptAndSub',
        depts: [{ id: 'dept_2', name: '华东销售部' }],
      },
    });
    expect(approval?.emptyApproverPolicy).toEqual({
      mode: 'specifiedMembers',
      users: [{ id: '8', name: '兜底审批人' }],
    });

    expect(workflowSchemaToDesignerTree(schema).childNode.approverConfigs[0]).toMatchObject({
      setType: 7,
      initiatorDeptScopeMode: 2,
      initiatorDeptList: [{ id: 'dept_2', name: '华东销售部' }],
    });
  });

  it('preserves condition branch merge nodes', () => {
    const designerTree = {
      nodeName: '发起人',
      nodeKey: 'start_1',
      type: 0,
      childNode: {
        nodeName: '条件分支',
        nodeKey: 'route_1',
        type: 4,
        conditionNodes: [
          {
            nodeName: '金额大于一万',
            nodeKey: 'branch_1',
            type: 3,
            priorityLevel: 1,
            conditionList: [[{ field: 'amount', operator: '>', value: '10000' }]],
            childNode: {
              nodeName: '财务审批',
              nodeKey: 'approval_1',
              type: 1,
              childNode: null,
            },
          },
          {
            nodeName: '其他情况',
            nodeKey: 'branch_2',
            type: 3,
            priorityLevel: 2,
            conditionList: [],
            childNode: null,
          },
        ],
        childNode: {
          nodeName: '抄送归档',
          nodeKey: 'copy_1',
          type: 2,
          copyConfigs: [{ setType: 5, nodeAssigneeList: [], examineLevel: 1 }],
          childNode: null,
        },
      },
    };

    const schema = designerTreeToWorkflowSchema(designerTree);
    const route = schema.nodes.find((node) => node.nodeId === 'route_1');
    const branchApproval = schema.nodes.find((node) => node.nodeId === 'approval_1');

    expect(route?.mergeNodeId).toBe('copy_1');
    expect(branchApproval?.nextNodeId).toBe('copy_1');
    expect(route?.branches).toEqual([
      {
        branchId: 'branch_1',
        conditionGroups: [[{ field: 'amount', operator: '>', value: '10000' }]],
        firstNodeId: 'approval_1',
        isFallback: false,
        name: '金额大于一万',
        priority: 1,
      },
      {
        branchId: 'branch_2',
        conditionGroups: [],
        firstNodeId: undefined,
        isFallback: true,
        name: '其他情况',
        priority: 2,
      },
    ]);
    const roundTripTree = workflowSchemaToDesignerTree(schema);
    expect(roundTripTree.childNode.childNode.nodeKey).toBe('copy_1');
    expect(roundTripTree.childNode.conditionNodes[0].childNode.childNode).toBeNull();
  });

  it('parses schema json and legacy designer tree json', () => {
    const schema = createDefaultWorkflowSchema('start_1');
    expect(parseWorkflowDefinitionSchema(serializeWorkflowSchema(schema))).toEqual(schema);

    const legacyTree = {
      nodeName: '发起人',
      nodeKey: 'legacy_start',
      type: 0,
      childNode: null,
    };
    expect(parseWorkflowDefinitionSchema(JSON.stringify(legacyTree))).toEqual({
      startNodeId: 'legacy_start',
      nodes: [
        {
          extensions: undefined,
          name: '发起人',
          nextNodeId: undefined,
          nodeId: 'legacy_start',
          type: 'start',
        },
      ],
    });
  });
});
