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

namespace Furion.Demo.Core;

public static class SqlSugarSetup
{
    public static SqlSugarScope SugarScope { get; private set; }

    public static void AddSqlSugar(this IServiceCollection services)
    {
        // 添加SqlSugar
        var connectionConfigs = App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs");
        SqlSugarScope sugarClient = new(connectionConfigs, db =>
        {
            foreach (var item in connectionConfigs)
            {
                var dbProvider = db.GetConnection(item.ConfigId);
                SetupSugarAop(dbProvider);
                // 之所以不在此初始化数据库，是因为默认启动时，不会立即创建数据库
                //InitDatabase(dbProvider, item.DbType);
            }
        });
        SugarScope = sugarClient;
        services.AddSingleton<ISqlSugarClient>(sugarClient);
        services.AddScoped(typeof(ISugarRepository<>), typeof(SugarRepository<>));
        try
        {
            foreach (var item in connectionConfigs)
            {
                var sqlSugarProvider = sugarClient.GetConnection(item.ConfigId);
                InitDatabase(sqlSugarProvider, item.DbType);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="db"></param>
    /// <param name="dbType"></param>
    private static void InitDatabase(SqlSugarProvider db, DbType dbType)
    {
        var entityTypes = App.EffectiveTypes
                       .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false))
                       .WhereIF(dbType == DbType.TDengine, u => u.IsDefined(typeof(TimingDataTableAttribute), true))
                       .WhereIF(dbType == DbType.MySql, u => u.IsDefined(typeof(TraditionDataTableAttribute), true))
                       .ToArray();
        var databaseName = db.Ado.Connection.Database;
        db.DbMaintenance.CreateDatabase(databaseName);
        db.CodeFirst.InitTables(entityTypes);
    }

    /// <summary>
    /// 配置Sugar AOP
    /// </summary>
    /// <param name="db"></param>
    private static void SetupSugarAop(SqlSugarProvider db)
    {
        db.QueryFilter.AddTableFilter<IDeleted>(t => t.IsDeleted == false);
        db.Aop.OnLogExecuting = (sql, paras) =>
        {
            var rawSql = UtilMethods.GetNativeSql(sql, paras);
            var log = $"【{DateTime.Now} Execute SQL】【{db.CurrentConnectionConfig.DbType}】\r\n{sql}\r\n";
            var originColor = Console.ForegroundColor;
            if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Green;
            if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Yellow;
            if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(log);
            Console.ForegroundColor = originColor;
        };

        db.Aop.OnError = (sugarException) =>
        {
            if (sugarException.Parametres == null) return;
            var rawSql = UtilMethods.GetNativeSql(sugarException.Sql, (SugarParameter[])sugarException.Parametres);
            var log = $"【{DateTime.Now} Error SQL】【{db.CurrentConnectionConfig.DbType}】\r\n{rawSql}\r\n";
            var originColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(log);
            Console.ForegroundColor = originColor;
        };
    }
}
