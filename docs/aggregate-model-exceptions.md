# 聚合模型补强例外清单

第四批只对明确承载业务状态流转、删除或并发冲突风险的聚合补强 `Deleted/IsDeleted`、`RowVersion` 与领域事件。以下类型暂不机械补字段或事件。

| 类型类别 | 代表聚合 | 例外原因 | 后续边界 |
| --- | --- | --- | --- |
| 配置/字典表 | `ProjectType`、`ProjectIndustry`、`ProjectStatusOption`、`Region`、`Industry`、`MeetingTypeOption`、`ContractTypeOption`、`IncomeExpenseTypeOption`、`OrderLogisticsCompany`、`OrderLogisticsMethod`、`TrainingType` | 多数为基础资料维护，删除语义与业务聚合不同，部分已由查询/权限约束控制。 | 仅当出现并发编辑或软删除恢复诉求时补 `RowVersion` / `Deleted`。 |
| 批量导入或同步明细 | `AttendanceRecord`、`DefaultSchedule`、`Schedule`、`OperationLog`、`SoftwareLicenseOperationLog` | 主要由外部同步或日志写入产生，通常是追加或批处理，软删除/领域事件价值低。 | 保持应用服务/批处理边界，避免为日志类强行发布领域事件。 |
| 共享主键或偏读模型状态 | `CustomerSeaVisibilityBoard`、`LeaveBalanceSnapshot`、`UserHomeDashboardPreference`、`UserCalendarMemo`、`EvaluateReadRecord`、`OfficeTaskReplyReadRecord` | 这些类型偏读模型、用户偏好或读取状态，删除/并发语义不等同业务聚合。 | 如出现用户侧覆盖冲突，再按具体业务行为补并发控制。 |
| 聚合内部或历史兼容模型 | `ContractShare`、`ContractInstallmentPlan`、`ProductParameter`、`ProductTrain` | 与主业务聚合关系紧密，当前接口更多是主流程辅助数据。 | 后续若拆成独立生命周期，再补领域事件与软删除。 |
| 本批已补强业务聚合 | `Project`、`PersonnelBenefitApplication`、`AfterSalesServiceTechnologyApplication` | 有明确状态流转、删除或审批协作场景。 | 已补 `RowVersion`；`Project` 已由硬删除改为软删除并发布删除事件。 |
