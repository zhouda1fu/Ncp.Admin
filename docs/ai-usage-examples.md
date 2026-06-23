# AI Usage Examples

本文档说明如何在本仓库里实际使用新增的 AI 能力：

- `ncp-admin-grill-me`：需求澄清 skill
- `.trellis/`：复杂任务的任务流转与跨会话记忆

如果你只想知道“项目里怎么用”，优先看本文档。  
如果你想知道“这些文件为什么这样组织”，再看 `docs/ai-tooling.md`。

## 一、先理解两个东西分别是什么

### 1. `ncp-admin-grill-me`

这是一个 **skill**，用来在动手前把需求问清楚。

适合：

- 需求描述很短，边界不清
- 不确定是改前端、后端还是两边
- 可能涉及权限、菜单、接口、测试
- 想先把范围和验收标准问清楚，再实现

当前路径：

- Codex 主副本：`.agents/skills/ncp-admin-grill-me/SKILL.md`
- Cursor 镜像副本：`.cursor/skills/ncp-admin-grill-me/SKILL.md`

### 2. `.trellis/`

这不是 skill，而是 **复杂任务工作区**。

适合：

- 一个需求要做很多步
- 任务要跨多个会话继续
- 需要显式记录 Plan / Implement / Verify
- 需要在 Codex / Claude Code / Cursor 之间接力

当前目录：

- `.trellis/spec/`
- `.trellis/tasks/`
- `.trellis/workspace/`

## 二、什么时候用 `ncp-admin-grill-me`

在你还没准备直接改代码时，用它最合适。

### 示例 1：需求很模糊

```text
使用 ncp-admin-grill-me 帮我先澄清这个需求，不要直接改代码：
给用户管理加一个导出功能，但不同角色看到的内容不一样。
```

预期效果：

- AI 会继续追问
- 会问权限影响、入口位置、前后端影响、导出格式、验收标准
- 最后产出清晰任务卡

### 示例 2：你已经知道大方向，但边界还不清楚

```text
先用 ncp-admin-grill-me，把这个需求的范围和非目标问清楚：
工作流定义页面要支持复制流程模板。
```

### 示例 3：你担心权限漏改

```text
用 ncp-admin-grill-me 帮我检查这个需求需要影响哪些权限、路由和前后端文件：
给客户列表新增“批量分配负责人”操作。
```

## 三、`ncp-admin-grill-me` 用完之后怎么接着做

通常会接两种方向：

### 1. 转实现

如果需求已经清楚：

- 后端实现：进入 `cleanddd-dotnet-coding`
- 前端实现：进入 `ncp-admin-frontend`

示例：

```text
刚才已经用 ncp-admin-grill-me 澄清完需求了。
现在按结果开始实现后端，遵循 cleanddd-dotnet-coding。
```

```text
刚才需求边界已经明确。
现在按 ncp-admin-frontend 的规则实现前端页面和路由。
```

### 2. 转 Trellis

如果任务很复杂，建议落到 `.trellis/tasks/` 再继续。

示例：

```text
基于刚才 ncp-admin-grill-me 产出的范围，把这个需求整理成一个 Trellis 任务，不要直接写代码。
```

## 四、什么时候用 Trellis

不是所有任务都值得进 `.trellis/`。

更适合进入 Trellis 的任务：

- 跨前后端
- 涉及权限
- 要补测试
- 需要多轮会话
- 一次做不完

不太需要进 Trellis 的任务：

- 小 bug 修复
- 只改一个字段
- 只改一个页面小样式
- 一次会话就能完成的小改动

## 五、怎么创建一个 Trellis 任务

建议先使用模板：

- `.trellis/tasks/TEMPLATE.md`

### 示例 1：让 AI 帮你创建任务

```text
基于 .trellis/tasks/TEMPLATE.md，
为“用户管理增加导出权限控制”创建一个 Trellis 任务，
只整理任务文档，不要直接改代码。
```

### 示例 2：指定任务目录

```text
在 .trellis/tasks/2026-06-23-user-export-permission/ 下创建任务文档，
目标是“用户管理导出功能按角色控制字段范围”，
先写 Goal、Scope、Impact、Plan、Verification。
```

## 六、Trellis 任务里通常写什么

一个复杂任务建议至少有这些文件：

- `task.md`：目标、范围、非目标、验收标准
- `implementation.md`：实现记录、关键决策、影响文件
- `verification.md`：测试、lint、type-check、手工验证结果

如果任务不大，也可以只先写一个 `task.md`。

## 七、推荐工作流

### 短任务

1. 直接提需求
2. 如果边界不清，先用 `ncp-admin-grill-me`
3. 然后直接实现

### 复杂任务

1. 先用 `ncp-admin-grill-me` 澄清
2. 再进入 `.trellis/tasks/` 建任务
3. 然后实现
4. 最后写验证记录

## 八、完整示例

### 示例 A：短任务

```text
使用 ncp-admin-grill-me 帮我先澄清这个需求：
部门列表新增一个“启用/停用”操作。
```

澄清完成后：

```text
现在按刚才确认的范围开始实现。
后端遵循 cleanddd-dotnet-coding，前端遵循 ncp-admin-frontend。
```

### 示例 B：复杂任务

```text
先用 ncp-admin-grill-me 澄清需求，不要直接改代码：
客户管理要支持批量导入，并且不同角色可导入的字段不同，还要记录操作日志。
```

澄清完成后：

```text
把这个需求整理为一个 Trellis 任务，放到 .trellis/tasks/，
包含 Goal、Scope、Impact、Plan、Verification。
```

然后再实现：

```text
基于刚才创建的 Trellis 任务，先实现后端部分，再补验证记录。
```

## 九、维护注意事项

### 1. 修改 `ncp-admin-grill-me` 时

主副本在：

- `.agents/skills/ncp-admin-grill-me/SKILL.md`

修改后可手动同步：

```powershell
powershell -ExecutionPolicy Bypass -File scripts\sync-agent-skills.ps1
```

如果仓库启用了 `lefthook`，提交时也会自动同步镜像副本。

### 2. Trellis 不要复制现有规则正文

Trellis 里应该：

- 写任务
- 写决策
- 写验证

不要：

- 把 `AGENTS.md` 大段复制进去
- 把前后端 skill 正文再抄一遍

## 十、最短使用建议

如果你只记一句话：

- **需求不清时，用 `ncp-admin-grill-me`**
- **任务复杂且跨会话时，用 `.trellis/tasks/`**

## 十一、真实业务示例

下面给一个更贴近当前仓库的真实使用方式。

### 场景：用户管理新增“批量导出”，并按角色控制可导出字段范围

这个需求通常会同时涉及：

- 前端页面按钮与交互
- 后端导出接口
- 权限码与权限树
- 角色差异
- 验收标准

因此很适合先用 `ncp-admin-grill-me`，再决定是否进入 Trellis。

### 第一步：先用 `ncp-admin-grill-me` 澄清

你可以这样发起：

```text
使用 ncp-admin-grill-me 帮我先澄清这个需求，不要直接改代码：
用户管理页面要新增批量导出功能，但不同角色允许导出的字段不一样。
```

比较理想的后续追问通常会覆盖：

- 导出入口在用户列表哪里
- 哪些角色可以看到导出按钮
- 哪些角色可以导出哪些字段
- 是否支持按当前筛选条件导出
- 导出格式是 Excel、CSV 还是别的
- 是否要记录操作日志
- 是否要限制导出条数
- 前端是否仅控制按钮显示，后端是否也要做权限校验
- 这次是否需要补测试

### 第二步：如果任务复杂，先整理成 Trellis 任务

如果你判断这个需求不是一轮能做完，可以继续这样说：

```text
基于刚才澄清出来的范围，
把“用户管理批量导出并按角色控制字段范围”整理成一个 Trellis 任务，
放到 .trellis/tasks/2026-06-23-user-export-permission/ 下，
先写 task.md，不要直接改代码。
```

建议这个 `task.md` 至少写出：

- Goal
- Scope
- Excludes
- Backend impact
- Frontend impact
- Permission impact
- Verification

### 第三步：进入实现

如果需求已经清楚，可以继续分层推进：

```text
基于刚才的 Trellis 任务，先实现后端部分。
遵循 cleanddd-dotnet-coding。
重点处理导出接口、权限校验和操作日志。
```

然后前端：

```text
现在实现前端部分，遵循 ncp-admin-frontend。
需要补导出按钮、权限控制、筛选条件联动和文案。
```

### 第四步：补验证记录

做完后可以要求 AI 把验证过程回写到 Trellis：

```text
把这次实现的验证结果补到该 Trellis 任务的 verification.md，
记录执行过的命令、通过项、未验证项和原因。
```

### 这个示例为什么适合当前仓库

因为它天然覆盖了你们项目里最容易漏的几个点：

- 后端权限码与端点权限
- 前端 `permission-codes.ts` 与 `permission-tree.ts`
- 列表页按钮权限显示
- 前后端都要做权限控制，而不是只做前端隐藏
- 可能还涉及操作日志和测试

### 一句话工作流

这个场景最推荐的顺序是：

1. `ncp-admin-grill-me` 先问清楚
2. 复杂就进入 `.trellis/tasks/`
3. 后端按 `cleanddd-dotnet-coding` 实现
4. 前端按 `ncp-admin-frontend` 实现
5. 最后把验证结果回写到 Trellis
