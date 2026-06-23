# Trellis

本目录用于承载复杂任务的任务流转、任务状态和跨会话工作记忆。

它的定位是：

- `AGENTS.md` / `CLAUDE.md` / `.cursor/rules` / `skills` 继续负责**规则**
- `.trellis/` 负责**复杂任务的 spec / task / workspace**

不要把现有规则全文复制到 `.trellis/` 中；优先引用现有文档。

## 目录

- `spec/`：仓库背景、约定和工作流索引
- `tasks/`：复杂任务的任务文档、执行上下文和验收记录
- `workspace/`：跨会话的工作记忆、研究记录、临时结论
