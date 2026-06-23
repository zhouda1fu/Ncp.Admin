# Repo Spec

本仓库是 `Ncp.Admin`，包含：

- 后端：`.NET + CleanDDD + FastEndpoints + MediatR`
- 前端：`Vue 3 + Vite + TypeScript + Ant Design Vue + Vben Admin`
- 测试：`xUnit v3`、`Vitest`

## 主规范入口

优先阅读以下文件，而不是在 `.trellis/` 中重复维护：

- `AGENTS.md`
- `CLAUDE.md`
- `.cursor/rules/project-conventions.mdc`
- `.cursor/rules/frontend-vben.mdc`
- `.cursor/skills/cleanddd-dotnet-coding/SKILL.md`
- `.cursor/skills/ncp-admin-frontend/SKILL.md`
- `docs/ai-tooling.md`

## 适用场景

`.trellis/` 主要用于以下任务：

- 跨前后端的复杂需求
- 需要多轮会话才能完成的任务
- 需要显式记录 PRD、实现边界、验证结果的任务
- 需要在 `Codex` / `Claude Code` / `Cursor` 间接力的任务

简单修复、小改字段、一次性短任务，通常不需要进入 `.trellis/`。
