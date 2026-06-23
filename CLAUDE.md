# CLAUDE.md

@AGENTS.md

本文件仅保留 Claude Code 需要的补充说明；通用仓库规范以 `AGENTS.md` 为准。

## 优先参考

- 通用仓库指南：`AGENTS.md`
- 前端详细规范：`.cursor/skills/ncp-admin-frontend/SKILL.md`
- 后端详细规范：`.cursor/skills/cleanddd-dotnet-coding/SKILL.md`
- 需求澄清：`.cursor/skills/cleanddd-requirements-analysis/SKILL.md`

## Claude Code 使用建议

- 需求不清、边界不稳、涉及权限或前后端联动时，先做需求澄清，再开始实现。
- 新增受权限保护的功能时，优先检查前后端权限码、权限树、路由 authority、端点 `Permissions(...)` 是否需要同步。
- 涉及 Vben Admin 表单或抽屉时，优先复用现有 `useVbenDrawer + useVbenForm` 模式。
- 涉及 CleanDDD 业务改动时，保持 `Web -> Infrastructure -> Domain` 依赖方向，不在 Command Handler 中直接 `SaveChanges`。

## 常用命令

```bash
dotnet run --project src/Ncp.Admin.AppHost
dotnet build Ncp.Admin.slnx
dotnet test Ncp.Admin.slnx --collect:"XPlat Code Coverage"

cd src/frontend
pnpm install
pnpm dev:antd
pnpm lint
pnpm check:type
pnpm test:unit
```
