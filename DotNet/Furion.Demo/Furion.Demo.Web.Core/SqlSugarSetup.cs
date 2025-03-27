using Furion.Demo.Core;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using SqlSugar.DbConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TDengine.TMQ;

namespace Furion.Demo.Web.Core;

public static class SqlSugarSetup
{

    public static void AddSqlSugarTdSupport(this IServiceCollection services)
    {
        //var sugarClient = DbContext.Instance;
        // 添加SqlSugar
        services.AddScoped(typeof(ISugarRepository<>), typeof(SugarRepository<>));
        var connectionConfigs = App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs");
        try
        {
            foreach (var item in connectionConfigs)
            {
                if (item.DbType == DbType.TDengine)
                {
                    var sugarClient = new SqlSugarClient(new ConnectionConfig()
                    {
                        DbType = SqlSugar.DbType.TDengine,
                        ConnectionString = item.ConnectionString,
                        IsAutoCloseConnection = true,
                        ConfigureExternalServices = new ConfigureExternalServices()
                        {
                            EntityService = (property, column) =>
                            {
                                if (column.SqlParameterDbType == null)
                                {
                                    //需要给列加上通用转换，这样实体就不需要一个一个转了 
                                    column.SqlParameterDbType = typeof(CommonPropertyConvert);
                                }
                            }
                        }
                    });
                    services.AddSingleton<ISqlSugarClient>(sugarClient); // 单例注册
                    var sqlSugarProvider = sugarClient.GetConnection(item.ConfigId);
                    var currentDbName = sqlSugarProvider.Ado.Connection.Database;// 当前连接的数据库名称
                    sqlSugarProvider.DbMaintenance.CreateDatabase(currentDbName);

                    var entityTypes = App.EffectiveTypes
                        .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false))
                        .ToList();
                    foreach (var entityType in entityTypes)
                    {

                        sqlSugarProvider.CodeFirst.InitTables(entityType);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
