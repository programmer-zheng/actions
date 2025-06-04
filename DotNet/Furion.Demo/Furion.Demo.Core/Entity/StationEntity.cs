using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core.Entity;

[SugarTable("station")]
[TraditionDataTable]
[Tenant(Consts.MainConfigId)]
public class StationEntity : BaseEntity
{
    /// <summary>
    /// 分站号(通讯用)
    /// </summary>
    [SugarColumn(ColumnName = "sno")]
    public string sno { get; set; }

    /// <summary>
    /// 通讯分类
    /// </summary>
    [SugarColumn(ColumnName = "com_mode")]
    public string mode { get; set; } = "Network";

    /// <summary>
    /// 分站类型(8口、16口、24口)
    /// </summary>
    [SugarColumn(ColumnName = "model")]
    public int model { get; set; } = 24;

    /// <summary>
    /// IP地址
    /// </summary>
    [SugarColumn(ColumnName = "ip")]
    public string ip { get; set; }

    /// <summary>
    /// 端口号
    /// </summary>
    [SugarColumn(ColumnName = "port")]
    public int port { get; set; }
}
