using SqlSugar;
using SqlSugar.TDengine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

[SugarTable("point_data")]
[TimingDataTable]
[STable(STableName = "point_data", Tag1 = nameof(SNO), Tag2 = nameof(PointNumber))]
public class PointDataEntity //: STable
{

    [SugarColumn(IsPrimaryKey = true, InsertServerTime = true)]
    public DateTime ts { get; set; }

    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }
}

[SugarTable("point_data")]
[TraditionDataTable]
public class PointEntity //: STable
{

    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }
}