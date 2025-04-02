using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

public class SugarRepository<T> : SimpleClient<T>, ISugarRepository<T> where T : class, new()
{
    private const string MySqlConfigId = "MySql";
    private const string TdConfigId = "TDengine";
    public SugarRepository()
    {
        if (typeof(T).IsDefined(typeof(TraditionDataTableAttribute), true))
        {
            base.Context = SqlSugarSetup.SugarScope.GetConnectionScope(MySqlConfigId);
        }
        else if (typeof(T).IsDefined(typeof(TimingDataTableAttribute), true))
        {
            base.Context = SqlSugarSetup.SugarScope.GetConnectionScope(TdConfigId);
        }
    }

    private bool IsSoftDelete => typeof(ISoftDelete).IsAssignableFrom(typeof(T));

    private long? GetCurrentUserId()
    {
        if (long.TryParse(App.User?.FindFirst("UserId")?.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    public override bool Delete(T deleteObj)
    {
        if (IsSoftDelete)
        {
            return this.Context.Deleteable<T>(deleteObj)
                .IsLogic()
                .ExecuteCommand("IsDeleted", true, "DeletionTime", "DeleterUserId", GetCurrentUserId()) > 0;
        }
        return base.Delete(deleteObj);
    }

    public override bool Delete(List<T> deleteObjs)
    {
        if (IsSoftDelete)
        {
            return this.Context.Deleteable<T>(deleteObjs)
                .IsLogic()
                .ExecuteCommand("IsDeleted", true, "DeletionTime", "DeleterUserId", GetCurrentUserId()) > 0;
        }
        return base.Delete(deleteObjs);
    }

    public override bool Delete(Expression<Func<T, bool>> whereExpression)
    {
        if (IsSoftDelete)
        {
            return this.Context.Deleteable<T>(whereExpression)
                .IsLogic()
                .ExecuteCommand("IsDeleted", true, "DeletionTime", "DeleterUserId", GetCurrentUserId()) > 0;
        }
        return base.Delete(whereExpression);
    }

    public override async Task<bool> DeleteAsync(T deleteObj)
    {
        if (IsSoftDelete)
        {
            return await this.Context.Deleteable<T>(deleteObj)
                .IsLogic()
                .ExecuteCommandAsync("IsDeleted", true, "DeletionTime", "DeleterUserId", GetCurrentUserId()) > 0;
        }
        return await base.DeleteAsync(deleteObj);
    }

    public override async Task<bool> DeleteAsync(List<T> deleteObjs)
    {
        if (IsSoftDelete)
        {
            return await this.Context.Deleteable<T>(deleteObjs)
                .IsLogic()
                .ExecuteCommandAsync("IsDeleted", true, "DeletionTime", "DeleterUserId", GetCurrentUserId()) > 0;
        }
        return await base.DeleteAsync(deleteObjs);
    }

    public override async Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression)
    {
        if (IsSoftDelete)
        {
            // 伪删除
            return await this.Context.Deleteable<T>(whereExpression)
                .IsLogic()
                .ExecuteCommandAsync("IsDeleted", true, "DeletionTime", "DeleterUserId", GetCurrentUserId()) > 0;
        }
        return await base.DeleteAsync(whereExpression);
    }
}

