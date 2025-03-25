namespace CustomTemplateSearch.EntityFrameworkCore;

[TemplateIgnore]
public class TemplateColumn : BaseEntity
{
    /// <summary>
    /// 列名
    /// </summary>
    public string ColmunName { get; set; }

    /// <summary>
    /// 数据库列类型
    /// </summary>
    public string ColumnType { get; set; }
}