using System.Diagnostics;
using System.Text;
using Dm;
using Furion.Demo.Application.System.Dtos;
using Furion.Demo.Core;
using Furion.Demo.Core.Dtos;
using Furion.Demo.Core.Service;
using Furion.EventBus;

namespace Furion.Demo.Application.SqlSugar;

[Route("api/Td")]
public class TdEngineAppservice : IDynamicApiController
{
    // https://docs.taosdata.com/develop/sql/

    // https://www.donet5.com/Home/Doc?typeId=2566

    private readonly ISugarRepository<PointDataEntity> _repository;

    private readonly ISugarRepositoryTd<PointDataEntity> _tdPrpository;

    private readonly TdService _tdService;

    public TdEngineAppservice(ISugarRepository<PointDataEntity> repository, ISugarRepositoryTd<PointDataEntity> tdPrpository, TdService tdService)
    {
        _tdPrpository = tdPrpository;
        _tdService = tdService;
        _repository = repository;
    }

    [HttpGet("batchInsertToTd")]
    public async Task BatchInsert(int type = 1)
    {
        var list = Enumerable.Range(1, 1_0000).Select(i => new PointDataEntity()
        {
            ts = DateTime.Today.AddMilliseconds(i),
            SNO = Random.Shared.Next(1000, 1005).ToString(),
            PointNumber = Random.Shared.Next(2000, 2005).ToString(),
            Id = i,
            PointValue = Random.Shared.Next(1, 40),
            Day = DateOnly.FromDateTime(DateTime.Today).ToString(),
        }).ToList();


        var sw = Stopwatch.StartNew();
        if (type == 1)
        {
            await _tdService.BatchInsert(list);
        }
        else if (type == 2 && list.Count > 0)
        {
            /*

INSERT INTO

`point_data_152_152A01`  USING `point_data` tags('152','152A01')  (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`datetime`)
 VALUES  ('2025-04-08 11:42:24',1100,'1D',46,'2025/4/8 0:00:00',null) ('2025-04-09 11:03:09',1103,'1D',22,'2025/4/9 0:00:00',null)

`point_data_152_152A02`  USING `point_data` tags('152','152A02')  (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`datetime`)
 VALUES  ('2025-04-08 12:04:08',1101,'3F',15,'2025/4/8 0:00:00',null) ('2025-04-09 12:03:09',1104,'3F',21,'2025/4/9 0:00:00',null)

`point_data_154_154B03`  USING `point_data` tags('154','154B03')  (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`datetime`)
 VALUES  ('2025-04-08 12:02:08',1102,'2G',21,'2025/4/8 0:00:00',null) ('2025-04-09 12:04:09',1105,'2G',47,'2025/4/9 0:00:00',null)

            */
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("INSERT INTO");
            var groups = list.GroupBy(t => new { t.SNO, t.PointNumber }).ToList();
            foreach (var tagGroup in groups)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"`point_data_{tagGroup.Key.SNO}_{tagGroup.Key.PointNumber}` "); //指定子表名称
                stringBuilder.Append($" USING `point_data` tags('{tagGroup.Key.SNO}','{tagGroup.Key.PointNumber}') "); //tags值
                stringBuilder.AppendLine(" (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`alarm_id`,`datetime`) "); //指定插入的字段
                stringBuilder.Append($" VALUES ");
                foreach (var item in tagGroup)
                {
                    // stringBuilder.Append($" (now,{item.Id},'{item.PointType}',{item.PointValue},'{item.Day}',");
                    stringBuilder.Append($" ('{item.ts:yyyy-MM-dd HH:mm:ss.fffffff}',{item.Id},'{item.PointType}',{item.PointValue},'{item.Day}',");
                    stringBuilder.Append(
                        $"{(item.AlarmId.HasValue ? item.AlarmId.Value : "null")},{(item.DateTime.HasValue ? "'" + item.DateTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'" : "null")})");
                }

                stringBuilder.AppendLine();
            }

            await _tdService.ExecuteSqlAsync(stringBuilder.ToString());
        }
        else if (type == 3)
        {
            // 最优
            _tdService.BatchInsertBulkCopy(list);
        }
        sw.Stop();
        Console.WriteLine(sw.Elapsed.TotalSeconds.ToString() + "s");


    }

    /// <summary>
    /// 往TdEngine中插入数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("InsertData")]
    public async Task CreateAsync(List<CreateTdDataDto> input)
    {
        var data = input.Adapt<List<PointDataEntity>>();
        data.ForEach(t =>
        {
            //t.ts = DateTime.Now;
            t.PointValue = Random.Shared.Next(10, 50);
        });
        await _tdService.BatchInsert(data);
    }

    /// <summary>
    /// 往TdEngine中插入数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("InsertDataWithSql")]
    public async Task CreateRawSqlAsync(List<CreateTdDataDto> input)
    {
        var data = input.Adapt<List<PointDataEntity>>();
        data.ForEach(t =>
        {
            //t.ts = DateTime.Now;
            t.PointValue = Random.Shared.Next(10, 50);
            t.AlarmId = 1234;
        });
        data.Last().AlarmId = null;
        data.Last().DateTime = DateTime.Now;
        if (data.Count > 0)
        {
            /*

INSERT INTO

`point_data_152_152A01`  USING `point_data` tags('152','152A01')  (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`datetime`)
 VALUES  ('2025-04-08 11:42:24',1100,'1D',46,'2025/4/8 0:00:00',null) ('2025-04-09 11:03:09',1103,'1D',22,'2025/4/9 0:00:00',null)

`point_data_152_152A02`  USING `point_data` tags('152','152A02')  (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`datetime`)
 VALUES  ('2025-04-08 12:04:08',1101,'3F',15,'2025/4/8 0:00:00',null) ('2025-04-09 12:03:09',1104,'3F',21,'2025/4/9 0:00:00',null)

`point_data_154_154B03`  USING `point_data` tags('154','154B03')  (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`datetime`)
 VALUES  ('2025-04-08 12:02:08',1102,'2G',21,'2025/4/8 0:00:00',null) ('2025-04-09 12:04:09',1105,'2G',47,'2025/4/9 0:00:00',null)

            */
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("INSERT INTO");
            var groups = data.GroupBy(t => new { t.SNO, t.PointNumber }).ToList();
            foreach (var tagGroup in groups)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"`point_data_{tagGroup.Key.SNO}_{tagGroup.Key.PointNumber}` "); //指定子表名称
                stringBuilder.Append($" USING `point_data` tags('{tagGroup.Key.SNO}','{tagGroup.Key.PointNumber}') "); //tags值
                stringBuilder.AppendLine(" (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`alarm_id`,`datetime`) "); //指定插入的字段
                stringBuilder.Append($" VALUES ");
                foreach (var item in tagGroup)
                {
                    // stringBuilder.Append($" (now,{item.Id},'{item.PointType}',{item.PointValue},'{item.Day}',");
                    stringBuilder.Append($" ('{item.ts:yyyy-MM-dd HH:mm:ss.fffffff}',{item.Id},'{item.PointType}',{item.PointValue},'{item.Day}',");
                    stringBuilder.Append(
                        $"{(item.AlarmId.HasValue ? item.AlarmId.Value : "null")},{(item.DateTime.HasValue ? "'" + item.DateTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'" : "null")})");
                }

                stringBuilder.AppendLine();
            }

            Console.WriteLine(stringBuilder.ToString());
            await _tdService.ExecuteSqlAsync(stringBuilder.ToString());
        }
    }

    /// <summary>
    /// 查询TdEngine中的数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("QueryData")]
    public Task<object> QueryDataAsync(QueryDataDto input)
    {
        return _tdService.QueryDataAsync(input);
    }

    /// <summary>
    /// 覆盖更新历史数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("UpdateHistoryData")]
    public async Task UpdateHistoryDataAsync()
    {
        /* await _repository.Context.Updateable<PointDataEntity>()
             .SetColumns(t => new PointDataEntity { PointValue = Random.Shared.Next() })
             .Where(t => t.Id == 1100)
             .ExecuteCommandAsync();*/
        // TdEngine中不支持直接update语句，如果需要更新历史数据，需要先知道历史数据的ts值，然后对其进行insert插入更新

        await _tdService.UpdateHistoryDataAsync();
    }

    /// <summary>
    /// 查询聚合数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("QueryAggregate")]
    public async Task<object> QueryAggregateAsync()
    {
        var data = await _tdService.QueryAggregateAsync();
        if (data.Count > 0)
        {
            return new TdAggregateDataDto()
            {
                Avg = data.First(t => t.Type == AgggegateTypeEnum.Avg).Val,
                Max = data.First(t => t.Type == AgggegateTypeEnum.Max).Val,
                Min = data.First(t => t.Type == AgggegateTypeEnum.Min).Val,
                MaxTime = data.First(t => t.Type == AgggegateTypeEnum.Max).Time,
                MinTime = data.First(t => t.Type == AgggegateTypeEnum.Min).Time
            };
        }

        return null;
    }

    [HttpGet("QueryAggregate2")]
    public async Task<object> QueryAggregate2Async()
    {
        var data = await _tdPrpository.QueryAggregateAsync<double>(t => t.SNO == "152" && t.PointNumber == "152A02", t => t.PointValue);
        return data;
    }

    [HttpGet("QueryAggregate3")]
    public async Task<object> QueryAggregate3Async()
    {
        var data = await _repository.QueryAggregateAsync<PointDataEntity, double>(t => t.Id > 1102, t => t.PointValue);
        return data;
    }

    /// <summary>
    /// 原生sql查询聚合数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("QueryAggregateRawSql")]
    public async Task<object> QueryAggregateRawSqlAsync()
    {
        var data = await _tdService.QueryAggregateWithSqlAsync();
        if (data.Count > 0)
        {
            return new TdAggregateDataDto()
            {
                Avg = data.First(t => t.Type == AgggegateTypeEnum.Avg).Val,
                Max = data.First(t => t.Type == AgggegateTypeEnum.Max).Val,
                Min = data.First(t => t.Type == AgggegateTypeEnum.Min).Val,
                MaxTime = data.First(t => t.Type == AgggegateTypeEnum.Max).Time,
                MinTime = data.First(t => t.Type == AgggegateTypeEnum.Min).Time
            };
        }

        return null;
    }

    [HttpGet("BackupDatabase")]
    public string BackupTdDatabase()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "backup", "td.sql");
            // var client = _repository.Context;
            //
            // client.DbMaintenance.BackupDataBase(client.Ado.Connection.Database, path);
            _tdService.Backup(path);
            return "success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}