# 用户模块业务流程

> 修改用户行为时优先看本文件。接口路径、DTO 和字段契约见 `api.md`；跨模块影响见 `dependencies.md`。

## 创建用户

前端 `form.vue` 在新建模式下调用 `createUser`。字段 schema 来自 `data.ts`，角色选项来自 `getRoleList`，部门选项来自 `getDeptTree`，头像通过文件上传后把路径写入 `avatarUrl`。

后端 `CreateUserEndpoint`：

1. 从登录 Claims 读取 `creatorId`。
2. 用 `RoleQuery.GetAdminRolesForAssignmentAsync` 将 `RoleId` 转为角色快照。
3. 发送 `CreateUserCommand`。

`CreateUserCommandHandler`：

1. 校验用户名必填、密码必填、用户名不重复。
2. 使用 `IPasswordHasher` 生成密码哈希。
3. 创建 `User` 聚合并写入 `UserRole`。
4. 可选分配部门 `AssignDept`，可选分配岗位 `AssignPosition`。
5. 发布 `UserChangedSyncEmployeeProfileNotification`，同步员工档案。

## 编辑用户

前端 `form.vue` 编辑模式通过 `getUser` 回显用户详情，并根据角色名称反查角色 ID。保存时：

1. 调用 `updateUser` 更新基础信息、部门、岗位、离职、头像等字段。
2. 如果表单里有 `roleIds`，再调用 `updateUserRoles` 更新角色。

后端 `UpdateUserCommandHandler`：

1. 读取用户聚合，不存在则抛 `UserNotFound`。
2. 调用 `UpdateUserInfo` 更新基础字段。
3. 密码非空才更新密码。
4. 按请求分配部门，岗位为空则清除岗位。
5. 发布员工档案同步通知。
6. 清理 `UserQuery` 用户详情缓存。

## 启用/禁用

前端列表页状态开关走 `onStatusChange`，确认后复用 `updateUser` 接口更新 `Status`。

风险点：它会用当前行数据组装更新请求，因此新增字段时要确认列表 DTO 是否包含该字段，否则可能在切换状态时被空值覆盖。

## 离职与删除

`User.IsResigned` 表示离职，`User.IsDeleted` 表示软删除。两者分别发布不同领域事件，但共用同一套联动处理器（清部门负责人、客户协作订单移交）：

- `UpdateUserInfo` 中由非离职首次变为离职时添加 `UserResignedDomainEvent`（避免已离职用户每次保存重复派发）。
- `SoftDelete` 添加 `UserSoftDeletedDomainEvent`。
- 事件处理器会清理用户作为部门负责人的关联。
- 用户变更通知会把员工档案标记为离职或在职。

查询侧多处排除离职用户，例如登录查询、根据部门取用户、用户详情缓存查询。修改离职逻辑时要确认哪些场景应该展示离职用户，哪些场景应该排除。

## 登录和权限

登录入口在 `LoginEndpoint.cs`：

1. 通过 `UserQuery.GetUserInfoForLoginAsync` 按用户名查用户，排除离职用户。
2. 校验密码哈希。
3. 从用户角色读取权限码和数据权限范围。
4. 写入 JWT Claims（具体字段清单见 `api.md` 的[登录 JWT 契约](./api.md#登录-jwt-契约)）。
5. 生成 access token 和 refresh token。
6. 发送 `UpdateUserLoginTimeCommand` 更新最后登录时间、IP 和刷新令牌。

权限码后端定义在 `PermissionCodes.cs`，权限树定义在 `PermissionDefinitionContext.cs`，前端常量在 `permission-codes.ts`。新增或改名权限码时必须前后端同步。

## 新增用户审批

前端 `form.vue` 的“提交审批”仅新建模式显示：

1. 查找已发布流程定义，分类为 `CreateUser`。
2. 将用户表单序列化为 JSON `variables`。
3. 调用 `startWorkflow`，业务类型为 `CreateUser`。

后端 `WorkflowInstanceCompletedDomainEventHandlerForCreateUser.cs` 在流程完成后：

1. 判断 `BusinessType == CreateUser`。
2. 反序列化 `CreateUserVariables`。
3. 转换角色、部门、岗位 ID。
4. 复用 `CreateUserCommand` 创建用户。
5. 失败时将工作流实例标记为 faulted。

## 导入用户

前端列表页通过 `importUsersExcel` 上传 Excel。后端 `ImportUsersEndpoint` 解析文件后发送 `ImportUsersCommand`。

`ImportUsersCommandHandler` 逐行构建 `CreateUserCommand`：

- 校验用户名、邮箱、初始密码、状态、出生日期、离职时间。
- 按部门名称查部门，重名或不存在会返回行级错误。
- 按岗位名称查岗位，存在同名岗位时需要部门限定。
- 按角色名称查角色，缺失角色会返回行级错误。
- 每行最终复用创建用户链路，因此创建用户新增校验会影响导入。

## 导出用户

前端列表页通过 `exportUsersExcel` 使用当前筛选条件导出。后端查询使用 `UserQuery.GetUsersForExportAsync`，和列表保持同一套筛选逻辑。

导出最大行数由 `UserQuery.UserExportMaxRows` 控制，目前为 50,000 行。超过上限会抛业务异常，提示缩小筛选条件。

## 常见修改点

### 新增用户字段

详见 `api.md` 的[新增/修改用户字段时的全栈检查清单](./api.md#新增修改用户字段时的全栈检查清单)。该清单覆盖领域、EF、查询、命令、前端表单、Excel、员工档案同步等位置。

### 修改角色分配

重点看：

- 前端 `form.vue` 编辑时通过角色名称反查角色 ID。
- `updateUserRoles` 与 `UpdateUserRolesCommand`。
- `User.UpdateRoles` 只按 `RoleId` 做增删，不主动更新已存在角色的 `RoleName`。
- 角色名称变更依赖 `RoleInfoChangedDomainEventHandlerForUpdateUserRoleName` 做冗余同步。

### 修改部门负责人逻辑

重点看：

- 用户表单中的 `deptId`、`deptName` 只维护用户所属部门。
- 部门负责人由部门管理页维护，接口字段为 `responsibleUserIds` 和 `defaultResponsibleUserId`。
- 新增用户表单可勾选“设为部门负责人/默认负责人”作为快捷入口；用户聚合发布负责人追加请求事件，事件处理器发送部门命令，最终仍由部门聚合写入 `dept_responsible_user`，不把负责人关系重新放回用户模型。
- 工作流中的部门负责人审批、部门负责人链均从 `dept_responsible_user` 解析。
- 用户离职或删除后会清理其作为部门负责人的关联。
