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

    public static void AddSqlSugar(this IServiceCollection services)
    {
        //var sugarClient = DbContext.Instance;
        // 添加SqlSugar
        var connectionConfigs = App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs");
        SqlSugarClient sugarClient = null;
        try
        {
            foreach (var item in connectionConfigs)
            {
                var entityTypes = App.EffectiveTypes
                       .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false))
                       .WhereIF(item.DbType == DbType.TDengine, u => u.GetCustomAttributes<TimingDataTableAttribute>().Any())
                       .WhereIF(item.DbType == DbType.MySql, u => u.GetCustomAttributes<TraditionDataTableAttribute>().Any())
                       .ToList();
                if (item.DbType == DbType.TDengine)
                {
                    sugarClient = SetUpTd(services, item);

                }
                else if (item.DbType == DbType.MySql)
                {
                    sugarClient = SetUpMySQL(services, item);
                }
                var sqlSugarProvider = sugarClient.GetConnection(item.ConfigId);
                var currentDbName = sqlSugarProvider.Ado.Connection.Database;// 当前连接的数据库名称
                sqlSugarProvider.DbMaintenance.CreateDatabase(currentDbName);


                foreach (var entityType in entityTypes)
                {

                    sqlSugarProvider.CodeFirst.InitTables(entityType);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static SqlSugarClient SetUpMySQL(IServiceCollection services, ConnectionConfig item)
    {
        SqlSugarClient sugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            DbType = SqlSugar.DbType.MySql,
            ConnectionString = item.ConnectionString,
            IsAutoCloseConnection = true,
        });
        services.AddKeyedSingleton("MySQL", sugarClient);
        services.AddScoped(typeof(ISugarRepository<>), typeof(SugarRepository<>));
        return sugarClient;
    }

    private static SqlSugarClient SetUpTd(IServiceCollection services, ConnectionConfig item)
    {
        SqlSugarClient sugarClient;
        sugarClient = new SqlSugarClient(new ConnectionConfig()
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
        services.AddKeyedSingleton("Td", sugarClient);
        services.AddScoped(typeof(ITdSugarRepository<>), typeof(TdSugarRepository<>));
        return sugarClient;
    }
}
