using SqlSugar;
using System;
using System.Linq.Expressions;
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

    public override async Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression)
    {
        if (typeof(IDeleted).IsAssignableFrom(typeof(T)))
        {
            // 伪删除
            return await this.Context.Deleteable<T>(whereExpression)
                .IsLogic()
                .ExecuteCommandAsync("IsDeleted", true, "DeletionTime") > 0;
        }
        return await base.DeleteAsync(whereExpression);
    }
}

