using SqlSugar;
using SqlSugar.TDengine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

/*
 * TdEngine的表，不能使用标准注释
 * SugarTable中也不能使用TableDescription
 * 否则会生成
 * comment on table `point_data` is 'xxx'
 *
 * STable中指定的表名需要与SugarTable中的一致，否则查询时会提示表不存在（表名与实体不一致情况下）
 */
[SugarTable("Point_Data")]
[TimingDataTable]
[Tenant(Consts.TdConfigId)]
[STable(STableName = "Point_Data", Tag1 = nameof(SNO), Tag2 = nameof(PointNumber))]
//[STable(STableName = "Point_Data", Tag1 = nameof(SNO), Tag2 = nameof(PointNumber), Tag3 = nameof(Day))]
//[STable(STableName = "Point_Data", Tag1 = nameof(Day))]
public class PointDataEntity : ITdPrimaryKey
{
    /*
     * [SugarColumn(IsPrimaryKey = true, InsertServerTime = true)]
     * 毫秒 默认
     * 纳秒：SqlParameterDbType =typeof(DateTime19)，TsType=config_ns
     * 微秒：SqlParameterDbType =typeof(DateTime16)，TsType=config_us
     *
     */

    //[SugarColumn(IsPrimaryKey = true, InsertServerTime = true)]
    [SugarColumn(IsPrimaryKey = true, SqlParameterDbType = typeof(DateTime19))]
    public DateTime ts { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    [Key]
    public int Id { get; set; }

    [SugarColumn(IsOnlyIgnoreInsert = true, IsOnlyIgnoreUpdate = true)]//Tags字段禁止插入
    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }

    public string Day { get; set; }
    
    [SugarColumn(ColumnName = "alarm_id")]
    public long? AlarmId { get; set; }

    public DateTime? DateTime { get; set; }
    
    public double? AvgValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public double? MinValue { get; set; }
}

/// <summary>
/// MySQL数据库中的表
/// </summary>
[SugarTable("Point_Data")]
[TraditionDataTable]
[Tenant(Consts.MySqlConfigId)]
public class PointEntity : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }
}

public abstract class BaseEntity : ISoftDelete
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? DeletionTime { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? DeleterUserId { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    DateTime? DeletionTime { get; set; }

    long? DeleterUserId { get; set; }
}