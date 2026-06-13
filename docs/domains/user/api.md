# 用户模块接口与契约

> 修改接口、DTO、权限码、缓存、导入导出字段时看本文件。业务行为见 `flows.md`。

## 主要接口

| 场景 | 前端方法 | 后端端点 | 权限 |
| --- | --- | --- | --- |
| 用户列表 | `getUserList` | `GET /api/admin/users` | `UserView` |
| 用户详情 | `getUser` | `GET /api/admin/users/{id}` | `UserView` |
| 创建用户 | `createUser` | `POST /api/admin/users` | `UserCreate` |
| 更新用户 | `updateUser` | `PUT /api/admin/user/update` | `UserEdit` |
| 删除用户 | `deleteUser` | `DELETE /api/admin/users/{id}` | `UserDelete` |
| 分配角色 | `updateUserRoles` | `PUT /api/admin/users/update-roles` | `UserRoleAssign` |
| 导出 Excel | `exportUsersExcel` | `GET /api/admin/users/excel/export` | `UserExport` |
| 下载导入模板 | `downloadUserImportTemplate` | `GET /api/admin/users/excel/import-template` | `UserImport` |
| 导入 Excel | `importUsersExcel` | `POST /api/admin/users/excel/import` | `UserImport` |
| 登录 | core auth API | `POST /api/admin/user/login` | 匿名 |
| 当前用户权限码 | core user API | `GET /api/admin/user/access-codes` | 登录用户 |

注意：更新用户接口路径是 `/api/admin/user/update`，不是 `/api/admin/users/{id}`。前端封装会把 `id` 合并为 `userId` 放进请求体。

## 前端契约

主要文件：`src/frontend/apps/admin-antd/src/api/system/user.ts`。

核心类型：

- `SystemUserApi.SystemUser`：列表和详情共用的用户类型。
- `GetUserListParams`：列表、导出和用户选择器常用查询参数。
- `UserImportResult`：导入结果，包含成功数和行级错误。
- `WorkflowRoutingRoleItem`：当前用户可用于工作流路由的角色。

列表查询参数：

- `pageIndex`、`pageSize`、`countTotal`。
- `keyword`：后端按用户名或邮箱模糊查询。
- `status`：启用/禁用。
- `isResigned`：在职/离职。
- `deptId`、`positionId`：部门和岗位筛选；后端优先 `positionId`。
- `onlyMarketingCenterDeptSubtree`：为 true 时仅返回名为「营销中心」的部门及其下级部门的用户，并忽略请求中的 `deptId`、`positionId`（用于客户协作转交/分享选人等）。

创建请求字段主要来自 `form.vue` 和 `data.ts`：

- 基础字段：`name`、`email`、`password`、`phone`、`realName`、`status`。
- 人员字段：`gender`、`birthDate`、`idCardNumber`、`address`、`education`、`graduateSchool`、`avatarUrl`。
- 组织字段：`deptId`、`deptName`、`roleIds`。
- 业务字段：`notOrderMeal`、`wechatGuid`、`isResigned`、`resignedTime`。

更新请求字段和创建类似，但通过请求体中的 `userId` 定位用户，`password` 可为空，空值表示不更新密码。

## 后端 DTO 和 Query

主要文件：

- `src/Ncp.Admin.Web/Application/Queries/UserQuery.cs`
- `src/Ncp.Admin.Web/Endpoints/Identity/Admin/UserEndpoints/`

核心 DTO：

- `UserInfoQueryDto`：用户列表和详情的主要输出模型。
- `UserLoginInfoQueryDto`：登录校验用，包含用户 ID、用户名、邮箱、密码哈希和角色集合。
- `UserDataPermissionSnapshot`：数据权限可见性判定用，包含用户部门和角色 ID。
- `UserQueryInput`：列表和导出筛选参数。

查询行为：

- `GetUserByIdAsync`：按用户 ID 查详情，带 10 分钟内存缓存，并排除离职用户。
- `GetAllUsersAsync`：按 `UserQueryInput` 分页查询。
- `GetUsersForExportAsync`：按同一套筛选条件导出，超过 `UserExportMaxRows` 抛异常。
- `GetUserInfoForLoginAsync` / `GetUserInfoForLoginByIdAsync`：登录和 token 场景，排除离职用户。
- `GetUserIdsByDeptIdAsync`、`GetUserIdsByDeptIdsAsync`：按部门取用户，排除离职用户。

## 权限契约

后端：

- `src/Ncp.Admin.Web/AppPermissions/PermissionCodes.cs`
- `src/Ncp.Admin.Web/AppPermissions/PermissionDefinitionContext.cs`

前端：

- `src/frontend/apps/admin-antd/src/constants/permission-codes.ts`
- `src/frontend/apps/admin-antd/src/router/routes/modules/system.ts`
- `src/frontend/apps/admin-antd/src/utils/permission-tree.ts`

用户模块权限码：

- `UserManagement`
- `UserCreate`
- `UserEdit`
- `UserDelete`
- `UserView`
- `UserRoleAssign`
- `UserResetPassword`
- `UserExport`
- `UserImport`

新增、删除或改名权限码时，要同步后端常量、权限树、前端常量、路由 authority、按钮级权限判断。

## 登录 JWT 契约

登录端点会写入：

- `ClaimTypes.Name`
- `ClaimTypes.Email`
- `ClaimTypes.NameIdentifier`
- `permissions`
- `data_scope`
- `dept_id`
- `authorized_dept_ids`

`data_scope` 和 `authorized_dept_ids` 来自用户角色和部门。改用户部门、角色或数据权限时，要确认这些 Claims 的消费方仍然一致。

## 缓存契约

`UserQuery.GetUserByIdAsync` 使用 `IMemoryCache`，缓存键为：

```text
user:{userId}
```

缓存过期时间为 10 分钟。`UpdateUserCommandHandler` 更新用户后会调用：

```csharp
memoryCache.Remove(UserQuery.GetUserCacheKey(request.UserId));
```

如果新增了其他会改变用户详情的写操作，也要考虑清理同一缓存。

## Excel 契约

主要目录：

- `src/Ncp.Admin.Web/Application/Identity/Admin/UserExcel/`
- `src/Ncp.Admin.Web/Application/Commands/Identity/Admin/UserCommands/ImportUsersCommand.cs`

导入：

- 模板和解析列由 `UserExcelColumns.cs`、`UserImportRowDto.cs`、`UserImportParsing.cs` 控制。
- 行数据最终构建为 `CreateUserCommand`。
- 单次导入限制为 1 到 5000 行。

导出：

- 导出使用 `UserQuery.GetUsersForExportAsync`。
- 导出最大行数为 `UserExportMaxRows = 50_000`。
- 导出不包含密码。

新增字段如果需要导入或导出，需要同时修改模板、解析 DTO、工作簿生成、前端提示。

## 新增/修改用户字段时的全栈检查清单

新增或重命名用户字段时，按以下顺序逐项确认。流程行为侧的入口（如 `form.vue`、Excel 导入等）见 `flows.md`，本表只罗列契约和数据层需要同步的位置。

后端：

1. `User.cs`：聚合字段、构造函数、`UpdateUserInfo` 等领域行为。
2. `UserEntityTypeConfiguration.cs`：字段长度、是否必填、注释、索引。
3. EF migration 脚本。
4. `UserQuery.UserInfoQueryDto` 和 `ToUserInfoQueryDto`，确认列表、详情、导出查询都覆盖。
5. Endpoint Request：`CreateUserRequest`、`UpdateUserRequest` 等。
6. Command record：`CreateUserCommand`、`UpdateUserCommand`，以及审批入口的 `CreateUserVariables`。
7. Excel 字段（如需）：`UserExcelColumns.cs`、`UserImportRowDto.cs`、`UserImportParsing.cs` 和工作簿生成。
8. 员工档案同步（如语义相关）：`UserChangedSyncEmployeeProfileNotificationHandler.cs`。

前端：

9. `src/frontend/apps/admin-antd/src/api/system/user.ts` 中 `SystemUser` 类型与请求体类型。
10. `form.vue`、`data.ts` 的表单 schema、回显、列展示。

跨场景核查：

- 列表接口、详情接口、导出接口是否都需要返回该字段。
- 创建请求、更新请求、审批变量是否字段一致（直接创建和审批创建都走 `CreateUserCommand`）。
- 字段是否会参与登录、权限、导入、员工档案同步或其他业务模块选择器（搜索 `getUserList` 引用）。
