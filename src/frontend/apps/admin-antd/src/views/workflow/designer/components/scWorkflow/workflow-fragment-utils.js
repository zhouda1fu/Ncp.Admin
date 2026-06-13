import { createWorkflowNodeKey } from '../../utils/createWorkflowNodeKey';

export function cloneWorkflowValue(value) {
  return JSON.parse(JSON.stringify(value));
}

function nodeTitle(node) {
  return node?.nodeName || '未命名节点';
}

function createTreeNode(node, rootKey) {
  if (!node) return null;

  const children = [];
  if (node.type === 4 && Array.isArray(node.conditionNodes)) {
    node.conditionNodes.forEach((branch) => {
      const branchChildren = [];
      const branchChild = createTreeNode(branch.childNode, rootKey);
      if (branchChild) branchChildren.push(branchChild);
      children.push({
        key: branch.nodeKey,
        title: nodeTitle(branch),
        children: branchChildren,
      });
    });
  }

  const child = createTreeNode(node.childNode, rootKey);
  if (child) children.push(child);

  return {
    key: node.nodeKey,
    title: nodeTitle(node),
    disabled: node.nodeKey === rootKey,
    children,
  };
}

export function createWorkflowFragmentTree(node) {
  const root = createTreeNode(node, node?.nodeKey);
  return root ? [root] : [];
}

export function collectWorkflowFragmentKeys(node) {
  const keys = [];
  const walk = (current) => {
    if (!current) return;
    if (current.nodeKey) keys.push(current.nodeKey);

    if (current.type === 4 && Array.isArray(current.conditionNodes)) {
      current.conditionNodes.forEach((branch) => {
        if (branch.nodeKey) keys.push(branch.nodeKey);
        walk(branch.childNode);
      });
    }

    walk(current.childNode);
  };

  walk(node);
  return keys;
}

export function createWorkflowFragment(node, checkedKeys) {
  const selectedKeys = new Set(checkedKeys || []);
  const prune = (current) => {
    if (!current?.nodeKey || !selectedKeys.has(current.nodeKey)) return null;

    const cloned = cloneWorkflowValue(current);
    if (cloned.type === 4 && Array.isArray(cloned.conditionNodes)) {
      cloned.conditionNodes = cloned.conditionNodes
        .filter((branch) => !branch.nodeKey || selectedKeys.has(branch.nodeKey))
        .map((branch, index) => ({
          ...branch,
          priorityLevel: index + 1,
          childNode: prune(branch.childNode),
        }));
    }

    cloned.childNode = prune(cloned.childNode);
    return cloned;
  };

  return prune(node);
}

function rekeyWorkflowFragment(node) {
  if (!node) return null;
  const cloned = cloneWorkflowValue(node);
  if (cloned.nodeKey) cloned.nodeKey = createWorkflowNodeKey();

  if (cloned.type === 4 && Array.isArray(cloned.conditionNodes)) {
    cloned.conditionNodes = cloned.conditionNodes.map((branch, index) => ({
      ...branch,
      nodeKey: branch.nodeKey ? createWorkflowNodeKey() : branch.nodeKey,
      priorityLevel: index + 1,
      childNode: rekeyWorkflowFragment(branch.childNode),
    }));
  }

  cloned.childNode = rekeyWorkflowFragment(cloned.childNode);
  return cloned;
}

function appendAfterFragmentTail(node, nextNode) {
  if (!node) return nextNode;

  if (node.childNode) {
    appendAfterFragmentTail(node.childNode, nextNode);
  } else {
    node.childNode = nextNode;
  }

  return node;
}

export function cloneWorkflowFragmentForInsert(fragment, originalNextNode) {
  const cloned = rekeyWorkflowFragment(fragment);
  return appendAfterFragmentTail(cloned, originalNextNode ? cloneWorkflowValue(originalNextNode) : null);
}
