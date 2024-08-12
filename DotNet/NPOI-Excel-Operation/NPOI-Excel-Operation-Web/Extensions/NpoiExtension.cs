using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace NPOI_Excel_Operation_Web.Extensions;

public static class NpoiExtension
{
    /// <summary>
    /// Excel 2003 中最大行号
    /// </summary>
    public const int MaxRowIndex2003 = 65535;

    /// <summary>
    /// Excel 2007+ 中最大行号
    /// </summary>
    public const int MaxRowIndex2007 = 1048575;

    /// <summary>
    /// 输出下拉数据源
    /// </summary>
    /// <param name="workbook">工作簿</param>
    /// <param name="sheetName">下拉数据源所处的Sheet名称</param>
    /// <param name="dropdownDataSource">下拉数据源</param>
    /// <param name="columnIndex">输出到数据源所处Sheet的第几列（从0开始）</param>
    /// <param name="nameName">Excel名称管理器中的名称（不允许有特殊字符，如中英文括号等，只允许中文、英文、数字、英文下划线，不能以数字开头）</param>
    public static void WriteDropDownDataSource(this IWorkbook workbook, string sheetName, List<string> dropdownDataSource, int columnIndex, string nameName)
    {
        //先创建一个Sheet专门用于存储下拉项的值
        ISheet sheet = workbook.GetSheet(sheetName);
        sheet ??= workbook.CreateSheet(sheetName);

        //隐藏sheet
        workbook.SetSheetHidden(workbook.GetSheetIndex(sheet), SheetState.Hidden);
        /*
         ** 输出格式如下：第一列为省、第二开始为各省下级市
        江苏省	南京市	合肥市	广州市
        安徽省	苏州市	宿州市	深圳市
        广东省
        */
        if (dropdownDataSource.Any())
        {
            int rowIndex = 0;
            foreach (var context in dropdownDataSource)
            {
                IRow row = sheet.GetRow(rowIndex);
                row ??= sheet.CreateRow(rowIndex);

                rowIndex++;
                ICell cell = row.CreateCell(columnIndex);
                cell.SetCellValue(context);
            }

            IName range = workbook.CreateName();
            // 方式一：使用CellRangeAddress，创建范围 格式：Sheet1!A1:A4
            var cellRange = new CellRangeAddress(0, dropdownDataSource.Count() - 1, columnIndex, columnIndex);
            var referAsString = $"{sheet.SheetName}!{cellRange.FormatAsString()}";
            range.RefersToFormula = referAsString;

            // 方式二：根据列，计算出在Excel中的列名 格式：Sheet1!$A$1:$A$3
            // string colName = GetExcelColumnName(columnIndex); //列数转为ABC等格式
            // var refers = sheet.SheetName + "!$" + colName + "$1:$" + colName + "$" + dropdownDataSource.Count() + "";
            // range.RefersToFormula = refers;

            range.NameName = nameName;
        }
    }

    public static void SetDropdownListByName(this ISheet sheet, int firstRow, int lastRow, int firstColumn, int lastColumn, string nameName)
    {
        CellRangeAddressList regions = new CellRangeAddressList(firstRow, lastRow, firstColumn, lastColumn);
        IDataValidation dataValidate;
        if (sheet is HSSFSheet)
        {
            DVConstraint constraint = DVConstraint.CreateFormulaListConstraint(nameName);
            dataValidate = new HSSFDataValidation(regions, constraint);
        }
        else
        {
            var dvHelper = sheet.GetDataValidationHelper();
            var constraint = dvHelper.CreateFormulaListConstraint(nameName);
            dataValidate = dvHelper.CreateValidation(constraint, regions);
        }

        dataValidate.CreateErrorBox("输入不合法", "请输入或选择下拉列表中的值。");
        sheet.AddValidationData(dataValidate);
    }

    public static string GetFileName(this IWorkbook workBook, string fileName)
    {
        if (workBook is HSSFWorkbook)
        {
            return $"{fileName}.xls";
        }

        return $"{fileName}.xlsx";
    }

    /// <summary>
    /// 保存Excel
    /// </summary>
    /// <param name="workBook"></param>
    /// <param name="fileFullPath"></param>
    public static void SaveToLocalFile(this IWorkbook workBook, string fileFullPath)
    {
        ArgumentNullException.ThrowIfNull(fileFullPath);

        if (workBook is HSSFWorkbook)
        {
            fileFullPath = Path.ChangeExtension(fileFullPath, "xls");
        }
        else
        {
            fileFullPath = Path.ChangeExtension(fileFullPath, "xlsx");
        }

        var directory = Path.GetDirectoryName(fileFullPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        using (var fs = File.Create(fileFullPath))
        {
            workBook.Write(fs);
        }
    }

    public static byte[] SaveToBytes(this IWorkbook workbook)
    {
        byte[] bytes = null;
        using (var ms = new MemoryStream())
        {
            workbook.Write(ms);
            bytes = ms.ToArray();
        }

        return bytes;
    }

    public static int GetSheetMaxRowCount(this ISheet sheet)
    {
        if (sheet is HSSFSheet)
        {
            return MaxRowIndex2003;
        }

        return MaxRowIndex2007;
    }
}