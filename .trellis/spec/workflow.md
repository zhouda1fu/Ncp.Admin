# Workflow Spec

本仓库在 Trellis 中推荐沿用以下节奏：

1. Plan
2. Implement
3. Verify
4. Finish

## Plan

- 澄清需求
- 记录范围与非目标
- 标出影响层：后端 / 前端 / 权限 / 数据库 / 测试
- 需要时引用 `ncp-admin-grill-me` 或 `cleanddd-requirements-analysis`

## Implement

- 按现有仓库规则实现
- 后端优先参考 `cleanddd-dotnet-coding`
- 前端优先参考 `ncp-admin-frontend`

## Verify

- 记录执行过的命令
- 记录测试、lint、type-check 或手工验证结果
- 明确未验证项与原因

## Finish

- 总结最终改动
- 记录残留风险
- 若任务可复用，回写到 `workspace/` 或相关文档
