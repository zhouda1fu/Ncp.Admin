# AI Tooling Migration Checklist

本文档用于把当前仓库的 AI 工具链结构迁移到另一个基于本项目演化出来的仓库。

适用场景：

- 另一个项目整体结构与当前仓库接近
- 希望复用 `Codex` / `Claude Code` / `Cursor` 的多工具规则组织方式
- 另一个项目已有部分自定义 skill，需要保留项目差异

## 迁移目标

- 建立单一主规范源
- 保留 Claude / Cursor 的薄入口
- 让跨工具 skill 有主副本与镜像副本
- 通过脚本和 git hook 降低双份 skill 漂移风险

## 文件分类

### 一、可以直接复制

这些文件主要是结构性能力，通常可以直接迁移：

- `docs/ai-tooling.md`
- `docs/ai-usage-examples.md`
- `docs/ai-tooling-migration-checklist.md`
- `scripts/sync-agent-skills.ps1`
- `CLAUDE.md`
- `.agents/skills/ncp-admin-grill-me/SKILL.md`
- `.cursor/skills/ncp-admin-grill-me/SKILL.md`

### 二、需要人工合并

这些文件通常包含项目路径、规则入口或已有配置，不建议直接覆盖：

- `AGENTS.md`
- `.cursor/rules/project-conventions.mdc`
- `.cursor/rules/frontend-vben.mdc`
- `lefthook.yml`

合并时重点保留本次整理出来的结构性约定：

- `AGENTS.md` 作为主规范源
- `CLAUDE.md` 使用 `@AGENTS.md` 作为薄包装
- Cursor rules 只保留入口索引和仓库特例
- `lefthook.yml` 增加 `sync-agent-skills` 的 `pre-commit` job

### 三、必须按目标项目定制

这些文件很可能带有目标项目自己的前端、领域或权限差异，必须人工确认：

- `.cursor/skills/ncp-admin-frontend/SKILL.md`
- `.cursor/skills/cleanddd-dotnet-coding/SKILL.md`
- `.cursor/skills/cleanddd-requirements-analysis/SKILL.md`

如果目标项目前端 skill 已经改过，优先检查：

- 前端根路径是否仍是 `src/frontend/apps/admin-antd/`
- 路由、权限树、菜单高亮规则是否一致
- `views / api / routes / locales` 的目录模式是否一致
- 是否还沿用 `useVbenDrawer + useVbenForm`

## 推荐迁移步骤

1. 复制“可以直接复制”的文件
2. 人工合并 `AGENTS.md`
3. 人工合并 `.cursor/rules/project-conventions.mdc`
4. 人工合并 `.cursor/rules/frontend-vben.mdc`
5. 在 `lefthook.yml` 中加入 `sync-agent-skills` 的 `pre-commit` job
6. 检查 `docs/ai-tooling.md` 里的项目名、路径、skill 名称是否需要调整
7. 检查 `scripts/sync-agent-skills.ps1` 中的 skill 映射是否匹配目标项目
8. 检查 `.agents/skills/` 与 `.cursor/skills/` 中是否都存在对应 skill
9. 修改一个 `.agents/skills/.../SKILL.md` 后试一次 `git commit`
10. 确认 `pre-commit` 会自动同步 `.cursor/skills/...` 并重新加入暂存区

## 迁移核对清单

- [ ] 目标项目已存在 `AGENTS.md`
- [ ] `CLAUDE.md` 顶部已使用 `@AGENTS.md`
- [ ] Cursor rules 没有重复大段仓库规范
- [ ] `.agents/skills/` 作为跨工具 skill 主副本路径
- [ ] `.cursor/skills/` 作为 Cursor 镜像副本路径
- [ ] `scripts/sync-agent-skills.ps1` 已根据目标项目调整映射
- [ ] `lefthook.yml` 已配置 `pre-commit` 自动同步
- [ ] 前端 skill 内容已按目标项目差异检查
- [ ] 项目名、程序集名、模块路径已全部替换
- [ ] 手工试过一次提交，确认 hook 生效

## 不建议直接覆盖的原因

- 目标项目的前端 skill 很可能已经分叉
- `AGENTS.md` 和 Cursor rules 往往包含项目专属路径和约束
- `lefthook.yml` 可能已有其他 job，直接覆盖容易丢失配置

## 当前仓库的基准结构

- 主规范：`AGENTS.md`
- Claude 入口：`CLAUDE.md`
- Cursor 总规则：`.cursor/rules/project-conventions.mdc`
- Cursor 前端入口：`.cursor/rules/frontend-vben.mdc`
- Codex 主 skill：`.agents/skills/`
- Cursor 镜像 skill：`.cursor/skills/`
- 同步脚本：`scripts/sync-agent-skills.ps1`
- 说明文档：`docs/ai-tooling.md`
