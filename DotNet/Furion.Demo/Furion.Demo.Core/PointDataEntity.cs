using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

[SugarTable("point_data")]
public class PointDataEntity
{

    /// <summary>
    /// 测点编号
    /// </summary>
    [SugarColumn(ColumnDescription = "测点编号")]
    public string PointNumber { get; set; }

    /// <summary>
    /// 测点值
    /// </summary>
    [SugarColumn(ColumnDescription = "测点值")]
    public double PointValue { get; set; }
}
