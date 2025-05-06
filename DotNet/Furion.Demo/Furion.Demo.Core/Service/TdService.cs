using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Furion.Demo.Core.Dtos;
using Furion.DependencyInjection;
using SqlSugar;
using SqlSugar.TDengine;

namespace Furion.Demo.Core.Service;

public class TdService : ISingleton
{
    private readonly ITenant _tenant;

    public TdService(ITenant tenant)
    {
        _tenant = tenant;
    }

    public async Task InsertAndUpdate(PointDataEntity data)
    {
        var oldData = await _tenant.QueryableWithAttr<PointDataEntity>().Where(t => t.SNO == data.SNO && t.ts < data.ts).OrderByDescending(t => t.ts).FirstAsync();
        if (oldData != null)
        {
            var aggregateData = await QueryAggregateAsync(data.SNO, data.PointNumber);
            if (aggregateData.Count > 0)
            {
                oldData.AvgValue = aggregateData.First(t => t.Type == AgggegateTypeEnum.Avg).Val;
                oldData.MaxValue = aggregateData.First(t => t.Type == AgggegateTypeEnum.Max).Val;
                oldData.MinValue = aggregateData.First(t => t.Type == AgggegateTypeEnum.Min).Val;
            }

            oldData.DateTime = DateTime.Now;
            await _tenant.InsertableWithAttr(oldData)
                .SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}")
                .ExecuteCommandAsync();
        }

        await _tenant.InsertableWithAttr(data)
            .SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}")
            .ExecuteCommandAsync();
    }


    public async Task BatchInsert(List<PointDataEntity> data)
    {
        // var db = _tenant.GetConnectionScope(Consts.MySqlConfigId);

        await _tenant.InsertableWithAttr(data)
            .SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}")
            //.SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}_{it.Day.ToString("yyyyMMdd")}")
            //.SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.Day.ToString("yyyyMMdd")}")
            .ExecuteCommandAsync();
    }

    public async Task ExecuteSqlAsync(string sql)
    {
        var db = _tenant.GetConnectionScope(Consts.TdConfigId);
        await db.Ado.ExecuteCommandAsync(sql);
    }

    public async Task<List<TdAggregateDataListDto>> QueryAggregateAsync(string sno = "152", string pointNumber = null)
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
        Expression<Func<PointDataEntity, bool>> expression = Expressionable.Create<PointDataEntity>() //创建表达式
            .And(it => it.SNO == sno)
            .AndIF(!string.IsNullOrWhiteSpace(pointNumber), it => it.PointNumber == pointNumber)
            .ToExpression(); //注意 这一句 不能少

        var q1 = _tenant.QueryableWithAttr<PointDataEntity>().Where(expression)
            .Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateMax(it.PointValue), Type = AgggegateTypeEnum.Max, Time = it.ts });

        var q2 = _tenant.QueryableWithAttr<PointDataEntity>().Where(expression)
            .Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateMin(it.PointValue), Type = AgggegateTypeEnum.Min, Time = it.ts });

        var q3 = _tenant.QueryableWithAttr<PointDataEntity>().Where(expression)
            .Select(it => new TdAggregateDataListDto { Val = SqlFunc.AggregateAvg(it.PointValue), Type = AgggegateTypeEnum.Avg, Time = DateTime.Now });

        var db = _tenant.GetConnectionScope(Consts.TdConfigId);
        var data = await db.UnionAll(q1, q2, q3).ToListAsync();
        return data;
    }

    public async Task<object> QueryDataAsync(QueryDataDto input)
    {
        var list = await _tenant.QueryableWithAttr<PointDataEntity>().AsTDengineSTable() //.AsQueryable()
            .Where(t => t.DateTime == null)
            .WhereIF(input.Sno > 0, t => t.SNO == (input.Sno.ToString()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.PointNumber), t => t.PointNumber.Equals(input.PointNumber))
            .ToListAsync();
        return list;
    }

    public async Task UpdateHistoryDataAsync()
    {
        var old = await _tenant.QueryableWithAttr<PointDataEntity>().Where(t => t.SNO == "152" && t.PointNumber == "152A01").FirstAsync();
        old.PointValue = Random.Shared.Next(100, 200);
        await _tenant.InsertableWithAttr<PointDataEntity>(old)
            // https://www.donet5.com/home/doc?masterId=1&typeId=1193
            //.InsertColumns(t => new { t.ts, t.PointValue })//指定列插入功能在TdEngine中不生效
            .SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}").ExecuteCommandAsync();
    }

    public async Task<List<TdAggregateDataListDto>> QueryAggregateWithSqlAsync()
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
        var db = _tenant.GetConnectionScope(Consts.TdConfigId);
        var list = await db.Ado.SqlQueryAsync<TdAggregateDataListDto>(sql, paramList);
        return list;
    }

    public Task Backup(string filePath)
    {
        var db = _tenant.GetConnectionScope(Consts.TdConfigId);
        db.DbMaintenance.BackupDataBase(db.Ado.Connection.Database, filePath);
        return Task.CompletedTask;
    }
}