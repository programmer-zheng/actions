using SqlSugar;

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
}

