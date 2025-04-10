using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public double Min { get; set; }
}