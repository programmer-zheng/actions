using SqlSugar.DbConvert;
using SqlSugar.TDengine;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

[SugarTable("point_alarm_data")]
[TimingDataTable]
[STable(STableName = "point_alarm_data", Tag1 = nameof(SubStationId), Tag2 = nameof(PointId), Tag3 = nameof(SensorType))]
[Tenant(Consts.TdConfigId)]
public class PointAlarmData
{
    [SugarColumn(IsPrimaryKey = true, SqlParameterDbType = typeof(DateTime19))]
    public DateTime ts { get; set; }

    /// <summary>
    /// 分站ID
    /// </summary>
    public string SubStationId { get; set; }

    /// <summary>
    /// 测点ID
    /// </summary>
    public string PointId { get; set; }

    /// <summary>
    /// 分站编号：001000
    /// </summary>
    [SugarColumn(ColumnName = "station_number")]
    public string StationNumber { get; set; }

    /// <summary>
    /// 测点名称
    /// </summary>
    [SugarColumn(ColumnName = "sensor_name")]
    public string SensorName { get; set; }

    /// <summary>
    /// 区域ID
    /// </summary>
    [SugarColumn(ColumnName = "area_id")]
    public long AreaId { get; set; }

    /// <summary>
    /// 安装区域名称
    /// </summary>
    [SugarColumn(ColumnName = "area_name")]
    public string AreaName { get; set; }

    /// <summary>
    /// 传感器类型
    /// </summary>
    public string SensorType { get; set; }

    /// <summary>
    /// 报警时刻值
    /// </summary>
    [SugarColumn(ColumnName = "alarm_value")]
    public double AlarmValue { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [SugarColumn(ColumnName = "start_time")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnName = "end_time")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 持续时间(单位:秒)
    /// </summary>
    [SugarColumn(ColumnName = "duration")]
    public double? Duration { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    [SugarColumn(ColumnName = "max_value")]
    public double? MaxValue { get; set; }

    /// <summary>
    /// 最大值时刻
    /// </summary>
    [SugarColumn(ColumnName = "max_value_time")]
    public DateTime? MaxValueTime { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    [SugarColumn(ColumnName = "min_value")]
    public double? MinValue { get; set; }

    /// <summary>
    /// 最小值时刻
    /// </summary>
    [SugarColumn(ColumnName = "min_value_time")]
    public DateTime? MinValueTime { get; set; }

    /// <summary>
    /// 平均值
    /// </summary>
    [SugarColumn(ColumnName = "avg_value")]
    public double? AvgValue { get; set; }

    /// <summary>
    /// 测点数据状态
    /// </summary>
    [SugarColumn(ColumnName = "point_data_status", ColumnDataType = "VARCHAR(50)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public PointDataStatusEnum PointDataStatus { get; set; }

    /// <summary>
    /// 测点原始状态
    /// </summary>
    [SugarColumn(ColumnName = "point_origin_status", ColumnDataType = "VARCHAR(50)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public PointDataStatusEnum PointOriginStatus { get; set; }

    /// <summary>
    /// 报警ID
    /// </summary>
    [SugarColumn(ColumnName = "alarm_id")]
    public long AlarmId { get; set; }
}
