using Furion.Demo.Application.System.Dtos;
using Furion.Demo.Core;
using Furion.Demo.Core.Dtos;
using Furion.Demo.Core.Service;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling.Internal;

namespace Furion.Demo.Application.System;

[Route("api/MySQL")]
public class MySqlAppService : IDynamicApiController
{
    private readonly ISugarRepository<PointEntity> _repository;

    private readonly MySqlService _mySqlService;

    public MySqlAppService(ISugarRepository<PointEntity> repository, MySqlService mySqlService /*IServiceProvider serviceProvider*/)
    {
        _repository = repository;
        _mySqlService = mySqlService;
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
        // await repository.InsertRangeAsync(data);
        await _mySqlService.BatchInsert(data);
    }

    /// <summary>
    /// 查询MySql中的数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("QueryData")]
    public async Task<object> QueryDataAsync(QueryDataDto input)
    {
        // return await _mySqlService.QueryDataAsync(input);

        var list = await _repository.Context.Queryable<PointEntity>()
            .WhereIF(input.Sno > 0, t => t.SNO.Equals(input.Sno))
            .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
            .Select(t => new { t.SNO, t.PointNumber, t.PointType, t.PointValue })
            .ToListAsync();
        return list;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [ActionName("DeleteData")]
    public async Task<bool> DeleteAsync(List<long> ids)
    {
        return await _mySqlService.DeleteAsync(ids);
    }

    [HttpGet("BackupDatabase")]
    public string BackupTdDatabase()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "backup", "mysql.sql");
            _mySqlService.Backup(path);
            return "success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}