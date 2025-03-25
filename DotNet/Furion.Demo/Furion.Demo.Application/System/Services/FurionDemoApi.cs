using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Application.System.Services;

public class FurionDemoApi : IDynamicApiController
{

    /// <summary>
    /// 打招呼
    /// </summary>
    /// <returns></returns>
    [ActionName("SayHello")]
    public string SayHello()
    {
        return "Hello Furion";
    }
}
