using SqlSugar;
using SqlSugar.DbConvert;
using System.ComponentModel;

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

    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert), ColumnDescription = "测点类型")]
    public SensorType SensorType { get; set; } = SensorType.Analog;
}

public enum SensorType
{
    [Description("模拟量")]
    Analog = 1,

    [Description("数字量")]
    Digital = 2,

    [Description("控制量")]
    Control = 3,

    [Description("馈电量")]
    Feed = 4,
}