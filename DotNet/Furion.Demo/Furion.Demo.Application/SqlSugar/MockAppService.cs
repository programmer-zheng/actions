using Furion.Demo.Core;
using Furion.EventBus;

namespace Furion.Demo.Application.SqlSugar;

[Route("api/mock")]
public class MockAppService:IDynamicApiController
{
    /// <summary>
    /// 往MySql中插入数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Generate")]
    public async Task CreateAsync()
    {
        for (int i = 0; i < 1000; i++)
        {
            var data = new PointDataEntity()
            {
                ts = DateTime.Now,
                SNO = Random.Shared.Next(1, 254).ToString(),
                PointNumber = Random.Shared.Next(1, 40).ToString(),
                Id = Random.Shared.Next(1, 100),
                PointValue = Random.Shared.Next(1, 40),
                Day = DateOnly.FromDateTime(DateTime.Today).ToString(),
            };
            await MessageCenter.PublishAsync("PointValueChanged", data);
            await Task.Delay(100);
        }
    }
}