using System;
using SqlSugar;
using SqlSugar.DbConvert;

namespace Furion.Demo.Core;

public class AggregateDataDto<TProperty>
{
    public TProperty Avg { get; set; }

    public TProperty Max { get; set; }
    
    public DateTime MaxTime { get; set; }

    public TProperty Min { get; set; }
    
    public DateTime MinTime { get; set; }
}

public class AggregateDataListDto<TProperty>
{
    public TProperty Val { get; set; }

    public DateTime Time { get; set; }

    [SugarColumn(SqlParameterDbType = typeof(EnumToStringConvert))]
    public AgggegateTypeEnum Type { get; set; }
}

public enum AgggegateTypeEnum
{
    Max,
    Min,
    Avg
}