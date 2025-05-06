using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.Demo.Core;

public class SugarRepository<T> : SimpleClient<T>, ISugarRepository<T> where T : class, new()
{

    public SugarRepository()
    {
        if (typeof(T).IsDefined(typeof(TraditionDataTableAttribute), true))
        {
            base.Context = SqlSugarSetup.SugarScope.GetConnectionScope(Consts.MySqlConfigId);
        }
        else if (typeof(T).IsDefined(typeof(TimingDataTableAttribute), true))
        {
            base.Context = SqlSugarSetup.SugarScope.GetConnectionScope(Consts.TdConfigId);
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

    public async Task<AggregateDataDto<TProperty>> QueryAggregateAsync<T1, TProperty>(Expression<Func<T1, bool>> whereExpression, Expression<Func<T1, TProperty>> propertySelector)
        where T1 : ITdPrimaryKey where TProperty : struct
    {
        // 解析属性名称
        var memberExpression = (MemberExpression)propertySelector.Body;
        var propertyName = memberExpression.Member.Name;

        // 构建聚合查询
        var q1 = this.Context.Queryable<T1>()
            .Where(whereExpression)
            .Select(it => new AggregateDataListDto<TProperty>
            {
                Val = SqlFunc.AggregateMax(SqlFunc.MappingColumn<TProperty>(propertyName)),
                Type = AgggegateTypeEnum.Max,
                Time = it.ts
            });

        var q2 = this.Context.Queryable<T1>()
            .Where(whereExpression)
            .Select(it => new AggregateDataListDto<TProperty>
            {
                Val = SqlFunc.AggregateMin(SqlFunc.MappingColumn<TProperty>(propertyName)),
                Type = AgggegateTypeEnum.Min,
                Time = it.ts
            });

        var q3 = this.Context.Queryable<T1>()
            .Where(whereExpression)
            .Select(it => new AggregateDataListDto<TProperty>
            {
                Val = SqlFunc.AggregateAvg(SqlFunc.MappingColumn<TProperty>(propertyName)),
                Type = AgggegateTypeEnum.Avg,
                Time = DateTime.Now
            });

        var data = await this.Context.UnionAll(q1, q2, q3).ToListAsync();

        return data.Count > 0
            ? new AggregateDataDto<TProperty>
            {
                Avg = data.First(t => t.Type == AgggegateTypeEnum.Avg).Val,
                Max = data.First(t => t.Type == AgggegateTypeEnum.Max).Val,
                Min = data.First(t => t.Type == AgggegateTypeEnum.Min).Val,
                MaxTime = data.First(t => t.Type == AgggegateTypeEnum.Max).Time,
                MinTime = data.First(t => t.Type == AgggegateTypeEnum.Min).Time
            }
            : null;
    }
}