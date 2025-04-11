using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Furion.Demo.Application.System.Dtos;

public class CreateTdDataDto
{

    public DateTime Ts { get; set; }

    public int Id { get; set; }

    /// <summary>
    /// 分站号
    /// </summary>
    [DefaultValue("152")]
    public string Sno { get; set; }

    /// <summary>
    /// 测点编号
    /// </summary>
    [DefaultValue("152A01")]
    public string PointNumber { get; set; }

    /// <summary>
    /// 测点类型
    /// </summary>
    [DefaultValue("1D")]
    public string PointType { get; set; }

    public DateTime Day { get; set; } = DateTime.Today;
}
