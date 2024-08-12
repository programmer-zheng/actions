using System.Drawing;
using System.Reflection;
using System.Text;
using NPOI_Excel_Operation_Web.Dto;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace NPOI_Excel_Operation_Web.Extensions;

public class CustomExcelHelper
{
    /// <summary>
    /// 为单元格设置下拉（不额外创建Sheet）
    /// 如果 firstCol 为0，lastCol 为1，则0、1两列都设置下拉 
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="firstCol"></param>
    /// <param name="lastCol"></param>
    /// <param name="vals">下拉的选项</param>
    public static void SetCellDropdownListDirect(ISheet sheet, int firstCol, int lastCol, string[] vals)
    {
        IDataValidation dataValidate;
        var maxRowCount = sheet.GetSheetMaxRowCount();
        //设置生成下拉框的行和列
        var cellRegions = new CellRangeAddressList(1, maxRowCount, firstCol, lastCol);
        if (sheet is XSSFSheet)
        {
            var dvHelper = sheet.GetDataValidationHelper();
            var constraint = dvHelper.CreateExplicitListConstraint(vals);

            dataValidate = dvHelper.CreateValidation(constraint, cellRegions);
        }
        else
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

    /// <summary>
    /// 根据列索引，获取Excel中的列名（如A、AB）
    /// </summary>
    /// <param name="columnIndex">列索引，从0开始，0为A</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string GetExcelColumnName(int columnIndex)
    {
        if (columnIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columnIndex), "Excel列的索引必须大于等于零.");
        }

        StringBuilder columnName = new StringBuilder();
        while (columnIndex >= 0)
        {
            int modulo = columnIndex % 26;
            columnName.Insert(0, Convert.ToChar(65 + modulo));
            columnIndex = (columnIndex / 26) - 1;
        }

        return columnName.ToString();
    }

    /// <summary>
    /// 创建指定版本Excel工作簿
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static IWorkbook CreateWorkbook(ExcelVersionEnum version = ExcelVersionEnum.Version2007)
    {
        if (version == ExcelVersionEnum.Version2003)
        {
            return new HSSFWorkbook();
        }

        return new XSSFWorkbook();
    }

    /// <summary>
    /// 导出到新建Sheet中
    /// </summary>
    /// <param name="list">需要导出的数据列表</param>
    /// <param name="configs">导出配置</param>
    /// <param name="workbook">工作簿</param>
    /// <param name="sheetName">表格名称，如果不指定，使用默认生成（从Sheet0开始）</param>
    /// <typeparam name="T"></typeparam>
    public static void Export<T>(List<T> list, List<ExcelColumnConfig> configs, IWorkbook workbook, string sheetName = null)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(workbook);
        ISheet sheet = null;
        if (!string.IsNullOrWhiteSpace(sheetName))
        {
            sheet = workbook.CreateSheet(sheetName);
        }
        else
        {
            sheet = workbook.CreateSheet();
        }

        var properties = typeof(T).GetProperties().ToList();

        //  属性列表中筛选出需要显示在Excel中的
        var existsProperties = (from p in properties
            join columnConfig in configs on p.Name equals columnConfig.PropertyName
            where !string.IsNullOrWhiteSpace(columnConfig.PropertyName)
            orderby columnConfig.SoftVal
            select columnConfig).ToList();

        IDataFormat dataFormat = workbook.CreateDataFormat();
        var styleDic = new Dictionary<int, ICellStyle>(); //存储每列的样式
        var headerRow = sheet.CreateRow(0);
        var headerCellIndex = 0;
        foreach (var excelColumnConfig in existsProperties)
        {
            var cell = headerRow.CreateCell(headerCellIndex);
            cell.SetCellValue(excelColumnConfig.ExcelCellHeader);

            // 列样式
            ICellStyle cellStyle = workbook.CreateCellStyle();
            if (excelColumnConfig.BackgroundColor != new Color())
            {
                // 背景
                SetBackgroundColor(cellStyle, excelColumnConfig.BackgroundColor);
            }

            var font = workbook.CreateFont();
            font.FontHeightInPoints = excelColumnConfig.FontSize != 0 ? excelColumnConfig.FontSize : 11; // 设置字号，默认11
            if (excelColumnConfig.FontColor != new Color())
            {
                // 文字颜色
                SetFontColor(font, excelColumnConfig.FontColor);
            }

            if (!string.IsNullOrWhiteSpace(excelColumnConfig.FontFamily))
            {
                // 设置字体 
                font.FontName = excelColumnConfig.FontFamily;
            }

            var propertyType = properties.First(t => t.Name == excelColumnConfig.PropertyName).PropertyType;
            if (propertyType == typeof(DateTime) || propertyType.GenericTypeArguments.FirstOrDefault() == typeof(DateTime))
            {
                // 如果当前列为日期，设置日期格式
                cellStyle.DataFormat = dataFormat.GetFormat("yyyy-MM-dd HH:mm:ss");
            }

            cellStyle.SetFont(font);
            cell.CellStyle = cellStyle;
            styleDic.Add(headerCellIndex, cellStyle);
            headerCellIndex++;
        }

        // 输出数据行
        var dataRowIndex = 1;
        foreach (T item in list)
        {
            var dataRow = sheet.CreateRow(dataRowIndex);
            var dataCellIndex = 0;
            foreach (var excelColumnConfig in existsProperties)
            {
                var dataCell = dataRow.CreateCell(dataCellIndex);
                var property = properties.First(t => t.Name == excelColumnConfig.PropertyName);
                var propertyValue = property.GetValue(item, null);
                string? val = propertyValue == null ? string.Empty : propertyValue.ToString();
                if (styleDic.ContainsKey(dataCellIndex))
                {
                    dataCell.CellStyle = styleDic[dataCellIndex];
                }

                SetCellValue(dataCell, property.PropertyType, val!);


                dataCellIndex++;
            }

            dataRowIndex++;
        }
    }

    /// <summary>
    /// 设置单元格值，针对特殊类型，设置单元格格式
    /// </summary>
    /// <param name="cell">目标单元格</param>
    /// <param name="dataType">属性数据类型</param>
    /// <param name="dataValue">值</param>
    private static void SetCellValue(ICell cell, Type dataType, string dataValue)
    {
        string callDataType = dataType.GenericTypeArguments.Count() == 0 ? dataType.ToString() : dataType.GenericTypeArguments.FirstOrDefault().ToString();
        if (string.IsNullOrWhiteSpace(dataValue))
        {
            cell.SetCellValue(string.Empty);
            return;
        }

        switch (callDataType)
        {
            case "System.String":
                cell.SetCellValue(dataValue);
                break;
            case "System.DateTime":
                cell.SetCellValue(Convert.ToDateTime(dataValue));
                break;
            case "System.Boolean":
                cell.SetCellValue(Convert.ToBoolean(dataValue));
                break;
            case "System.Short":
            case "System.Int16":
            case "System.Int32":
            case "System.Int64":
            case "System.Byte":
                cell.SetCellValue(Convert.ToInt64(dataValue));
                cell.SetCellType(CellType.Numeric);
                break;
            case "System.Decimal":
            case "System.Double":
            case "System.Single":
                cell.SetCellValue(Convert.ToDouble(dataValue));
                cell.SetCellType(CellType.Numeric);
                break;
            case "System.DBNull":
                cell.SetCellValue(string.Empty);
                break;
            default:
                cell.SetCellValue(dataValue);
                break;
        }
    }
}