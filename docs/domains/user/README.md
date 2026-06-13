# 用户模块领域文档

> 第一入口。修改用户模块前，先用这一页确定阅读顺序和影响范围；需要深入时再跳到流程、接口或依赖文档。

## 文档结构

| 文档 | 适合什么时候看 |
| --- | --- |
| `README.md` | 快速了解用户模块职责、入口文件、阅读顺序和高频踩坑摘要 |
| `flows.md` | 修改新增、编辑、禁用、离职、删除、登录、导入、审批等业务行为 |
| `api.md` | 修改接口、DTO、请求字段、权限码、缓存、导入导出契约、字段变更检查表 |
| `dependencies.md` | 判断用户模块改动会影响哪些模块、事件、权限和公共能力，含完整风险清单 |

## 模块职责

用户模块负责 OA 系统中的账号与员工基础身份管理，主要包含：

- 用户列表、搜索、分页、导出、导入。
- 新增、编辑、软删除、启用/禁用、离职标记。
- 登录、刷新令牌、退出登录、当前用户信息与权限码获取。
- 用户与角色、部门、岗位的绑定。
- 用户变更后同步员工档案、冗余角色/部门/岗位名称。
- 新增用户可直接保存，也可走工作流审批后再创建。

## 快速入口

### 前端入口

| 用途 | 文件 |
| --- | --- |
| 系统管理路由、菜单权限 | `src/frontend/apps/admin-antd/src/router/routes/modules/system.ts` |
| 用户列表页 | `src/frontend/apps/admin-antd/src/views/system/user/list.vue` |
| 用户创建/编辑页 | `src/frontend/apps/admin-antd/src/views/system/user/form.vue` |
| 用户列表列、搜索表单、编辑表单 schema | `src/frontend/apps/admin-antd/src/views/system/user/data.ts` |
| 用户接口封装与 TS 类型 | `src/frontend/apps/admin-antd/src/api/system/user.ts` |
| 前端权限码常量 | `src/frontend/apps/admin-antd/src/constants/permission-codes.ts` |
| 当前登录用户 store | `src/frontend/packages/stores/src/modules/user.ts` |

### 后端入口

| 用途 | 文件 |
| --- | --- |
| 用户聚合根 | `src/Ncp.Admin.Domain/AggregatesModel/UserAggregate/User.cs` |
| 用户子对象：角色、部门、岗位、刷新令牌 | `src/Ncp.Admin.Domain/AggregatesModel/UserAggregate/UserRole.cs`、`UserDept.cs`、`UserPosition.cs`、`UserRefreshToken.cs` |
| 用户领域事件 | `src/Ncp.Admin.Domain/DomainEvents/UserDomainEvents.cs` |
| EF 映射与表结构 | `src/Ncp.Admin.Infrastructure/EntityConfigurations/UserEntityTypeConfiguration.cs` |
| 用户仓储 | `src/Ncp.Admin.Infrastructure/Repositories/UserRepository.cs` |
| 用户查询模型 | `src/Ncp.Admin.Web/Application/Queries/UserQuery.cs` |
| 用户命令 | `src/Ncp.Admin.Web/Application/Commands/Identity/Admin/UserCommands/` |
| 用户 HTTP 端点 | `src/Ncp.Admin.Web/Endpoints/Identity/Admin/UserEndpoints/` |
| 权限码定义 | `src/Ncp.Admin.Web/AppPermissions/PermissionCodes.cs` |
| 权限树定义 | `src/Ncp.Admin.Web/AppPermissions/PermissionDefinitionContext.cs` |

## 建议阅读顺序

1. 先读前端 `list.vue`、`form.vue`、`data.ts`，确认页面上有哪些字段、按钮和流程。
2. 再读 `src/frontend/apps/admin-antd/src/api/system/user.ts`，确认前端请求路径、请求体和返回字段。
3. 读对应 Endpoint，例如创建用户看 `CreateUserEndpoint.cs`，编辑看 `UpdateUserEndpoint.cs`，登录看 `LoginEndpoint.cs`。
4. 读对应 Command，例如 `CreateUserCommand.cs`、`UpdateUserCommand.cs`、`UpdateUserRolesCommand.cs`、`DeleteUserCommand.cs`。
5. 读 `User.cs` 和 `UserEntityTypeConfiguration.cs`，确认领域规则、表结构、导航属性和软删/离职语义。
6. 如果改动涉及角色、部门、岗位、员工档案或工作流，再读 `dependencies.md`。

## 核心模型

`User` 是用户聚合根，主键为 `UserId`，主要字段分为几类：

- 账号信息：`Name`、`Email`、`Phone`、`PasswordHash`、`Status`、`LastLoginTime`、`LastLoginIp`。
- 人员信息：`RealName`、`Gender`、`BirthDate`、`Age`、`IdCardNumber`、`Address`、`Education`、`GraduateSchool`、`AvatarUrl`、`WechatGuid`。
- OA 业务字段：`NotOrderMeal`、`OrderMealSort`、`IsResigned`、`ResignedTime`。
- 审计字段：`CreatedAt`、`CreatorId`、`ModifierId`、`DeleterId`、`UpdateTime`、`RowVersion`、`IsDeleted`、`DeletedAt`。
- 关联对象：`Roles`、`Dept`、`Position`、`RefreshTokens`。

表结构集中在 `UserEntityTypeConfiguration.cs`：

- `user`：用户主表。
- `user_role`：用户角色，多对多联结，主键为 `UserId + RoleId`，冗余 `RoleName`。
- `user_dept`：用户部门，一对一，主键列名为 `UserId`，冗余 `DeptName`。
- `user_position`：用户岗位，一对一，主键列名为 `UserId`，冗余 `PositionName`。
- `user_refresh_token`：刷新令牌。

## 进一步阅读

- 业务链路：`docs/domains/user/flows.md`
- 接口和 DTO：`docs/domains/user/api.md`
- 跨模块依赖：`docs/domains/user/dependencies.md`

## 高频踩坑摘要

> 完整风险清单见 [`dependencies.md`](./dependencies.md#风险清单)。这里只列三条最容易踩到的。

- **状态/离职/软删是三个独立概念**：`Status`、`IsResigned`、`IsDeleted`。前端保存离职用户时会把 `status` 置为 0；后端登录和多处查询主要按 `IsResigned` 排除。混用会导致登录失败或列表意外不显示。
- **状态开关复用整包 `updateUser`**：列表页切状态时会用当前行数据组装更新请求，新增必填字段后如果列表行没有该字段，可能在切状态时被空值覆盖。
- **新增用户有两条入口**：直接创建走 `CreateUserCommand`，审批创建经流程完成后也复用 `CreateUserCommand`。字段变更要同时覆盖前端审批 `variables` 和后端 `CreateUserVariables`。

## 测试入口

| 层级 | 文件 | 覆盖点 |
| --- | --- | --- |
| 领域测试 | `test/Ncp.Admin.Domain.Tests/UserTests.cs` | 角色更新、年龄计算、角色名称同步、软删除、UserRole |
| Web 测试 | `test/Ncp.Admin.Web.Tests/UserTests.cs` | 创建、重复用户名、空用户名、详情、更新、删除 |
| 角色/部门关联测试 | `test/Ncp.Admin.Domain.Tests/RoleTests.cs`、`DeptTests.cs`、`test/Ncp.Admin.Web.Tests/RoleTests.cs`、`DeptTests.cs` | 用户依赖的角色和部门规则 |

建议后端改动后至少跑：

```powershell
dotnet test test\Ncp.Admin.Domain.Tests\Ncp.Admin.Domain.Tests.csproj --filter UserTests
dotnet test test\Ncp.Admin.Web.Tests\Ncp.Admin.Web.Tests.csproj --filter UserTests
```
