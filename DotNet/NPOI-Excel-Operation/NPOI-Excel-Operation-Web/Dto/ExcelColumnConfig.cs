using System.Drawing;

namespace NPOI_Excel_Operation_Web.Dto;

public class ExcelColumnConfig
{
    /// <summary>
    /// Excel中的列取值，对应实体属性名
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Excel表头列名
    /// </summary>
    public string ExcelCellHeader { get; set; }

    /// <summary>
    /// 字体颜色
    /// </summary>
    public Color FontColor { get; set; } = Color.Black;

    /// <summary>
    /// 背景颜色
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.White;

    /// <summary>
    /// 字体 
    /// </summary>
    public string FontFamily { get; set; } = "";

    /// <summary>
    /// 字号.
    /// </summary>
    public short FontSize { get; set; }


    /// <summary>
    /// 单元格宽度
    /// </summary>
    public int ExcelCellWidth { get; set; }
}