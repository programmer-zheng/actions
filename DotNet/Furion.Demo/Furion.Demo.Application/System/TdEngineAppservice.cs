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

    public TdEngineAppservice(ISugarRepository<PointDataEntity> repository,ISqlSugarClient sqlSugarClient /*IServiceProvider serviceProvider*/)
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
        data.ForEach(t => t.PointValue = Random.Shared.Next(10, 50));
        await _sqlSugarClient.Insertable(data)
            .SetTDengineChildTableName((stableName, it) => $"{stableName}_{it.SNO}_{it.PointNumber}")
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

    [HttpGet("BackupDatabase")]
    public string BackupTdDatabase()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "tdbackup.bak");
            _sqlSugarClient.DbMaintenance.BackupDataBase(_sqlSugarClient.Ado.Connection.Database, path);
            return "success";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
