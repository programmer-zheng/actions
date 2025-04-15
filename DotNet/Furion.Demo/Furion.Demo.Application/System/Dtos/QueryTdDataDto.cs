using SqlSugar.DbConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Furion.Demo.Core;

namespace Furion.Demo.Application.System.Dtos;

public class QueryTdDataDto
{

    public string Sno { get; set; }

    public string PointNumber { get; set; }
}


public class TdAggregateDataDto
{
    public double Avg { get; set; }

    public double Max { get; set; }
    
    public DateTime MaxTime { get; set; }

    public double Min { get; set; }
    
    public DateTime MinTime { get; set; }
}

public class TdAggregateDataListDto
{
    public double Val { get; set; }

    public DateTime Time { get; set; }

    [SugarColumn(SqlParameterDbType = typeof(EnumToStringConvert))]
    public AgggegateTypeEnum Type { get; set; }
}