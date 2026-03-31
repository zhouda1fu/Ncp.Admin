using System.Globalization;
using ClosedXML.Excel;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Identity.Admin.UserExcel;

/// <summary>
/// 用户 Excel 模板、导出、导入解析（ClosedXML）。
/// </summary>
public static class UserExcelWorkbook
{
    private const int TitleRowHeight = 32;
    private const int HeaderRowHeight = 26;
    private const double DefaultRowHeight = 18;

    /// <summary>导入模板列宽（与 <see cref="UserExcelColumns.ImportTemplateHeaders"/> 顺序一致）。</summary>
    private static readonly double[] ImportColumnWidths =
    [
        12, 26, 12, 14, 12, 8, 8, 14,
        16, 12, 14, 22,
        20, 28, 10, 16, 28,
        10, 10, 14, 10, 18
    ];

    /// <summary>导出列宽（与 <see cref="UserExcelColumns.ExportHeaders"/> 顺序一致，首列为用户ID）。</summary>
    private static readonly double[] ExportColumnWidths =
    [
        11, 12, 26, 14, 12, 8, 8, 14,
        16, 12, 14, 22,
        20, 28, 10, 16, 28,
        10, 10, 14, 10, 18
    ];

    public static MemoryStream CreateTemplateStream()
    {
        var ms = new MemoryStream();
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("用户导入");
        sheet.TabColor = XLColor.FromArgb(68, 114, 196);
        var colCount = UserExcelColumns.ImportTemplateHeaders.Length;
        const int headerRow = 2;

        ApplyTitleBanner(sheet, 1, colCount, "用户批量导入模板", "请从第 3 行起填写数据 · 角色多个用英文逗号分隔 · 部门/岗位名称须与系统一致");

        for (var i = 0; i < colCount; i++)
        {
            sheet.Cell(headerRow, i + 1).Value = UserExcelColumns.ImportTemplateHeaders[i];
        }

        ApplyColumnHeaderRow(sheet, headerRow, colCount);
        ApplyColumnWidths(sheet, colCount, ImportColumnWidths);
        ApplyTableBorders(sheet, 1, headerRow, colCount);
        sheet.SheetView.FreezeRows(headerRow);

        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    public static MemoryStream CreateExportStream(IReadOnlyList<UserInfoQueryDto> users)
    {
        var ms = new MemoryStream();
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("用户列表");
        sheet.TabColor = XLColor.FromArgb(68, 114, 196);
        var colCount = UserExcelColumns.ExportHeaders.Length;
        const int headerRow = 2;
        var dataStartRow = headerRow + 1;

        ApplyTitleBanner(
            sheet,
            1,
            colCount,
            "用户数据导出",
            $"导出时间（UTC）：{DateTime.UtcNow:yyyy-MM-dd HH:mm} · 共 {users.Count} 条");

        for (var c = 0; c < colCount; c++)
        {
            sheet.Cell(headerRow, c + 1).Value = UserExcelColumns.ExportHeaders[c];
        }

        ApplyColumnHeaderRow(sheet, headerRow, colCount);
        ApplyColumnWidths(sheet, colCount, ExportColumnWidths);

        for (var r = 0; r < users.Count; r++)
        {
            var u = users[r];
            var row = dataStartRow + r;
            var cells = new[]
            {
                u.UserId.ToString(),
                u.Name,
                u.Email,
                u.Phone,
                u.RealName,
                u.Status.ToString(CultureInfo.InvariantCulture),
                u.Gender,
                u.BirthDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                u.DeptName,
                u.IsDeptManager ? "是" : "否",
                u.PositionName,
                string.Join(',', u.Roles),
                u.IdCardNumber,
                u.Address,
                u.Education,
                u.GraduateSchool,
                u.AvatarUrl,
                u.NotOrderMeal ? "是" : "否",
                u.OrderMealSort.ToString(CultureInfo.InvariantCulture),
                u.WechatGuid,
                u.IsResigned ? "是" : "否",
                u.ResignedTime.HasValue
                    ? u.ResignedTime.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                    : string.Empty,
            };
            for (var c = 0; c < cells.Length; c++)
            {
                var cell = sheet.Cell(row, c + 1);
                cell.Value = cells[c];
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
            }

            sheet.Row(row).Height = DefaultRowHeight;
            if (r % 2 == 1)
            {
                sheet.Range(row, 1, row, colCount).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 249, 251);
            }
        }

        var lastRow = users.Count == 0 ? headerRow : dataStartRow + users.Count - 1;
        ApplyTableBorders(sheet, 1, lastRow, colCount);
        sheet.SheetView.FreezeRows(headerRow);

        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// 解析导入文件；自动定位包含全部必填表头的行（支持第 1 行即为表头，或第 1 行为标题、第 2 行为表头）。
    /// </summary>
    public static IReadOnlyList<UserImportRowDto> ParseImportRows(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);
        var (headerRowNumber, headerMap) = TryFindImportHeaderRow(sheet);
        if (headerMap == null)
        {
            throw new KnownException("Excel 中未找到与模板一致的表头行，请使用「下载导入模板」获取最新模板", ErrorCodes.InvalidExcelFile);
        }

        var rows = new List<UserImportRowDto>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? headerRowNumber;
        for (var r = headerRowNumber + 1; r <= lastRow; r++)
        {
            string G(string header) =>
                sheet.Cell(r, headerMap[header]).GetString().Trim();

            if (UserExcelColumns.ImportTemplateHeaders.All(h => string.IsNullOrWhiteSpace(G(h))))
            {
                continue;
            }

            rows.Add(new UserImportRowDto(
                RowNumber: r,
                Name: G(UserExcelColumns.Name),
                Email: G(UserExcelColumns.Email),
                Password: G(UserExcelColumns.Password),
                Phone: G(UserExcelColumns.Phone),
                RealName: G(UserExcelColumns.RealName),
                Status: G(UserExcelColumns.Status),
                Gender: G(UserExcelColumns.Gender),
                BirthDate: G(UserExcelColumns.BirthDate),
                DeptName: G(UserExcelColumns.DeptName),
                IsDeptManager: G(UserExcelColumns.IsDeptManager),
                PositionName: G(UserExcelColumns.PositionName),
                Roles: G(UserExcelColumns.Roles),
                IdCardNumber: G(UserExcelColumns.IdCardNumber),
                Address: G(UserExcelColumns.Address),
                Education: G(UserExcelColumns.Education),
                GraduateSchool: G(UserExcelColumns.GraduateSchool),
                AvatarUrl: G(UserExcelColumns.AvatarUrl),
                NotOrderMeal: G(UserExcelColumns.NotOrderMeal),
                OrderMealSort: G(UserExcelColumns.OrderMealSort),
                WechatGuid: G(UserExcelColumns.WechatGuid),
                IsResigned: G(UserExcelColumns.IsResigned),
                ResignedTime: G(UserExcelColumns.ResignedTime)));
        }

        return rows;
    }

    private static (int HeaderRowNumber, Dictionary<string, int>? Map) TryFindImportHeaderRow(IXLWorksheet sheet)
    {
        var maxScan = Math.Min(sheet.LastRowUsed()?.RowNumber() ?? 1, 30);
        for (var r = 1; r <= maxScan; r++)
        {
            var row = sheet.Row(r);
            var map = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var cell in row.CellsUsed())
            {
                var text = cell.GetString().Trim();
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                map[text] = cell.Address.ColumnNumber;
            }

            if (UserExcelColumns.ImportTemplateHeaders.All(h => map.ContainsKey(h)))
            {
                return (r, map);
            }
        }

        return (0, null);
    }

    /// <summary>顶部标题区：主标题 + 副标题（副标题为合并区域内换行）。</summary>
    private static void ApplyTitleBanner(IXLWorksheet sheet, int rowIndex, int colCount, string title, string? subtitle = null)
    {
        var range = sheet.Range(rowIndex, 1, rowIndex, colCount);
        range.Merge();
        var cell = range.FirstCell();
        cell.Value = string.IsNullOrEmpty(subtitle) ? title : $"{title}\n{subtitle}";
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 14;
        cell.Style.Font.FontColor = XLColor.White;
        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 121);
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
        sheet.Row(rowIndex).Height = string.IsNullOrEmpty(subtitle) ? TitleRowHeight : 46;
    }

    private static void ApplyColumnHeaderRow(IXLWorksheet sheet, int rowIndex, int colCount)
    {
        sheet.Row(rowIndex).Height = HeaderRowHeight;
        for (var c = 1; c <= colCount; c++)
        {
            var cell = sheet.Cell(rowIndex, c);
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(68, 114, 196);
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;
        }
    }

    private static void ApplyColumnWidths(IXLWorksheet sheet, int colCount, double[] widths)
    {
        for (var i = 0; i < colCount && i < widths.Length; i++)
        {
            sheet.Column(i + 1).Width = widths[i];
        }
    }

    private static void ApplyTableBorders(IXLWorksheet sheet, int rowFirst, int rowLast, int colCount)
    {
        if (rowLast < rowFirst)
        {
            return;
        }

        var range = sheet.Range(rowFirst, 1, rowLast, colCount);
        range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.TopBorderColor = XLColor.FromArgb(180, 198, 231);
        range.Style.Border.BottomBorderColor = XLColor.FromArgb(180, 198, 231);
        range.Style.Border.LeftBorderColor = XLColor.FromArgb(180, 198, 231);
        range.Style.Border.RightBorderColor = XLColor.FromArgb(180, 198, 231);
        range.Style.Border.InsideBorderColor = XLColor.FromArgb(217, 225, 242);
    }
}
