namespace Furion.Demo.Core;

public class Consts
{
    public const string MainConfigId = "Main";
    public const string TdConfigId = "TDengine";


    /// <summary>
    /// 测点报警（含恢复）缓存键，Key为报警ID，value为报警数据
    /// </summary>
    public const string PointAlarmHashTableKey = "cache_hash_point_alarm";

    /// <summary>
    /// 测点实时数据缓存键，用于计算报警时段内的最大、最小、平均值，key为测点ID，value为（测点值、采集时间）
    /// </summary>
    public const string PointRelaTimeHashTableKey = "cache_hash_point_relatime";

}