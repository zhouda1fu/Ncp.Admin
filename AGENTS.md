# 仓库指南

## 项目结构与模块组织
`src/` 存放应用代码。后端按职责拆分为 `Ncp.Admin.Domain`、`Ncp.Admin.Infrastructure`、`Ncp.Admin.Web`、`Ncp.Admin.AppHost` 和 `Ncp.Admin.ServiceDefaults`。前端位于 `src/frontend/`，主应用在 `src/frontend/apps/admin-antd`，共享包集中在 `src/frontend/packages` 与 `src/frontend/internal`。测试按层放在 `test/` 下，如 `Ncp.Admin.Domain.Tests`、`Ncp.Admin.Infrastructure.Tests`、`Ncp.Admin.Web.Tests`、`Ncp.Admin.AppHost.Tests`。补充文档和图片在 `docs/`，基础设施脚本在 `scripts/`。

## 构建、测试与开发命令
后端：
- `dotnet run --project src/Ncp.Admin.AppHost`：启动基于 Aspire 的本地依赖栈。
- `dotnet build Ncp.Admin.slnx`：构建全部 .NET 项目。
- `dotnet test Ncp.Admin.slnx --collect:"XPlat Code Coverage"`：运行 xUnit 测试并采集 Coverlet 覆盖率。

前端（先执行 `cd src/frontend`）：
- `pnpm install`：安装工作区依赖（要求 `Node >= 20.12`、`pnpm >= 10`）。
- `pnpm dev:antd`：启动管理端，本地地址为 `http://localhost:5666`。
- `pnpm build:antd`：构建生产包。
- `pnpm lint`、`pnpm format`、`pnpm check:type`、`pnpm test:unit`：分别执行代码检查、格式化、类型检查和 Vitest 单测。

## 代码风格与命名约定
遵循 `.editorconfig`：UTF-8、CRLF、文件末尾保留换行、禁止行尾空格。后端保持现有 .NET 风格：类型和公共成员使用 PascalCase，局部变量和参数使用 camelCase，通常一文件一类。前端遵循工作区 ESLint 和 Prettier 配置，即 `src/frontend/eslint.config.mjs` 与 `src/frontend/.prettierrc.mjs`。命名保持现有模式：Vue 组件使用 PascalCase，组合式函数使用 `useXxx`，功能目录按业务域分组。

## 测试规范
后端测试使用 xUnit v3，各层测试放在对应的 `test/` 项目中；新增测试应放到所覆盖层旁边，文件名以 `Tests.cs` 结尾。前端单元测试使用 Vitest 和 `happy-dom`；测试文件应靠近功能代码或放在既有测试目录中，`e2e` 不纳入单测覆盖范围。凡是改动共享基础设施、接口端点或可复用前端包时，都应同步补充或更新测试。

## 提交与合并请求规范
近期提交历史主要使用 `feat`、`fix`、`refactor` 等前缀。前端工作区还配置了 commitlint，允许的类型包括 `feat`、`fix`、`docs`、`test`、`refactor`、`build`、`ci`、`chore`、`perf`、`style`、`types` 和 `revert`；有明确范围时，优先使用 `type(scope): summary`。Pull Request 应说明变更内容、标注影响范围（如 `src/` 或 `src/frontend/`）、关联 Issue；涉及界面改动时附上截图。若包含数据库结构、种子数据或环境变量调整，需要在描述中明确说明。
