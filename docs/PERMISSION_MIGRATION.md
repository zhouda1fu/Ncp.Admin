# 权限迁移说明：AllApiAccess 与公共基础数据

## 背景

`AllApiAccess` 原为「全局接口访问兜底」，与 FastEndpoints `Permissions(A, B)` 的 **OR** 语义叠加后，会削弱模块细粒度权限。本次改造将公共查询能力拆为独立权限，`AllApiAccess` 仅保留给超级管理员手工勾选。

## 新增权限（角色管理 → 公共基础数据）

| 权限码 | 说明 |
|--------|------|
| `RoleOptionView` | 角色下拉/列表基础查询 |
| `UserOptionView` | 用户下拉/列表基础查询 |
| `DeptOptionView` | 部门树基础查询 |
| `PositionOptionView` | 岗位下拉基础查询 |
| `FileAccess` | 通用文件上传/下载/预览 |

## 存量角色手工迁移步骤

1. 在「角色管理」中排查仍勾选 **所有接口访问权限（AllApiAccess）** 的非超级管理员角色。
2. 按业务需要，在 **公共基础数据** 分组下勾选对应 `*OptionView` 与 `FileAccess`（见下表）。
3. 取消该角色的 `AllApiAccess`。
4. 让相关用户重新登录（或等待权限缓存刷新，约 1 分钟）。
5. 回归验证：工作流设计器、待办转办、带附件的表单、组织用户页等。

### 常见角色配置参考

| 业务场景 | 建议勾选的公共权限 |
|----------|------------------|
| 工作流设计/审批（无 IAM 菜单） | `RoleOptionView`、`UserOptionView`、`DeptOptionView` |
| 组织用户等业务页 | 上述 + `PositionOptionView`、`UserOptionView` |
| 含附件上传的表单 | `FileAccess` |
| 仅需重置用户密码（无完整编辑权限） | `UserResetPassword`（调用 `PUT /api/admin/user/password-reset`，勿走更新用户接口） |
| 平台超级管理员 | 可保留 `AllApiAccess`（全局兜底）；默认 `admin` 种子角色不含此项，靠全量模块权限 |

## 破坏性变更说明

- 仅持有 `AllApiAccess`、未勾选具体模块权限的角色：**无法再**通过 `AllApiAccess` 访问各业务 Endpoint（除仍显式持有 `AllApiAccess` 且由全局授权处理器放行的超级管理员场景）。
- 未迁移的业务角色可能在业务页出现下拉数据 403、附件上传失败等，需按上表补全公共权限。

## 新库与开发环境

- `PlatformAdminSeeder` 仍**排除** `AllApiAccess`，默认管理员通过反射授予全部 `PermissionCodes`（含新增 OptionView）。
- 新建角色请使用「公共基础数据」+ 模块权限组合，勿将 `AllApiAccess` 授予普通业务角色。
