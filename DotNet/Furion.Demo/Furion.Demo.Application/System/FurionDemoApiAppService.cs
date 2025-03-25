using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Application.System;

[ApiDescriptionSettings("CustomGroup","CustomGroup2", Description = "自定义分组")]
[Route("api/FurionDemoApi")]
public class FurionDemoApiAppService : IDynamicApiController
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
