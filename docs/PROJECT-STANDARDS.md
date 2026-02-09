# Ncp.Admin 项目开发规范

本文档为项目负责人制定的统一开发标准，适用于本仓库全体成员（含新成员与 AI 辅助开发）。开发时请先阅读本规范，再结合后端 instructions 与前端约定执行。

---

## 1. 适用范围与原则

- **适用范围**：本仓库后端（C# / .NET、ASP.NET Core + DDD）与前端（Vue 3 + TypeScript + Vite + Ant Design Vue）。
- **优先级**：本规范 > 各层 instructions / 前端 contributing；若本规范与具体 instructions 冲突，以本规范为准；本规范未约定处，遵循各模块对应 instructions 或前端约定。
- **查阅顺序**：先本规范 → 再 [.github/copilot-instructions.md](../.github/copilot-instructions.md) 及 [.github/instructions/](../.github/instructions/)（后端）、[src/frontend/.github/](../src/frontend/.github/)（前端）。

---

## 2. 代码规范

### 2.1 后端（C# / .NET）

- **必须遵循**：[.github/copilot-instructions.md](../.github/copilot-instructions.md) 及对应 [.github/instructions/](../.github/instructions/) 中的开发流程与强制性要求。
- **开发顺序**：定义聚合与实体 → 领域事件 → 仓储接口与实现 → 实体配置 → 命令与命令处理器 → 查询与查询处理器 → Endpoints → 领域事件处理器 → 集成事件与转换器、处理器。
- **关键约定**：主构造函数、Async 优先、业务异常使用 `KnownException`、强类型 ID 不手动赋值、命令必有验证器、分层依赖 Web → Infrastructure → Domain。格式与风格由 IDE 统一。

### 2.2 前端（Vue / TypeScript）

- **必须执行**：在 `src/frontend` 目录下运行 `pnpm lint`、`pnpm format`、`pnpm check:type`，确保通过后再提交。
- **风格**：以现有 ESLint、Prettier、Stylelint 配置为准，不随意关闭规则；新增代码需符合 [src/frontend](src/frontend) 内 lint/format 要求。

---

## 3. 命名与注释

### 3.1 命名

- **后端**：类、方法、公共属性 PascalCase；局部变量、参数 camelCase；常量 PascalCase 或 UPPER_SNAKE；与 .NET 及现有 instructions 一致。
- **前端**：组件、类型 PascalCase；变量、函数 camelCase；与现有 ESLint/TypeScript 约定一致。

### 3.2 注释

- **必须写注释**：公共 API（对外暴露的接口、端点、重要类型）、复杂业务逻辑、非常规设计或绕过逻辑。
- **可选**：自解释的简单方法、与 instructions 示例一致的样板代码可省略注释。
- **语言**：注释与用户可见文案可与现有 instructions 一致，业务错误信息可使用中文。

---

## 4. Git 与提交

### 4.1 分支策略

- 采用 **main + 功能分支** 的简单模型：
  - `main`：可发布的主线，受保护，仅通过 PR 合并。
  - 功能/修复从 `main` 拉取分支（如 `feature/xxx`、`fix/xxx`），开发完成后通过 Pull Request 合并回 `main`。
- 不强制 Git Flow；若团队约定 release 分支或 hotfix 流程，可在此文档中补充。

### 4.2 提交信息

- **全仓库统一** 采用 [Conventional Commits](https://www.conventionalcommits.org/)。
- 格式：`<type>(<scope>): <subject>`；scope 可选，用于区分模块。
- **type**：`feat` | `fix` | `docs` | `style` | `refactor` | `perf` | `test` | `build` | `ci` | `chore` 等。
- **scope 示例**：`api`、`frontend`、`infra`、`domain`、`web` 等，与变更范围一致即可。
- **subject**：简短描述，首字母小写，结尾不加句号；长度建议 50 字以内。
- 详细约定与示例见：[src/frontend/.github/commit-convention.md](../src/frontend/.github/commit-convention.md)。

---

## 5. 分支与 Code Review

- 功能/修复均在**功能分支**上开发，通过 **Pull Request** 合并到 `main`。
- **合并前要求**：
  - CI 通过（若有）。
  - 无未解决的 Review 评论。
  - 至少一名成员 Approve（可按团队实际放宽或收紧）。
- **PR 描述**：说明变更目的、类型（feature/fix/refactor 等）、是否破坏性变更；前端 PR 请勾选 [src/frontend/.github/pull_request_template.md](../src/frontend/.github/pull_request_template.md) 中的检查项。
- **后端**：PR 中需确保 `dotnet build` 无警告、相关测试已运行并通过。

---

## 6. 质量门禁与 CI

### 6.1 合并前必须通过

- **后端**：`dotnet build` 无警告（Directory.Build.props 已启用 WarningsAsErrors）、所有相关单元/集成测试通过。
- **前端**：在 `src/frontend` 下执行 `pnpm lint`、`pnpm check:type`，以及（若适用）`pnpm test`，无报错。

### 6.2 本地 Git 钩子（可选）

- 若团队后续启用 [Lefthook](https://github.com/evilmartians/lefthook)，可在根目录添加 `lefthook.yml`，建议：**pre-commit** 对暂存 `*.cs` 执行 `dotnet build`、对前端变更执行 `pnpm lint`/`pnpm format`；**pre-push** 执行 `dotnet test` 及前端 `pnpm check:type`。启用后请在本文档中补充具体说明。

---

## 7. 测试要求

### 7.1 后端

- 遵循 [.github/instructions/unit-testing.instructions.md](../.github/instructions/unit-testing.instructions.md)。
- 新增功能或修复需配套测试；领域逻辑、命令/查询、关键端点应有单元或集成测试，AAA 模式，命名表达意图。

### 7.2 前端

- 新功能或关键路径建议补充测试；与现有 Vitest 配置一致，不提交仅跳过或仅占位的测试。

---

## 8. 文档与沟通

- **何时更新文档**：新增对外模块、对外 API 变更、部署/运行方式变更、环境变量变更时，更新 [README.md](../README.md) 或 [docs/](.) 下相应文档。
- **Issue/PR**：描述清晰、可复现（Bug）或目标明确（Feature）；引用本规范或 instructions 时可直接贴链接。
- **查阅顺序**：先本规范 → 再各层 instructions / 前端 contributing。

---

## 9. 现有规范索引

| 类别 | 说明 | 参考位置 |
|------|------|----------|
| 后端总纲与顺序 | 开发流程、分层、强制性要求 | [.github/copilot-instructions.md](../.github/copilot-instructions.md) |
| 聚合根 | 实体、聚合根 | [.github/instructions/aggregate.instructions.md](../.github/instructions/aggregate.instructions.md) |
| 领域事件 | 定义与发布 | [.github/instructions/domain-event.instructions.md](../.github/instructions/domain-event.instructions.md) |
| 仓储 | 接口与实现 | [.github/instructions/repository.instructions.md](../.github/instructions/repository.instructions.md) |
| 实体配置 | EF 映射 | [.github/instructions/entity-configuration.instructions.md](../.github/instructions/entity-configuration.instructions.md) |
| DbContext | 数据库上下文 | [.github/instructions/dbcontext.instructions.md](../.github/instructions/dbcontext.instructions.md) |
| 命令 | 命令与验证器、处理器 | [.github/instructions/command.instructions.md](../.github/instructions/command.instructions.md) |
| 查询 | 查询与处理器 | [.github/instructions/query.instructions.md](../.github/instructions/query.instructions.md) |
| 领域事件处理 | 领域事件处理器 | [.github/instructions/domain-event-handler.instructions.md](../.github/instructions/domain-event-handler.instructions.md) |
| API 端点 | FastEndpoints | [.github/instructions/endpoint.instructions.md](../.github/instructions/endpoint.instructions.md) |
| 集成事件 | 事件、转换器、处理器 | [.github/instructions/integration-event.instructions.md](../.github/instructions/integration-event.instructions.md) 等 |
| 单元测试 | 后端测试规范 | [.github/instructions/unit-testing.instructions.md](../.github/instructions/unit-testing.instructions.md) |
| 前端提交约定 | Conventional Commits 细则 | [src/frontend/.github/commit-convention.md](../src/frontend/.github/commit-convention.md) |
| 前端贡献与 PR | 贡献指南、PR 检查项 | [src/frontend/.github/contributing.md](../src/frontend/.github/contributing.md)、[pull_request_template.md](../src/frontend/.github/pull_request_template.md) |
| 前端 Lint/Format | 命令与配置 | `src/frontend` 下 `pnpm lint`、`pnpm format`、`pnpm check:type` 及 eslint/prettier 配置 |

---

*本文档与 .github/instructions 及前端 .github 约定同步维护；若有冲突，以本规范为准。*
