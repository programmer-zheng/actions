using Furion.Demo.Application.System.Dtos;
using Furion.Demo.Core;
using Furion.Demo.Core.Dtos;
using Furion.Demo.Core.Service;
using SqlSugar.DbConvert;
using StackExchange.Profiling.Internal;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Furion.Demo.Application.SqlSugar;

[Route("api/MySQL")]
public class MySqlAppService : IDynamicApiController, ITransient
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
    /// 测试枚举转换
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("TestEnumConvert")]
    public Task<string> TestEnumConvert(CreatePointDto input)
    {
        var xx = Newtonsoft.Json.JsonConvert.SerializeObject(input,new Newtonsoft.Json.Converters.StringEnumConverter());

        return Task.FromResult<string>(xx);
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
        data.ForEach(t =>
        {
            t.PointValue = Random.Shared.Next(10, 50);
            t.Id = SnowFlakeSingle.instance.NextId();
        });
        // await repository.InsertRangeAsync(data);
        await _mySqlService.BatchInsert(data);
    }

    /// <summary>
    /// 查询MySql中的数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("QueryData")]
    [FormatFilter]
    public async Task<object> QueryDataAsync(QueryDataDto input)
    {
        // return await _mySqlService.QueryDataAsync(input);

        var list = await _repository.Context.Queryable<PointEntity>()
            .WhereIF(input.Sno > 0, t => t.SNO.Equals(input.Sno))
            .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
            .ToListAsync();

        return list.Adapt<List<PointListDto>>();
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

public class PointListDto
{
    public long Id { get; set; }

    public string SNO { get; set; }

    public string PointType { get; set; }

    public string PointNumber { get; set; }

    public double PointValue { get; set; }

    public SensorType SensorType { get; set; }

    public string SensorName => SensorType.ToString();

    public string SensorDescription => EnumHelper.GetEnumDescription(SensorType);
}

public class CreatePointDto
{
    public string PointNumber { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SensorType? SensorType { get; set; }
}