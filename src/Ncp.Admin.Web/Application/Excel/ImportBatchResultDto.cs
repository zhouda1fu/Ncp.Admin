namespace Ncp.Admin.Web.Application.Excel;

/// <summary>
/// 批量导入中单行失败信息（行号为 Excel 数据行号，含表头则从 2 开始）。
/// </summary>
public record ImportRowErrorDto(int RowNumber, string Message);

/// <summary>
/// 批量导入统一结果结构，供各业务模块复用。
/// </summary>
public record ImportBatchResultDto(int SuccessCount, IReadOnlyList<ImportRowErrorDto> Errors);
