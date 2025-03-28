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
        //var sugarClient = DbContext.Instance;
        // 添加SqlSugar
        var connectionConfigs = App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs");
        SqlSugarScope sugarClient = new(connectionConfigs, db =>
        {
            foreach (var item in connectionConfigs)
            {
                var dbProvider = db.GetConnectionScope(item.ConfigId);
                SetupSugarAop(dbProvider);
            }
        });
        SugarScope = sugarClient;
        services.AddSingleton<ISqlSugarClient>(sugarClient);
        services.AddScoped(typeof(ISugarRepository<>), typeof(SugarRepository<>));
        try
        {
            foreach (var item in connectionConfigs)
            {
                var entityTypes = App.EffectiveTypes
                       .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false))
                       .WhereIF(item.DbType == DbType.TDengine, u => u.IsDefined(typeof(TimingDataTableAttribute), true))
                       .WhereIF(item.DbType == DbType.MySql, u => u.IsDefined(typeof(TraditionDataTableAttribute), true))
                       .ToList();
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

    private static void SetupSugarAop(SqlSugarScopeProvider db)
    {
        db.Aop.OnLogExecuting = (sql, paras) =>
        {
            var rawSql = UtilMethods.GetNativeSql(sql, paras);
            var log = $"【{DateTime.Now}-{db.CurrentConnectionConfig.DbType} Execute SQL】\r\n{sql}\r\n";
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
            var log = $"【{DateTime.Now}-{db.CurrentConnectionConfig.DbType} Error SQL】\r\n{rawSql}\r\n";
            var originColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(log);
            Console.ForegroundColor = originColor;
        };
    }
}
