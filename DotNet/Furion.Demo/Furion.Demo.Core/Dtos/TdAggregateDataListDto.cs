using System;
using SqlSugar;
using SqlSugar.DbConvert;

namespace Furion.Demo.Core.Dtos;

public class TdAggregateDataListDto
{
    public double Val { get; set; }

    public DateTime Time { get; set; }

    
    [SugarColumn(SqlParameterDbType = typeof(EnumToStringConvert))]
    public AgggegateTypeEnum Type { get; set; }
}