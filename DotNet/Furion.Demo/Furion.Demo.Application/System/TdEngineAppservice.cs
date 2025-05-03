using Furion.Demo.Application.System.Dtos;
using Furion.Demo.Core;
using SqlSugar.TDengine;
using StackExchange.Profiling.Internal;
using System.Text;

namespace Furion.Demo.Application.System;

[Route("api/Td")]
public class TdEngineAppservice : IDynamicApiController
{
    // https://docs.taosdata.com/develop/sql/

    // https://www.donet5.com/Home/Doc?typeId=2566

    private readonly ISugarRepository<PointDataEntity> _repository;

    private readonly ISugarRepositoryTd<PointDataEntity> _tdPrpository;

    private readonly ISqlSugarClient _sqlSugarClient;

    public TdEngineAppservice(ISugarRepository<PointDataEntity> repository, ISqlSugarClient sqlSugarClient, ISugarRepositoryTd<PointDataEntity> tdPrpository /*IServiceProvider serviceProvider*/)
    {
        _sqlSugarClient = sqlSugarClient;
        _tdPrpository = tdPrpository;
        //_sqlSugarClient = serviceProvider.GetKeyedService<SqlSugarClient>("Td");
        this._repository = repository;
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
        await _sqlSugarClient.Insertable(data)
            .SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}")
            //.SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}_{it.Day.ToString("yyyyMMdd")}")
            //.SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.Day.ToString("yyyyMMdd")}")
            .ExecuteCommandAsync();
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
        });
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
                stringBuilder.Append($"`point_data_{tagGroup.Key.SNO}_{tagGroup.Key.PointNumber}` ");//指定子表名称
                stringBuilder.Append($" USING `point_data` tags('{tagGroup.Key.SNO}','{ tagGroup.Key.PointNumber}') ");//tags值
                stringBuilder.AppendLine(" (`ts`,`id`,`pointtype`,`pointvalue`,`day`,`datetime`) ");//指定插入的字段
                stringBuilder.Append($" VALUES ");
                foreach (var item in tagGroup)
                {
                    stringBuilder.Append($" ('{item.ts:yyyy-MM-dd HH:mm:ss}',{item.Id},'{item.PointType}',{item.PointValue},'{item.Day}',null)");
                }
                stringBuilder.AppendLine();
            }
            Console.WriteLine(stringBuilder.ToString());
            await _sqlSugarClient.Ado.ExecuteCommandAsync(stringBuilder.ToString());
        }
    }

    /// <summary>
    /// 查询TdEngine中的数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("QueryData")]
    public async Task<object> QueryDataAsync(QueryTdDataDto input)
    {
        var list = await _repository.Context.Queryable<PointDataEntity>()//.AsQueryable()
            .Where(t => t.DateTime == null)
            .WhereIF(input.Sno > 0, t => t.SNO == (input.Sno.ToString()))
            .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
            .ToListAsync();
        return list;
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


        var old = await _repository.AsQueryable().Where(t => t.SNO == "152" && t.PointNumber == "152A01").FirstAsync();
        old.PointValue = Random.Shared.Next(100, 200);
        await _repository.Context.Insertable(old)
            // https://www.donet5.com/home/doc?masterId=1&typeId=1193
            //.InsertColumns(t => new { t.ts, t.PointValue })//指定列插入功能在TdEngine中不生效
            .SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}").ExecuteCommandAsync();
    }

    /// <summary>
    /// 查询聚合数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("QueryAggregate")]
    public async Task<object> QueryAggregateAsync()
    {
        /* var data = await repository.AsQueryable()
             .Where(t => t.ts >= Convert.ToDateTime("2025-04-08 12:04:08") && t.ts <= Convert.ToDateTime("2025-04-09 12:03:09"))
             .Select(t => new TdAggregateDataDto
             {
                 //Avg = SqlFunc.AggregateAvg(t.PointValue),
                 Avg = SqlFunc.Round<double>(SqlFunc.AggregateAvg(t.PointValue), 2),
                 Max = SqlFunc.AggregateMax(t.PointValue),
                 Min = SqlFunc.AggregateMin(t.PointValue)
             }).FirstAsync();*/

        var q1 = _repository.AsQueryable().Where(t => t.SNO == "152")
            .Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateMax(it.PointValue), Type = AgggegateTypeEnum.Max, Time = it.ts });
        var q2 = _repository.AsQueryable().Where(t => t.SNO == "152")
            .Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateMin(it.PointValue), Type = AgggegateTypeEnum.Min, Time = it.ts });
        var q3 = _repository.AsQueryable().Where(t => t.SNO == "152")
            .Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateAvg(it.PointValue), Type = AgggegateTypeEnum.Avg, Time = DateTime.Now });
        var data = await _repository.Context.UnionAll(q1, q2, q3).ToListAsync();
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
        var sql = """
                  SELECT * FROM ((select MAX(`pointvalue`) as `val`,`ts` as `time`,'Max' as `type` from point_data where sno = @sno)
                  union all (select MIN(`pointvalue`) as `val`,`ts` as `time`,'Min' as `type` from point_data where sno = @sno)
                  union all (select ROUND(AVG(`pointvalue`),2) as val,'' as `time`,'Avg' as `type` from point_data where sno = @sno)
                  )  ttt
                  """;

        var paramList = new List<SugarParameter>()
        {
            new SugarParameter("@sno", "152")
        };
        var data = await _repository.Context.Ado.SqlQueryAsync<TdAggregateDataListDto>(sql, paramList);
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
            var client = _repository.Context;
            var path = Path.Combine(AppContext.BaseDirectory, "td.sql");
            client.DbMaintenance.BackupDataBase(client.Ado.Connection.Database, path);
            return "success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}