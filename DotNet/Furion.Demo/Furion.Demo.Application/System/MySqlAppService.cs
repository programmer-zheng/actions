﻿using Furion.Demo.Application.System.Dtos;
using Furion.Demo.Core;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling.Internal;

namespace Furion.Demo.Application.System;

[Route("api/MySQL")]
public class MySqlAppService : IDynamicApiController
{

    private readonly ISugarRepository<PointEntity> repository;

    private readonly ISqlSugarClient _sqlSugarClient;

    public MySqlAppService(ISugarRepository<PointEntity> repository, ISqlSugarClient sqlSugarClient /*IServiceProvider serviceProvider*/)
    {
        this.repository = repository;
        _sqlSugarClient = sqlSugarClient;
        //_sqlSugarClient = serviceProvider.GetKeyedService<SqlSugarClient>("MySQL");
    }


    /// <summary>
    /// 往MySql中插入数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("InsertData")]
    public async Task CreateAsync(List<CreateTdDataDto> input)
    {
        var data = input.Adapt<List<PointEntity>>();
        data.ForEach(t => t.PointValue = Random.Shared.Next(10, 50));
        await repository.InsertRangeAsync(data);
    }

    /// <summary>
    /// 查询MySql中的数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("QueryData")]
    public async Task<object> QueryDataAsync(QueryTdDataDto input)
    {
        var list = await repository.AsQueryable()
            .WhereIF(input.Sno>0, t => t.SNO.Equals(input.Sno.ToString()))
            .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
            .ToListAsync();
        return list;

        /*        var list = await repository.Context.Queryable<PointEntity>()
                    .WhereIF(!input.Sno.IsNullOrWhiteSpace(), t => t.SNO.Equals(input.Sno))
                    .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
                    .Select(t => new { t.SNO, t.PointNumber, t.PointType, t.PointValue })
                    .ToListAsync();
                return list;*/
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [ActionName("DeleteData")]
    public async Task<bool> DeleteAsync(List<long> ids)
    {
        var result = await repository.DeleteAsync(t => ids.Contains(t.Id));
        return result;
    }

    [HttpGet("BackupDatabase")]
    public string BackupTdDatabase()
    {
        try
        {
            var client = repository.Context;
            var path = Path.Combine(AppContext.BaseDirectory, "mysql.sql");
            client.DbMaintenance.BackupDataBase(client.Ado.Connection.Database, path);
            return "success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
