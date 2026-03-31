import { getDeptTree } from '#/api/system/dept';
import { getRoleList } from '#/api/system/role';
import { getUserList } from '#/api/system/user';

/** 审批工作流人员/组织选择器配置 - 使用真实部门、用户、角色接口 */
const config = {
  successCode: 200,
  group: {
    apiObj: {
      get: async () => ({ data: await getDeptTree() }),
    },
    parseData(res: { data?: Array<{ id?: string; name?: string; key?: string; label?: string; children?: any[] }> }) {
      const raw = res?.data ?? [];
      const toTree = (arr: typeof raw) =>
        arr.map((n) => ({
          key: (n as any).key ?? String(n.id),
          label: (n as any).label ?? n.name,
          children: n.children?.length ? toTree(n.children) : undefined,
        }));
      return { rows: toTree(raw), msg: '', code: 200 };
    },
    props: { key: 'key', label: 'label', children: 'children' },
  },
  user: {
    apiObj: {
      get: async (params: { page?: number; pageSize?: number; groupId?: string; keyword?: string }) => {
        const result = await getUserList({
          pageIndex: params?.page ?? 1,
          pageSize: params?.pageSize ?? 20,
          deptId: params?.groupId ?? undefined,
          keyword: params?.keyword ?? undefined,
        });
        return { data: { rows: result.items, total: result.total } };
      },
    },
    pageSize: 20,
    parseData(res: { data?: { rows?: any[]; total?: number }; code?: number; msg?: string }) {
      const data = res?.data ?? {};
      const raw = data.rows ?? [];
      // 保证每行有 key（后端可能用 userId 或 id），供树节点与选择结果 id 使用，否则保存后 nodeAssigneeList 只有 name 无 id，解析审批人为空
      const rows = raw.map((r: any) => ({
        ...r,
        key: r.userId ?? r.id ?? '',
        label: r.name ?? r.label ?? '',
      }));
      return {
        rows,
        total: data.total ?? 0,
        msg: res?.msg ?? '',
        code: res?.code ?? 200,
      };
    },
    props: { key: 'key', label: 'label' },
    request: { page: 'page', pageSize: 'pageSize', groupId: 'groupId', keyword: 'keyword' },
  },
  role: {
    apiObj: {
      get: async () => {
        const result = await getRoleList({ pageIndex: 1, pageSize: 999 });
        return { data: result.items ?? [] };
      },
    },
    parseData(res: { data?: Array<{ roleId: string; name: string }> }) {
      const raw = res?.data ?? [];
      const rows = raw.map((r) => ({ key: r.roleId, label: r.name, children: [] }));
      return { rows, msg: '', code: 200 };
    },
    props: { key: 'key', label: 'label', children: 'children' },
  },
};

export default config;
