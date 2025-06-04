using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;

namespace Furion.Demo.Core;

public interface ISugarRepository<T> : ISugarRepository, ISimpleClient<T> where T : class, new()
{
    Task<AggregateDataDto<TProperty>> QueryAggregateAsync<T, TProperty>(
        Expression<Func<T, bool>> whereExpression,
        Expression<Func<T, TProperty>> propertySelector
    ) where T : ITdPrimaryKey where TProperty : struct;
}