using Furion.Demo.Application.System.Dtos;
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
        //await _sqlSugarClient.Insertable(data).ExecuteCommandAsync();
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
            .WhereIF(!input.Sno.IsNullOrWhiteSpace(), t => t.SNO.Equals(input.Sno))
            .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
            .ToListAsync();
        return list;
    }

    [HttpGet("BackupDatabase")]
    public string BackupTdDatabase()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "mysql.sql");
            _sqlSugarClient.DbMaintenance.BackupDataBase(_sqlSugarClient.Ado.Connection.Database, path);
            return "success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
