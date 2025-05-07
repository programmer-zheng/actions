using Furion.Demo.Core;
using Furion.EventBus;

namespace Furion.Demo.Application.SqlSugar;

[Route("api/mock")]
public class MockAppService : IDynamicApiController
{
    /// <summary>
    /// 批量生成数据，使用事件总线发布
    /// </summary>
    /// <returns></returns>
    [HttpGet("Generate")]
    public async Task CreateAsync()
    {
        var t1 = Enumerable.Range(1, 10000).Select(async i =>
         {
             var data = new PointDataEntity()
             {
                 ts = DateTime.Today.AddMicroseconds(i),
                 SNO = Random.Shared.Next(1000, 1005).ToString(),
                 PointNumber = Random.Shared.Next(2000, 2005).ToString(),
                 Id = i,
                 PointValue = Random.Shared.Next(1, 40),
                 Day = DateOnly.FromDateTime(DateTime.Today).ToString(),
             };
             await MessageCenter.PublishAsync("PointValueChanged", data);
         });
        var t2 = Enumerable.Range(20000, 10000).Select(async i =>
        {
            var data = new PointDataEntity()
            {
                ts = DateTime.Today.AddMicroseconds(i),
                SNO = Random.Shared.Next(1000, 1005).ToString(),
                PointNumber = Random.Shared.Next(2000, 2005).ToString(),
                Id = i,
                PointValue = Random.Shared.Next(1, 40),
                Day = DateOnly.FromDateTime(DateTime.Today).ToString(),
            };
            await MessageCenter.PublishAsync("PointValueChanged2", data);
        });
        var t3 = Enumerable.Range(40000, 10000).Select(async i =>
        {
            var data = new PointDataEntity()
            {
                ts = DateTime.Today.AddMicroseconds(i),
                SNO = Random.Shared.Next(1000, 1005).ToString(),
                PointNumber = Random.Shared.Next(2000, 2005).ToString(),
                Id = i,
                PointValue = Random.Shared.Next(1, 40),
                Day = DateOnly.FromDateTime(DateTime.Today).ToString(),
            };
            await MessageCenter.PublishAsync("PointValueChanged3", data);
        });
        var t4 = Enumerable.Range(60000, 10000).Select(async i =>
        {
            var data = new PointDataEntity()
            {
                ts = DateTime.Today.AddMicroseconds(i),
                SNO = Random.Shared.Next(1000, 1005).ToString(),
                PointNumber = Random.Shared.Next(2000, 2005).ToString(),
                Id = i,
                PointValue = Random.Shared.Next(1, 40),
                Day = DateOnly.FromDateTime(DateTime.Today).ToString(),
            };
            await MessageCenter.PublishAsync("PointValueChanged4", data);
        });
        await Task.WhenAll(t1.Concat(t2).Concat(t3).Concat(t4));
    }
}