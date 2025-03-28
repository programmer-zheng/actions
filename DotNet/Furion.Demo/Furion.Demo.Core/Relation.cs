using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

[AttributeUsage(AttributeTargets.Class)]
public class TraditionDataTableAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class TimingDataTableAttribute : Attribute
{
}
