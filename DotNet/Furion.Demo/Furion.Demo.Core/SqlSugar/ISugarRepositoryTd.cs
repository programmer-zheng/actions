using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;

namespace Furion.Demo.Core;

public interface ISugarRepositoryTd<TDEntity> where TDEntity : class, ITdPrimaryKey, new()
{
    Task<AggregateDataDto<TProperty>> QueryAggregateAsync<TProperty>(
        Expression<Func<TDEntity, bool>> whereExpression,
        Expression<Func<TDEntity, TProperty>> propertySelector
    ) where TProperty : struct;
}

public class SugarRepositoryTd<TDEntity> : SugarRepository<TDEntity>, ISugarRepositoryTd<TDEntity> where TDEntity : class, ITdPrimaryKey, new()
{
    public SugarRepositoryTd() : base()
    {
    }

    public async Task<AggregateDataDto<TProperty>> QueryAggregateAsync<TProperty>(Expression<Func<TDEntity, bool>> whereExpression, Expression<Func<TDEntity, TProperty>> propertySelector)
        where TProperty : struct
    {
        
        if (Context.CurrentConnectionConfig.DbType != DbType.TDengine)
        {
            throw new NotSupportedException("只支持TdEngine数据库");
        }

        // 解析属性名称
        var memberExpression = (MemberExpression)propertySelector.Body;
        var propertyName = memberExpression.Member.Name;

        // 构建聚合查询
        var q1 = this.Context.Queryable<TDEntity>()
            .Where(whereExpression)
            .Select(it => new AggregateDataListDto<TProperty>
            {
                Val = SqlFunc.AggregateMax(SqlFunc.MappingColumn<TProperty>(propertyName)),
                Type = AgggegateTypeEnum.Max,
                Time = it.ts
            });

        var q2 = this.Context.Queryable<TDEntity>()
            .Where(whereExpression)
            .Select(it => new AggregateDataListDto<TProperty>
            {
                Val = SqlFunc.AggregateMin(SqlFunc.MappingColumn<TProperty>(propertyName)),
                Type = AgggegateTypeEnum.Min,
                Time = it.ts
            });

        var q3 = this.Context.Queryable<TDEntity>()
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