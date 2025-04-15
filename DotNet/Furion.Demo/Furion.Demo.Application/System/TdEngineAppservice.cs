using Furion.Demo.Application.System.Dtos;
using Furion.Demo.Core;
using Furion.HttpRemote;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar.TDengine;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Application.System;

[Route("api/Td")]
public class TdEngineAppservice : IDynamicApiController
{
    // https://docs.taosdata.com/develop/sql/

    // https://www.donet5.com/Home/Doc?typeId=2566

    private readonly ISugarRepository<PointDataEntity> repository;

    private readonly ISqlSugarClient _sqlSugarClient;

    public TdEngineAppservice(ISugarRepository<PointDataEntity> repository, ISqlSugarClient sqlSugarClient /*IServiceProvider serviceProvider*/)
    {
        _sqlSugarClient = sqlSugarClient;
        //_sqlSugarClient = serviceProvider.GetKeyedService<SqlSugarClient>("Td");
        this.repository = repository;
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
    /// 查询TdEngine中的数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("QueryData")]
    public async Task<object> QueryDataAsync(QueryTdDataDto input)
    {
        var list = await repository.AsQueryable()
            .WhereIF(!input.Sno.IsNullOrWhiteSpace(), t => t.SNO.Equals(input.Sno))
            .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
            .ToListAsync();
        return list;
    }

    [HttpGet("UpdateHistoryData")]
    public async Task UpdateHistoryDataAsync()
    {
        await repository.Context.Updateable<PointDataEntity>()
            .SetColumns(t => new PointDataEntity { PointValue = Random.Shared.Next() })
            .Where(t => t.Id == 1100)
            .ExecuteCommandAsync();
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

        var q1 = repository.AsQueryable().Where(t => t.SNO == "152").Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateMax(it.PointValue), Type = AgggegateTypeEnum.Max, Time = it.ts });
        var q2 = repository.AsQueryable().Where(t => t.SNO == "152").Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateMin(it.PointValue), Type = AgggegateTypeEnum.Min, Time = it.ts });
        var q3 = repository.AsQueryable().Where(t => t.SNO == "152")
            .Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateAvg(it.PointValue), Type = AgggegateTypeEnum.Avg, Time = DateTime.Now });
        var data = await repository.Context.UnionAll(q1, q2, q3).ToListAsync();
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
        var data = await repository.Context.Ado.SqlQueryAsync<TdAggregateDataListDto>(sql, paramList);
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
            var client = repository.Context;
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