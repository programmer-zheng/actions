using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

public enum PointDataStatusEnum
{
    /// <summary>
    /// 正常
    /// </summary>
    [Description("正常")]
    Normal,
    /// <summary>
    /// 断线
    /// </summary>
    [Description("断线")]
    Disconnect,

    /// <summary>
    /// 通讯异常
    /// </summary>
    [Description("通讯异常")]
    CommunicationFail,

    /// <summary>
    /// 报警
    /// </summary>
    [Description("报警")]
    Alarm,

    /// <summary>
    /// 标校
    /// </summary>
    [Description("标校")]
    CalibrationMode,


    /// <summary>
    /// 0态
    /// </summary>
    [Description("0态")]
    ZeroStatus,


    /// <summary>
    /// 1态
    /// </summary>
    [Description("1态")]
    OneStatus,


    /// <summary>
    /// 2态
    /// </summary>
    [Description("2态")]
    TwoStatus,


    [Description("异常点")]
    ExceptionPoint,

    [Description("逻辑点")]
    LogicPoint,

    [Description("火警点")]
    FirePoint,

    [Description("门限值")]
    ThresholdValue,

    [Description("报警规则")]
    AlarmRule,

    [Description("断电规则")]
    PowerOffRule,

    [Description("闭锁规则")]
    LockRule,

    [Description("瓦斯规则")]
    GasRule,

    [Description("异地断电")]
    RemotePower,

    [Description("区域断电规则")]
    RegionalPowerOffRule,

    [Description("四级报警规则")]
    FourLevelAlarm,

    [Description("上溢报警规则")]
    Overflow,

    [Description("负偏报警规则")]
    Negative,

    [Description("瓦斯涌出规则")]
    GasOutburstRule,

    [Description("开关量报警规则")]
    SwitchRule,

    [Description("门限值-上断电")]
    UpPowerOutageAlarm,

    [Description("门限值-下断电")]
    DownPowerOutageAlarm,

    [Description("门限值-4级")]
    Level4Alarm,

    [Description("门限值-3级")]
    Level3Alarm,

    [Description("门限值-2级")]
    Level2Alarm,

    [Description("门限值-1级")]
    Level1Alarm,

    [Description("门限值-上报警")]
    UpThresholdAlarm,

    [Description("门限值-下报警")]
    DownThresholdAlarm,

    [Description("门限值-上预警")]
    UpThresholdWarning,

    [Description("门限值-下预警")]
    DownThresholdWarning,

    [Description("复电规则")]
    RestoreRule,


    [Description("正常")]
    Fail,

    [Description("新测点")]
    NewPoint,

    [Description("类型改变")]
    Change,
    /// <summary>
    /// 开关量1态 风电瓦斯闭锁
    /// </summary>
    [Description("1态")]
    OneWindPowerGas,

    /// <summary>
    /// 开关量2态 风电瓦斯闭锁
    /// </summary>
    [Description("2态")]
    TwoWindPowerGas,

    /// <summary>
    /// 模拟量 风电瓦斯闭锁
    /// </summary>
    [Description("2态")]
    AnalogQuantity,
}
