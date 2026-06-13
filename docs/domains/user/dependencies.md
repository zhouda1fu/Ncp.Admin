# 用户模块依赖关系

> 判断“改用户模块会影响谁”时看本文件。这里记录跨模块依赖、公共能力和重点风险。

## 依赖总览

| 关联模块 | 关联点 | 主要文件 |
| --- | --- | --- |
| 角色模块 | 用户分配角色、登录权限码、角色名称冗余同步 | `RoleQuery.cs`、`RoleCommands/`、`RoleInfoChangedDomainEventHandlerForUpdateUserRoleName.cs` |
| 部门模块 | 用户所属部门、部门负责人、数据权限、部门名称冗余同步 | `DeptQuery.cs`、`DeptCommands/`、`DeptInfoChangedDomainEventHandlerForUpdateUserDeptName.cs` |
| 岗位模块 | 用户所属岗位、岗位名称冗余同步 | `PositionInfoChangedDomainEventHandlerForUpdateUserPositionName.cs` |
| 员工档案 | 用户创建/编辑后自动创建或更新员工档案，离职状态同步 | `UserChangedSyncEmployeeProfileNotificationHandler.cs` |
| 工作流 | 新增用户审批通过后创建用户 | `WorkflowInstanceCompletedDomainEventHandlerForCreateUser.cs`、前端 `form.vue` |
| 订餐 | 用户字段 `NotOrderMeal`、`OrderMealSort` 影响订餐排序/是否订餐 | `User.cs`、订餐模块查询用户时需关注 |
| 客户、售后、合同、考勤、请假、证书等业务模块 | 多处使用 `getUserList` 作为人员选择源 | 前端各业务页面中 `import { getUserList } from '#/api/system/user'` |
| 数据权限 | 登录 JWT 写入部门和数据范围，服务端中间件读取 | `LoginEndpoint.cs`、`DataPermissionContextMiddleware.cs`、`DataPermissionContextExtensions.cs` |

## 角色模块

用户与角色通过 `UserRole` 关联。用户侧冗余 `RoleName`，用于展示和查询。

重点文件：

- `src/Ncp.Admin.Web/Application/Queries/RoleQuery.cs`
- `src/Ncp.Admin.Web/Application/Commands/Identity/Admin/UserCommands/UpdateUserRolesCommand.cs`
- `src/Ncp.Admin.Web/Application/DomainEventHandlers/RoleInfoChangedDomainEventHandlerForUpdateUserRoleName.cs`
- `src/Ncp.Admin.Infrastructure/Repositories/UserRepository.cs`

注意点：

- `User.UpdateRoles` 按 `RoleId` 做增删，不主动更新已有角色名。
- 角色名称变更要依赖事件处理器批量同步用户侧 `RoleName`。
- 登录权限码来自用户角色，再通过角色查询得到权限集合。

## 部门模块

用户通过 `UserDept` 关联部门。部门负责人改由部门模块的 `dept_responsible_user` 维护，用户模块不再承载主管标记。

重点文件：

- `src/Ncp.Admin.Web/Application/Queries/DeptQuery.cs`
- `src/Ncp.Admin.Web/Application/Commands/Identity/Admin/DeptCommands/`
- `src/Ncp.Admin.Web/Application/DomainEventHandlers/DeptInfoChangedDomainEventHandlerForUpdateUserDeptName.cs`

注意点：

- 部门管理页维护部门负责人，支持一个部门配置多人负责人。
- `User.AssignDept` 只维护所属部门和冗余部门名称，不再触发主管同步事件。
- 用户离职或删除后，会清理其作为部门负责人的关联。
- 登录数据权限依赖用户所属部门。

## 岗位模块

用户通过 `UserPosition` 关联岗位，并冗余 `PositionName`。

重点文件：

- `src/Ncp.Admin.Web/Application/Queries/PositionQuery.cs`
- `src/Ncp.Admin.Web/Application/DomainEventHandlers/PositionInfoChangedDomainEventHandlerForUpdateUserPositionName.cs`
- `src/Ncp.Admin.Infrastructure/Repositories/UserRepository.cs`

注意点：

- 编辑用户时如果岗位为空，后端会清除岗位。
- 导入用户时岗位名称可能重名，需要结合部门限定。
- 岗位名称变更后需要同步用户侧冗余名称。

## 员工档案

用户创建和编辑后会发布 `UserChangedSyncEmployeeProfileNotification`，由员工档案处理器创建或更新员工档案。

重点文件：

- `src/Ncp.Admin.Web/Application/DomainEventHandlers/UserChangedSyncEmployeeProfileNotificationHandler.cs`
- `src/Ncp.Admin.Domain/AggregatesModel/PersonnelAggregate/`

同步字段包括：

- 工号/用户名、真实姓名、手机号、性别、生日。
- 部门名称、岗位名称。
- 身份证、地址、学历、毕业院校、微信号。
- 离职状态和离职时间。

注意点：

- 用户生日为默认值时，员工档案侧会把 1900 年以前视为未填写。
- 用户离职会把员工档案标记为离职，否则标记为在职。
- 修改用户字段语义时，确认员工档案同步是否仍然正确。

## 工作流

新增用户可以走审批。前端把用户表单序列化到工作流变量，后端在流程完成后复用 `CreateUserCommand` 创建用户。

重点文件：

- `src/frontend/apps/admin-antd/src/views/system/user/form.vue`
- `src/Ncp.Admin.Web/Application/DomainEventHandlers/WorkflowInstanceCompletedDomainEventHandlerForCreateUser.cs`
- `src/Ncp.Admin.Web/Application/Commands/Identity/Admin/UserCommands/CreateUserCommand.cs`

注意点：

- 用户字段新增或改名时，要同步前端 `variables` 和后端 `CreateUserVariables`。
- 审批创建和直接创建最终都走 `CreateUserCommand`，但前端入口不同。
- 审批失败会将工作流实例标记为 faulted。

## 数据权限

登录时会根据用户角色和部门生成 JWT Claims：

- `data_scope`
- `dept_id`
- `authorized_dept_ids`

重点文件：

- `src/Ncp.Admin.Web/Endpoints/Identity/Admin/UserEndpoints/LoginEndpoint.cs`
- `src/Ncp.Admin.Web/Middleware/DataPermissionContextMiddleware.cs`
- `src/Ncp.Admin.Web/Extensions/DataPermissionContextExtensions.cs`
- `src/Ncp.Admin.Web/Services/JwtDataPermissionClaimTypes.cs`

注意点：

- 改用户部门会影响登录后的数据可见范围。
- 改角色数据权限会影响 `authorized_dept_ids` 计算。
- 客户公海、工作流待办等模块可能间接依赖这些数据权限结果。

## 前端业务模块中的用户选择器

多个业务页面直接复用 `getUserList` 作为人员选择源。修改列表筛选、返回字段、离职过滤、分页契约时，要搜索：

```powershell
rg "getUserList" src/frontend/apps/admin-antd/src
```

当前已知涉及：

- 聊天
- 售后
- 考勤排班
- 证书
- 合同
- 客户公海
- 请假余额
- 订单
- 工作流
- 片区项目

## 风险清单

- 用户状态、离职、软删除三个概念要分清：`Status`、`IsResigned`、`IsDeleted`。
- 用户详情缓存需要在写操作后失效。
- 角色、部门、岗位名称是冗余字段，依赖同步逻辑。
- 用户模块字段变更可能影响员工档案和 Excel 导入导出。
- 直接创建和审批创建都要覆盖。
- 用户列表字段缺失会影响状态开关这种“整包更新”操作。
- 数据权限依赖用户部门和角色，改动可能扩散到客户、工作流等模块。
