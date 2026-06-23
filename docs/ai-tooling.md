# AI Tooling Guide

本文档说明本仓库在 `Codex`、`Claude Code`、`Cursor` 间如何组织 AI 规则与技能，避免多份说明长期漂移。

## 目标

- 通用规范只维护一份
- 各工具只保留自己的入口包装
- 自定义 skill 有明确主副本与同步方式

## 文件分工

### 通用规范

- `AGENTS.md`

这是仓库级主规范，放通用结构、构建命令、测试规范、命名约定等所有跨工具都适用的内容。

### Claude Code

- `CLAUDE.md`

这是 Claude Code 的入口文件。它应尽量保持精简：

- 顶部通过 `@AGENTS.md` 复用通用规范
- 只补充 Claude 专属提示
- 不重复大段仓库规则

### Cursor

- `.cursor/rules/project-conventions.mdc`
- `.cursor/rules/frontend-vben.mdc`

这些文件用于 Cursor 自动应用规则。它们应保留：

- 自动触发时值得提前提醒的仓库特例
- 规则导航入口

不要在这里重复完整业务规范；详细内容放到对应 skill。

### Skills

- Codex 主副本：`.agents/skills/`
- Cursor 镜像副本：`.cursor/skills/`

当前仓库中的 `ncp-admin-grill-me` 采用：

- 主副本：`.agents/skills/ncp-admin-grill-me/SKILL.md`
- 镜像副本：`.cursor/skills/ncp-admin-grill-me/SKILL.md`

## Skill 维护方式

修改跨工具共用 skill 时：

1. 先修改 `.agents/skills/` 下的主副本
2. 再运行同步脚本：

```powershell
powershell -ExecutionPolicy Bypass -File scripts\sync-agent-skills.ps1
```

当前同步脚本：

- `scripts/sync-agent-skills.ps1`

如果仓库已启用 `lefthook`，提交时也会在 `pre-commit` 自动执行同步，并把镜像副本重新加入暂存区。

## 推荐维护原则

- 通用规则优先写入 `AGENTS.md`
- Claude / Cursor 入口文件保持薄包装
- 详细操作流程、易错点、检查清单放 skill
- 不要在多个入口文件中复制同一段长说明
- 修改规则后，顺手检查是否有别的入口文件已经可以删减

## 当前前端规则边界

- `.cursor/rules/frontend-vben.mdc`：只做前端路径索引与“何时切到详细 skill”
- `.cursor/skills/ncp-admin-frontend/SKILL.md`：前端详细规范、易错点、权限树、抽屉表单、`activePath`

## 当前后端规则边界

- `AGENTS.md`：通用仓库说明
- `.cursor/rules/project-conventions.mdc`：Cursor 自动应用时需要提前提醒的仓库特例
- `.cursor/skills/cleanddd-dotnet-coding/SKILL.md`：详细 CleanDDD / .NET 实现规范
