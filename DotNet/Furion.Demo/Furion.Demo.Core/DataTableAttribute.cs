using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

/// <summary>
/// 标记传统关系型数据库表
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TraditionDataTableAttribute : Attribute
{
}

/// <summary>
/// 标记时序数据库表
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TimingDataTableAttribute : Attribute
{
}
