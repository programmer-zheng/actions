using SqlSugar;

namespace Furion.Demo.Core;

/// <summary>
/// MySQL数据库中的表
/// </summary>
[SugarTable("Point_Data")]
[TraditionDataTable]
[Tenant(Consts.MainConfigId)]
public class PointEntity : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }

    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }
}
