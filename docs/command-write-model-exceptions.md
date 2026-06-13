# 命令写模型例外清单

本清单记录暂不按“命令处理器不显式 `SaveChangesAsync` / 不显式 `UpdateAsync`”立即改造的场景。后续改造时应优先封装应用服务或拆分为聚合行为 + 领域事件处理器。

| 位置 | 例外类型 | 原因与边界 | 后续治理方向 |
| --- | --- | --- | --- |
| `Application/Commands/Personnel/ImportEmployeeProfilesCommand.cs` | 批量导入 | 导入员工档案时需要同步号码池绑定关系；跨聚合编排保留在命令内，已去除显式 `UpdateAsync`。 | 拆为员工导入服务与号码池绑定领域事件，逐步减少命令内跨聚合写入。 |
| `Application/DomainEventHandlers/UserResignedOrDeletedDomainEventHandlerForSyncEmployeeProfile.cs` | 事件同步 | 用户离职/删除事件驱动员工档案同步，当前处理器直接落库以保证事件副作用及时完成。 | 改为发送员工档案同步命令，由命令驱动聚合行为。 |
| `Application/Services/Attendance/RaV1AttendanceSyncService.cs` | 外部同步 | 外部考勤同步服务直接批量写入，属于集成同步边界。 | 保持应用服务边界，补充分批、幂等和失败补偿说明。 |
| `Application/Services/Notification/WeChatBindingService.cs` | 外部绑定服务 | 企业微信绑定/解绑服务需要即时持久化绑定状态，当前作为应用服务直接操作 DbContext。 | 后续引入绑定聚合或命令，服务只负责外部协议适配。 |

## 已治理（2026-06）

以下项已完成 CleanDDD 对齐，自本清单移除：

- `BulkGenerateSchedulesCommand` → `IDefaultScheduleRepository`
- `RecalculateCustomerSeaVisibilityBatchCommand` → 分块子命令 + `ICustomerSeaVisibilityBoardRepository`
- `ImportProductsCommand` → 依赖 UnitOfWork，去除显式 SaveChanges
- `ImportInterviewRecordsCommand` → 去除显式 UpdateAsync
- `UpdateContractInstallmentsCommand` → `IContractInstallmentPlanRepository`
- `SetLeaveBalanceBatchCommand` → `ILeaveBalanceAdjustmentChangeLogRepository`
- `DeleteGeneralCertificateCommand` → 聚合 `Delete()` + 领域事件清理通知
- `DeleteDomainCertificateCommand` → 领域事件清理通知
- `SaveUserCalendarMemoCommand` → `IUserCalendarMemoRepository`
- `ClearUserAsDeptResponsibleUserCommand` → `IDeptRepository` + 部门聚合 `SetResponsibleUser`
- `SaveOfficeTaskCustomizationRoleMembersCommand` → `IOfficeTaskCustomizationRoleMemberRepository`
- `ContractShareCommands` 写路径 → `IContractShareRepository`
- `SaveUserHomeDashboardLayoutCommand` → `IUserHomeDashboardPreferenceRepository`
- `ApplyApprovedCustomerCollaborationCommand` → 编排子命令；转交级联改由 `CustomerOwnershipTransferredForCollaborationApprovalDomainEvent` 处理器驱动
- `AssignCustomerSeaRegionsCommand` / `PropagateCustomerSeaRegionsToSupervisorsCommand` → 片区/用户/区域读路径收拢至仓储
- `ContractShareCommands` → `IContractRepository.AllExistAsync` + `IUserRepository.TryGetUserDeptAsync`
