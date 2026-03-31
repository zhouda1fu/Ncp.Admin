using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 条件字段可选值（用于前端下拉，value 须与工作流 Variables JSON 中实际类型一致）
/// </summary>
public record ConditionFieldOptionDto(string Value, string Label);

/// <summary>
/// 条件字段定义 DTO（按流程分类返回可用字段，供前端结构化条件表单使用）
/// </summary>
/// <param name="Key">与 Variables JSON 属性名一致（PascalCase）</param>
/// <param name="Label">展示名称</param>
/// <param name="Type">number | string | boolean | enum（enum 表示必须用 options 选择）</param>
/// <param name="Options">有值时前端应使用下拉框而非自由输入</param>
public record ConditionFieldDto(
    string Key,
    string Label,
    string Type,
    IReadOnlyList<ConditionFieldOptionDto>? Options = null);

/// <summary>
/// 按流程分类返回可用于条件分支的字段定义（供前端结构化条件表单使用）
/// </summary>
public static class WorkflowConditionFieldsProvider
{
    /// <summary>
    /// 根据流程分类返回条件字段列表。仅支持用户管理、订单审批。
    /// </summary>
    public static List<ConditionFieldDto> GetFields(string category)
    {
        return category switch
        {
            WorkflowBusinessTypes.Order => GetOrderFields(),
            WorkflowBusinessTypes.CreateUser => GetCreateUserFields(),
            _ => []
        };
    }

    /// <summary>
    /// 订单审批条件分支：到款情况、布尔项提供下拉选项（与 JSON 中枚举数值、布尔字符串一致）
    /// </summary>
    private static List<ConditionFieldDto> GetOrderFields()
    {
        return
        [
            new ConditionFieldDto(
                "PaymentStatus",
                "到款情况",
                "enum",
                [
                    new ConditionFieldOptionDto(((int)PaymentStatus.FullPayment).ToString(), "已到全款"),
                    new ConditionFieldOptionDto(((int)PaymentStatus.PartialPayment).ToString(), "未到全款"),
                    new ConditionFieldOptionDto(((int)PaymentStatus.InstallmentUrgent).ToString(), "有分期未到全款加急发货"),
                    new ConditionFieldOptionDto(((int)PaymentStatus.PendingConfirmation).ToString(), "待确认"),
                ]),
            new ConditionFieldDto(
                "ContractNotCompanyTemplate",
                "合同非公司模板",
                "boolean",
                [
                    new ConditionFieldOptionDto("true", "是"),
                    new ConditionFieldOptionDto("false", "否"),
                ]),
        ];
    }

    private static List<ConditionFieldDto> GetCreateUserFields()
    {
        return
        [
            new ConditionFieldDto("Name", "用户名", "string"),
            new ConditionFieldDto("Email", "邮箱", "string"),
            new ConditionFieldDto("RealName", "真实姓名", "string"),
            new ConditionFieldDto("Phone", "手机号", "string"),
            new ConditionFieldDto("Status", "状态", "number"),
            new ConditionFieldDto("Gender", "性别", "string"),
            new ConditionFieldDto("DeptId", "部门ID", "string"),
            new ConditionFieldDto("DeptName", "部门名称", "string"),
        ];
    }
}
