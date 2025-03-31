using SqlSugar;
using SqlSugar.TDengine;
using System;
using System.Collections.Generic;
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
[STable(STableName = "Point_Data", Tag1 = nameof(SNO), Tag2 = nameof(PointNumber))]
public class PointDataEntity
{
    /*
     * [SugarColumn(IsPrimaryKey = true, InsertServerTime = true)]
     * 毫秒 默认
     * 纳秒：SqlParameterDbType =typeof(DateTime19)，TsType=config_ns
     * 微秒：SqlParameterDbType =typeof(DateTime16)，TsType=config_us
     * 
     */

    [SugarColumn(IsPrimaryKey = true, InsertServerTime = true)]
    public DateTime ts { get; set; }

    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }
}

/// <summary>
/// MySQL数据库中的表
/// </summary>
[SugarTable("Point_Data")]
[TraditionDataTable]
public class PointEntity
{

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }
}