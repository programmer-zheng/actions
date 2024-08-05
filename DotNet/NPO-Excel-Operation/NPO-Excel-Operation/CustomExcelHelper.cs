using System.Drawing;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace NPO_Excel_Operation;

public static class CustomExcelHelper
{
    /// <summary>
    /// 为单元格设置下拉（不额外创建Sheet）
    /// 如果firstcol为0，lastcol为1，则0、1两列都设置下拉 
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="firstcol"></param>
    /// <param name="lastcol"></param>
    /// <param name="vals">下拉的选项</param>
    public static void SetCellDropdownListDirect(ISheet sheet, int firstcol, int lastcol, string[] vals)
    {
        IDataValidation dataValidate;
        //设置生成下拉框的行和列
        var cellRegions = new CellRangeAddressList(1, 65535, firstcol, lastcol);
        if (sheet is XSSFSheet)
        {
            var dvHelper = sheet.GetDataValidationHelper();
            var constraint = dvHelper.CreateExplicitListConstraint(vals);

            dataValidate = dvHelper.CreateValidation(constraint, cellRegions);
        }
        else //if (sheet is HSSFSheet)
        {
            // 2003 设置 下拉框内容
            DVConstraint constraint = DVConstraint.CreateExplicitListConstraint(vals);

            //绑定下拉框和作用区域，并设置错误提示信息
            dataValidate = new HSSFDataValidation(cellRegions, constraint);
        }

        dataValidate.CreateErrorBox("输入不合法", "请输入或选择下拉列表中的值。");
        dataValidate.ShowPromptBox = true;

        sheet.AddValidationData(dataValidate);
    }


    public static void SetCellDropdownList(IWorkbook workbook, ISheet sheet, string sheetName, int firstcol, int lastcol, string[] vals, int sheetindex = 1)
    {
        ISheet hideSheet = workbook.GetSheet(sheetName);
        //先创建一个Sheet专门用于存储下拉项的值
        if (hideSheet == null)
        {
            hideSheet = workbook.CreateSheet(sheetName);
        }

        //隐藏
        workbook.SetSheetHidden(sheetindex, SheetState.Hidden);
        int index = 0;
        foreach (var item in vals)
        {
            hideSheet.CreateRow(index).CreateCell(0).SetCellValue(item);
            index++;
        }

        //创建的下拉项的区域：
        var rangeName = sheetName + "Range"+Guid.NewGuid().ToString("N");
        IName range = workbook.CreateName();
        range.RefersToFormula = sheetName + "!$A$1:$A$" + index;
        range.NameName = rangeName;
        IDataValidation dataValidate;
        if (workbook is HSSFWorkbook)
        {
            CellRangeAddressList regions = new CellRangeAddressList(0, 65535, firstcol, lastcol);

            DVConstraint constraint = DVConstraint.CreateFormulaListConstraint(rangeName);
            dataValidate = new HSSFDataValidation(regions, constraint);
        }
        else
        {
            CellRangeAddressList regions = new CellRangeAddressList(0, 65535, firstcol, lastcol);
            var dvHelper = sheet.GetDataValidationHelper();
            var constraint = dvHelper.CreateFormulaListConstraint(rangeName);

            dataValidate = dvHelper.CreateValidation(constraint, regions);
        }

        dataValidate.CreateErrorBox("输入不合法", "请输入或选择下拉列表中的值。");
        dataValidate.ShowPromptBox = true;
        sheet.AddValidationData(dataValidate);
    }

    /// <summary>
    /// 设置背景颜色
    /// </summary>
    /// <param name="cellStyle"></param>
    /// <param name="color"></param>
    public static void SetBackgroundColor(ICellStyle cellStyle, Color color)
    {
        if (cellStyle is XSSFCellStyle)
        {
            ((XSSFCellStyle)cellStyle).SetFillBackgroundColor(new XSSFColor(new byte[] { color.R, color.G, color.B }));
        }
        else if (cellStyle is HSSFCellStyle)
        {
            ((HSSFCellStyle)cellStyle).FillForegroundColor = GetHssfColor(color);
        }
    }

    /// <summary>
    /// 设置字体颜色
    /// </summary>
    /// <param name="font"></param>
    /// <param name="color"></param>
    public static void SetFontColor(IFont font, Color color)
    {
        if (font is XSSFFont)
        {
            ((XSSFFont)font).SetColor(new XSSFColor(new byte[] { color.R, color.G, color.B }));
        }
        else if (font is HSSFFont)
        {
            ((HSSFFont)font).Color = GetHssfColor(color);
        }
    }

    /// <summary>
    /// 保存Excel
    /// </summary>
    /// <param name="workBook"></param>
    /// <param name="fileFullPath"></param>
    public static void Save(this IWorkbook workBook, string fileFullPath)
    {
        var suffix = Path.GetExtension(fileFullPath);
        if (workBook is HSSFWorkbook)
        {
            fileFullPath = fileFullPath.Replace(suffix, ".xls");
        }
        else
        {
            fileFullPath = fileFullPath.Replace(suffix, ".xlsx");
        }

        using (var fs = File.Create(fileFullPath))
        {
            workBook.Write(fs);
        }
    }

    /// <summary>
    /// 获取Excel中03版本的颜色定义
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static short GetHssfColor(Color color)
    {
        var dic = GetColorMap();
        if (dic.TryGetValue(color, out var indexedColor))
        {
            // 系统颜色与NPOI中的IndexedColors匹配，则使用NPOI中的颜色
            return indexedColor.Index;
        }

        // 无匹配，默认使用黑色
        return IndexedColors.Black.Index;
    }

    /// <summary>
    /// 获取System.Drawing.Color与NPOI.HSSF.Util.IndexedColors颜色对应映射关系
    /// </summary>
    /// <returns></returns>
    private static Dictionary<Color, IndexedColors> GetColorMap()
    {
        Dictionary<Color, IndexedColors> colorMap = new Dictionary<Color, IndexedColors>();

        // 从System.Drawing.Color中获取公开的静态颜色属性
        PropertyInfo[] colorProperties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);

        // 从NPOI.HSSF.Util.IndexedColors中获取公开的静态颜色字段
        FieldInfo[] indexedColorFields = typeof(IndexedColors).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var colorProperty in colorProperties)
        {
            if (colorProperty.PropertyType == typeof(Color))
            {
                Color systemColor = (Color)colorProperty.GetValue(null)!;
                string colorName = colorProperty.Name;

                foreach (var indexedColorField in indexedColorFields)
                {
                    if (indexedColorField.FieldType == typeof(IndexedColors))
                    {
                        IndexedColors indexedColor = (IndexedColors)indexedColorField.GetValue(null)!;
                        if (indexedColorField.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase))
                        {
                            colorMap.TryAdd(systemColor, indexedColor);
                        }
                    }
                }
            }
        }

        return colorMap;
    }

    private static string GetExcelColumnName(int columnIndex)
    {
        if (columnIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columnIndex), "Excel列的索引必须大于等于零.");
        }

        string columnName = string.Empty;
        while (columnIndex >= 0)
        {
            int remainder = columnIndex % 26;
            columnName = (char)(remainder + 'A') + columnName;
            columnIndex = (columnIndex / 26) - 1; // 移动到下一轮
        }

        return columnName;
    }
}