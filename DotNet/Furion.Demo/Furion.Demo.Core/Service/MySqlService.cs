using System.Collections.Generic;
using System.Threading.Tasks;
using Furion.Demo.Core.Dtos;
using Furion.DependencyInjection;
using SqlSugar;

namespace Furion.Demo.Core.Service;

public class MySqlService : IScoped
{
    private readonly ITenant _tenant;

    public MySqlService(ITenant tenant)
    {
        _tenant = tenant;
    }

    public async Task BatchInsert(List<PointEntity> data)
    {
        // var db = _tenant.GetConnectionScope(Consts.MySqlConfigId);
        await _tenant.InsertableWithAttr(data).ExecuteCommandAsync();
    }

    public async Task<object> QueryDataAsync(QueryDataDto input)
    {
        var db = _tenant.GetConnectionScope(Consts.MainConfigId);
        var list = await db.Queryable<PointEntity>()
            .WhereIF(input.Sno > 0, t => t.SNO.Equals(input.Sno.ToString()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.PointNumber), t => t.PointNumber.Equals(input.PointNumber))
            .ToListAsync();
        return list;
    }

    public async Task<bool> DeleteAsync(List<long> ids)
    {
        var db = _tenant.GetConnectionScope(Consts.MainConfigId);
        var count = await db.Deleteable<PointEntity>(t => ids.Contains(t.Id)).ExecuteCommandAsync();
        return count > 0;
    }

    public Task Backup(string filePath)
    {
        var db = _tenant.GetConnectionScope(Consts.MainConfigId);
        db.DbMaintenance.BackupDataBase(db.Ado.Connection.Database, filePath);
        return Task.CompletedTask;
    }
}