namespace Furion.Demo.Application.System;

[ApiDescriptionSettings("Furion CustomGroup", Description = "自定义分组")]
[Route("api/FurionDemoApi")]
public class FurionDemoApiAppService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 打招呼
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public string SayHello()
    {
        return "Hello Furion";
    }
}
