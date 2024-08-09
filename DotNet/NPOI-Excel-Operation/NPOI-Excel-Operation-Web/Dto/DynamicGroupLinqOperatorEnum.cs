namespace NPOI_Excel_Operation_Web.Dto;

/// <summary>
/// 动态分组操作
/// </summary>
public enum DynamicGroupLinqOperatorEnum
{
    /// <summary>
    /// 分组取最大值
    /// </summary>
    Max,

    /// <summary>
    /// 分组取最小值
    /// </summary>
    Min,

    /// <summary>
    /// 分组取合
    /// </summary>
    Sum,

    /// <summary>
    /// 分组取第一个
    /// </summary>
    First,

    /// <summary>
    /// 字符串形式拼接（逗号分割）
    /// </summary>
    Concat,

    /// <summary>
    /// 去重后汇总
    /// </summary>
    DistinctSum,

}