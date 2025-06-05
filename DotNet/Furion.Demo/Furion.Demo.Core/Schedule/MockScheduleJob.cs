using Furion.Demo.Core.Service;
using Furion.Schedule;
using Furion.TimeCrontab;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;
using Yitter.IdGenerator;

namespace Furion.Demo.Core.Schedule;
[JobDetail(nameof(MockScheduleJob), "模拟报警生成测试")]
//[Secondly]
[Cron("0/2 * * * * ?", CronStringFormat.WithSeconds)]
//[Minutely]
public class MockScheduleJob : IJob
{
    private readonly IDatabase _cache;

    public MockScheduleJob(IDatabase cache)
    {
        _cache = cache;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        await InsertToCache();

    }


    async Task InsertToCache()
    {
        try
        {
            const int rangeStart = 100000;
            int pointCount = 5000;
            var start = DateTime.Now.AddSeconds(-5);
            var now = DateTime.Now;
            List<StationPointBaseDto> pointData = new List<StationPointBaseDto>();
            for (int i = 1; i <= 3; i++)
            {
                pointData.AddRange(Enumerable.Range(rangeStart, pointCount)
                    .Select(t => new StationPointBaseDto
                    {
                        Id = t,
                        PointDateTime = start.AddSeconds(i),
                        RealValue = Random.Shared.Next(1, 10),
                    })
                    .ToList());
            }
            var tempList = pointData.GroupBy(t => t.Id)
                .Select(t => new
                {
                    Id = t.Key,
                    value = t.Select(x => new PointRelValues { Ts = x.PointDateTime, Value = x.RealValue }).ToList()
                })
                .ToList();
            var pointRelatimeEntries = tempList
                .Select(t => new HashEntry(t.Id.ToString(), JsonConvert.SerializeObject(t.value)))
                .ToArray();

            await _cache.HashSetAsync(Consts.PointRelaTimeHashTableKey, pointRelatimeEntries);

            var data = Enumerable.Range(rangeStart, pointCount).Select(t => new StationPointBaseDto
            {
                Id = t,
                AlarmId = YitIdHelper.NextId(),
                PointName = "高低浓甲烷",
                PointNumber = $"001A{t:D3}",
                SubStationId = 1000,
                StationNumber = "001000",
                PointDateTime = start,
                EndDateTime = now,//Random.Shared.Next(1, 20) > 5 ? now : null,
                PointStatus = PointDataStatusEnum.UpPowerOutageAlarm,
                TypeId = "93",
                RealValue = 1,
                AlarmStatus = "报警",
            }).ToList();
            var hashEntries = new List<HashEntry>(data.Count);
            foreach (var item in data)
            {
                hashEntries.Add(new HashEntry(item.AlarmId.ToString(), JsonConvert.SerializeObject(item)));
            }

            await _cache.HashSetAsync(Consts.PointAlarmHashTableKey, hashEntries.ToArray());
        }
        catch (Exception e)
        {
            Console.WriteLine("----------定时Mock出错----------");
            Console.WriteLine(e);

        }
    }
}


[JobDetail(nameof(ScheduleSavePointAlarmDataFromCacheJob), "定时从缓存中获取报警数据保存到Td")]
//[Secondly]
[Cron("0/2 * * * * ?", CronStringFormat.WithSeconds)]
//[Minutely]
public class ScheduleSavePointAlarmDataFromCacheJob : IJob
{
    private readonly IDatabase _cache;
    private readonly TdService tdService;

    public ScheduleSavePointAlarmDataFromCacheJob(IDatabase redisService, TdService td)
    {
        _cache = redisService;
        tdService = td;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        await Save();
    }

    async Task<Dictionary<string, List<PointRelValues>>> GetDictionaryByHashKeyAsync(string hashKey, List<string> fields)
    {
        var xxx = await _cache.HashGetAllAsync(hashKey);
        var result = new Dictionary<string, List<PointRelValues>>();
        foreach (var entry in xxx)
        {
            var key = entry.Name.ToString();
            var value = JsonConvert.DeserializeObject<List<PointRelValues>>(entry.Value);
            result[key] = value;
        }

        return result;
    }

    async Task Save()
    {
        try
        {
            // 从缓存中获取数据
            var xxx = await _cache.HashGetAllAsync(Consts.PointAlarmHashTableKey);
            var list = new List<StationPointBaseDto>();
            foreach (var item in xxx)
            {
                list.Add(JsonConvert.DeserializeObject<StationPointBaseDto>(item.Value));
            }

            Console.WriteLine($"当前Redis获取到的数量-----------  {list.Count}");
            if (list.Count > 0)
            {
                var pointIdList = list.Select(t => t.Id.ToString()).ToList();
                var pointData = await GetDictionaryByHashKeyAsync(Consts.PointRelaTimeHashTableKey, pointIdList);
                var data = new List<PointAlarmData>();
                var hashKeys = new List<string>();
                foreach (var item in list)
                {
                    var alarmData = new PointAlarmData
                    {
                        ts = item.PointDateTime,
                        SubStationId = item.SubStationId.ToString(),
                        PointId = item.Id.ToString(),
                        StationNumber = item.StationNumber,
                        SensorName = item.PointName,
                        AreaId = item.AreaId,
                        AreaName = item.AreaName,
                        SensorType = item.TypeId,
                        StartTime = item.PointDateTime,
                        EndTime = item.EndDateTime,
                        PointDataStatus = item.PointStatus,
                        PointOriginStatus = item.PointDataOriginStatus,
                        AlarmId = item.AlarmId!.Value,
                        AlarmValue = item.RealValue,
                    };

                    if (item.EndDateTime.HasValue)
                    {
                        SetAggregatedData(alarmData, pointData);
                        hashKeys.Add(item.AlarmId!.Value.ToString());
                    }
                    data.Add(alarmData);
                }

                var hashFields = hashKeys.Select(t => (RedisValue)t).ToArray();
                await _cache.HashDeleteAsync(Consts.PointAlarmHashTableKey, hashFields).ConfigureAwait(false);
                await tdService.BatchInsertPointAlarmData(data);

                // 删除有报警结束时间的报警缓存
                /*_ = Task.Run(async () =>
                {
                });*/

            }

            var currentCount = await tdService.GetAlarmCount();
            Console.WriteLine($"当前数据库数量--- {currentCount}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine("===========================");
            Console.WriteLine(ex);
        }
    }

    private void SetAggregatedData(PointAlarmData alarmData, Dictionary<string, List<PointRelValues>> pointData)
    {
        if (pointData == null || pointData.Count == 0)
        {
            return;
        }

        if (!pointData.ContainsKey(alarmData.PointId))
        {
            return;
        }

        double max = 0, min = 0, sum = 0;
        DateTime? maxTime = null, minTime = null;
        int count = 0;
        var pointDataList = pointData[alarmData.PointId];
        foreach (var item in pointDataList)
        {
            if ( /*item.Id.ToString() == alarmData.PointId &&*/ item.Ts >= alarmData.StartTime && item.Ts <= alarmData.EndTime)
            {
                if (item.Value > max)
                {
                    max = item.Value;
                    maxTime = item.Ts;
                }

                if (item.Value < min)
                {
                    min = item.Value;
                    minTime = item.Ts;
                }

                sum += item.Value;
                count++;
            }
        }

        var avg = count > 0 ? sum / count : 0;
        alarmData.MaxValue = max;
        alarmData.MinValue = min;
        alarmData.AvgValue = avg;
        alarmData.MaxValueTime = maxTime;
        alarmData.MinValueTime = minTime;
    }
}