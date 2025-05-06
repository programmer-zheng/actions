using SqlSugar.DbConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Furion.Demo.Core;

namespace Furion.Demo.Application.System.Dtos;

public class TdAggregateDataDto
{
    public double Avg { get; set; }

    public double Max { get; set; }

    public DateTime MaxTime { get; set; }

    public double Min { get; set; }

    public DateTime MinTime { get; set; }
}