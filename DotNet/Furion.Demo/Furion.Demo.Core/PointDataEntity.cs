using SqlSugar;
using SqlSugar.TDengine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

[SugarTable("point_data")]
public class PointDataEntity : STable
{

    [SugarColumn(IsPrimaryKey = true, InsertServerTime = true)]
    public DateTime ts { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }
}
