using Furion.Demo.Core;
using Furion.EventBus;

namespace Furion.Demo.Application.SqlSugar;

[Route("api/mock")]
public class MockAppService:IDynamicApiController
{
    /// <summary>
    /// 批量生成数据，使用事件总线发布
    /// </summary>
    /// <returns></returns>
    [HttpGet("Generate")]
    public async Task CreateAsync()
    {
        for (int i = 0; i < 1000; i++)
        {
            var data = new PointDataEntity()
            {
                ts = DateTime.Today.AddSeconds(i),
                SNO = Random.Shared.Next(1000, 1005).ToString(),
                PointNumber = Random.Shared.Next(2000, 2005).ToString(),
                Id = Random.Shared.Next(1, 100),
                PointValue = Random.Shared.Next(1, 40),
                Day = DateOnly.FromDateTime(DateTime.Today).ToString(),
            };
            await MessageCenter.PublishAsync("PointValueChanged", data);
        }
    }
}