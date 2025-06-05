using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;


public class StationPointBaseDto
{
    public long AreaId { get; set; }

    public bool IsAlarm { get; set; }
    /// <summary>
    /// 区域名称
    /// </summary>
    public string AreaName { get; set; }

    /// <summary>
    /// 分站表主键
    /// </summary>
    public long SubStationId { get; set; }

    /// <summary>
    /// 分站编号
    /// </summary>
    public string StationNumber { get; set; }

    /// <summary>
    /// 测点表主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 测点名称
    /// </summary>
    public string PointName { get; set; }

    /// <summary>
    /// 测点编码
    /// </summary>
    public string PointNumber { get; set; }

    /// <summary>
    /// 赋值时间
    /// </summary>
    public DateTime PointDateTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndDateTime { get; set; }
    /// <summary>
    /// 电压值
    /// </summary>
    public string SensorVoltage { get; set; }


    /// <summary>
    /// 485 ID
    /// </summary>
    public string CommunicationId { get; set; }

    /// <summary>
    /// 测点分类(模拟量、开关量、控制口、馈电口等）
    /// </summary>
    public SensorType PointClass { get; set; }

    /// <summary>
    /// 传感器类型
    /// </summary>
    public virtual string TypeId { get; set; }

    /// <summary>
    /// 实时值
    /// </summary>
    public virtual double RealValue { get; set; }

    /// <summary>
    /// 实时值字符串
    /// </summary>
    public string StrValue { get; set; }

    /// <summary>
    /// 报警状态，只有Normal   和  Alarm
    /// </summary>
    public PointDataStatusEnum PointStatus { get; set; }


    public string PowerStatus { get; set; }

    public string FeedStatus { get; set; }


    public string AlarmStatus { get; set; }

    /// <summary>
    /// 总报警ID
    /// </summary>
    public long? AlarmId { get; set; }

    /// <summary>
    /// 断电报警ID
    /// </summary>
    public long? PowerId { get; set; }

    /// <summary>
    ///  馈电ID
    /// </summary>
    public long? PowerOffAlarmId { get; set; }

    /// <summary>
    /// 状态变更报警ID
    /// </summary>
    public long? StatusAlarmId { get; set; }


    /// <summary>
    /// 规则ID
    /// </summary>
    public long? RuleId { get; set; }


    /// <summary>
    /// 测点数据状态，正常、异常
    /// </summary>
    public PointDataStatusEnum? PointDataStatus { get; set; }

    /// <summary>
    /// 测点原始状态
    /// </summary>
    public PointDataStatusEnum PointDataOriginStatus { get; set; }

    /// <summary>
    /// 是否校准模式
    /// </summary>
    public bool IsCalibrationMode { get; set; }
}