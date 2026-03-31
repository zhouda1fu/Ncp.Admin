namespace Ncp.Admin.Web.Application.Identity.Admin.UserExcel;

/// <summary>
/// 用户导入导出表头（中文，与模板一致）。
/// </summary>
public static class UserExcelColumns
{
    public const string UserId = "用户ID";
    public const string Name = "用户名";
    public const string Email = "邮箱";
    public const string Password = "初始密码";
    public const string Phone = "手机";
    public const string RealName = "真实姓名";
    public const string Status = "状态";
    public const string Gender = "性别";
    public const string BirthDate = "出生日期";
    public const string DeptName = "部门名称";
    public const string IsDeptManager = "部门主管";
    public const string PositionName = "岗位名称";
    public const string Roles = "角色";
    public const string IdCardNumber = "身份证";
    public const string Address = "地址";
    public const string Education = "学历";
    public const string GraduateSchool = "毕业院校";
    public const string AvatarUrl = "头像地址";
    public const string NotOrderMeal = "不订餐";
    public const string OrderMealSort = "订餐排序";
    public const string WechatGuid = "微信唯一码";
    public const string IsResigned = "是否离职";
    public const string ResignedTime = "离职时间";

    /// <summary>导出表头顺序（含用户ID）。</summary>
    public static readonly string[] ExportHeaders =
    [
        UserId, Name, Email, Phone, RealName, Status, Gender, BirthDate,
        DeptName, IsDeptManager, PositionName, Roles,
        IdCardNumber, Address, Education, GraduateSchool, AvatarUrl,
        NotOrderMeal, OrderMealSort, WechatGuid, IsResigned, ResignedTime
    ];

    /// <summary>导入模板表头（无用户ID）。</summary>
    public static readonly string[] ImportTemplateHeaders =
    [
        Name, Email, Password, Phone, RealName, Status, Gender, BirthDate,
        DeptName, IsDeptManager, PositionName, Roles,
        IdCardNumber, Address, Education, GraduateSchool, AvatarUrl,
        NotOrderMeal, OrderMealSort, WechatGuid, IsResigned, ResignedTime
    ];
}
