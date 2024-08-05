using System.Drawing;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace NPO_Excel_Operation;

class Program
{
    static void Main(string[] args)
    {
        //test.ExportExcelWithCascadingDropdown();

        IWorkbook workBook = new XSSFWorkbook();
        // workBook = new HSSFWorkbook();
        // 不创建sheet，保存后无法打开excel文件
        // 不指定sheetname，默认使用sheet0作为sheetname，与正常excel创建后显sheet的名称不一致
        var sheet = workBook.CreateSheet("Sheet1");

        var columnConfigs = GetExcelColumnConfigs();
        var headerRow = sheet.CreateRow(0);
        for (int i = 0; i < columnConfigs.Count; i++)
        {
            var cellConfig = columnConfigs[i];
            var cellStyle = GetCellStyle(workBook, cellConfig);
            var cell = headerRow.CreateCell(i);
            cell.CellStyle = cellStyle;
            cell.SetCellValue(cellConfig.ExcelCellHeader);
        }

        // 设置列宽
        sheet.SetColumnWidth(0, 9 * 256);
        sheet.SetColumnWidth(1, 9 * 256);
        sheet.SetColumnWidth(2, 9 * 256);

        var sampleData = BaseAddressInfo.GetSampleData();
        var provinceList = sampleData.Select(t => t.Name).ToList();
        CustomExcelHelper.SetCellDropdownListDirect(sheet, 0, 0, provinceList.ToArray());
        //SetCellDropdownList(workBook, sheet, "ProvinceList", 0, 0, provinceList);


        var columnIndex = 0;
        workBook.WriteDropDownDataSource("BaseAddressInfoData", provinceList, columnIndex++);
        var cityCount = sampleData.Select(t => t.Children).Count();
        var areaColumnIndex = 0;
        foreach (var province in sampleData)
        {
            var cityList = province.Children.Select(t => t.Name).ToList();
            workBook.WriteDropDownDataSource("BaseAddressInfoData", cityList, columnIndex++);
            foreach (var city in province.Children)
            {
                var areaList = city.Children.Select(t => t.Name).ToList();
                areaColumnIndex++;
                workBook.WriteDropDownDataSource("BaseAddressInfoData", areaList, cityCount + areaColumnIndex);
            }
        }
        //sheet.CreateDropList(1, 0, 0);

        var fileFullPath = Path.Combine("C:", "ExcelSample.xlsx");
        workBook.Save(fileFullPath);

        Console.WriteLine("Excel generate success!");
    }


    static string GetExcelColumnName(int columnNumber)
    {
        int dividend = columnNumber + 1;
        string columnName = String.Empty;
        int modulo;

        while (dividend > 0)
        {
            modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
            dividend = (int)((dividend - modulo) / 26);
        }

        return columnName;
    }


    /// <summary>
    /// 获取Excel表头配置
    /// </summary>
    /// <returns></returns>
    static List<ExcelColumnConfig> GetExcelColumnConfigs()
    {
        return new List<ExcelColumnConfig>()
        {
            new() { ExcelCellHeader = "省", FontColor = Color.Red, FontSize = 12, FontFamily = "宋体" },
            new() { ExcelCellHeader = "市", FontColor = Color.Purple, FontSize = 12, FontFamily = "宋体" },
            new() { ExcelCellHeader = "区", FontColor = Color.Green, FontSize = 12, FontFamily = "宋体" },
        };
    }

    /// <summary>
    /// 获取Excel列样式
    /// </summary>
    /// <param name="workBook"></param>
    /// <param name="excelColumnConfig"></param>
    /// <returns></returns>
    static ICellStyle GetCellStyle(IWorkbook workBook, ExcelColumnConfig excelColumnConfig)
    {
        var cellStyle = workBook.CreateCellStyle();
        cellStyle.Alignment = HorizontalAlignment.Center;
        cellStyle.VerticalAlignment = VerticalAlignment.Center;

        if (excelColumnConfig.BackgroundColor != new Color())
        {
            CustomExcelHelper.SetBackgroundColor(cellStyle, excelColumnConfig.BackgroundColor);
        }

        // 字体相关
        var font = workBook.CreateFont();
        font.FontHeightInPoints = excelColumnConfig.FontSize != 0 ? excelColumnConfig.FontSize : 11;
        font.IsBold = true;
        if (excelColumnConfig.FontColor != new Color())
        {
            CustomExcelHelper.SetFontColor(font, excelColumnConfig.FontColor);
        }

        if (!string.IsNullOrWhiteSpace(excelColumnConfig.FontFamily))
        {
            font.FontName = excelColumnConfig.FontFamily;
        }

        cellStyle.SetFont(font);
        return cellStyle;
    }
}