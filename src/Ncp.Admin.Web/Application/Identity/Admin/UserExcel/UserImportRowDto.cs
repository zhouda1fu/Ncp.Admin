namespace Ncp.Admin.Web.Application.Identity.Admin.UserExcel;

/// <summary>
/// 从 Excel 解析出的单行用户导入数据（均为原始字符串，由命令处理器再校验与转换）。
/// </summary>
public record UserImportRowDto(
    int RowNumber,
    string Name,
    string Email,
    string Password,
    string Phone,
    string RealName,
    string Status,
    string Gender,
    string BirthDate,
    string DeptName,
    string IsDeptManager,
    string PositionName,
    string Roles,
    string IdCardNumber,
    string Address,
    string Education,
    string GraduateSchool,
    string AvatarUrl,
    string NotOrderMeal,
    string OrderMealSort,
    string WechatGuid,
    string IsResigned,
    string ResignedTime);
