using Furion.Demo.Core;
using Furion.Logging;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections;
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
                item.MoreSettings = new ConnMoreSettings()
                {
                    IsNoReadXmlDescription = true, // 禁止读取XML中备注,true是禁用
                };
                var dbProvider = db.GetConnection(item.ConfigId);
                SetupSugarAop(dbProvider);
                // 之所以不在此初始化数据库，是因为默认启动时，不会立即创建数据库
                //InitDatabase(dbProvider, item.DbType);
            }
        });
        SugarScope = sugarClient;
        services.AddScoped<ISqlSugarClient>(t => sugarClient);
        services.AddScoped<ITenant>(t => sugarClient);
        services.AddScoped(typeof(ISugarRepository<>), typeof(SugarRepository<>));
        services.AddScoped(typeof(ISugarRepositoryTd<>), typeof(SugarRepositoryTd<>));
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
            .WhereIF(dbType == DbType.Sqlite, u => u.IsDefined(typeof(TraditionDataTableAttribute), true))
            .ToArray();
        var databaseName = db.Ado.Connection.Database;
        db.DbMaintenance.CreateDatabase(databaseName);
        foreach (var entityType in entityTypes)
        {
            var tableName = db.EntityMaintenance.GetEntityInfo(entityType).DbTableName;
            if (db.DbMaintenance.IsAnyTable(tableName))
            {
                db.DbMaintenance.DropTable(tableName);
            }
        }
        db.CodeFirst.InitTables(entityTypes);

        var seedDataTypes = App.EffectiveTypes
                   .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                               && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(ISqlSugarEntitySeedData<>))))
                   .ToList();
        foreach (var seedType in seedDataTypes)
        {

            var entityType = seedType.GetInterfaces().First().GetGenericArguments().First();
            if (dbType == DbType.MySql) // 默认库（有系统表特性、没有日志表和租户表特性）
            {
                if (entityType.GetCustomAttribute<TraditionDataTableAttribute>() == null &&
                    (entityType.GetCustomAttribute<TenantAttribute>() != null))
                    continue;
            }
            else
            {
                continue;
            }
            var instance = Activator.CreateInstance(seedType);
            var hasDataMethod = seedType.GetMethod("HasData");
            var seedData = ((IEnumerable)hasDataMethod?.Invoke(instance, null))?.Cast<object>();
            if (seedData == null)
                continue;

            var entityInfo = db.EntityMaintenance.GetEntityInfo(entityType);
            if (entityInfo.Columns.Any(u => u.IsPrimarykey))
            {
                // 按主键进行批量增加和更新
                var storage = db.StorageableByObject(seedData.ToList()).ToStorage();
                storage.AsInsertable.ExecuteCommand();
                //if (seedType.GetCustomAttribute<IgnoreUpdateSeedAttribute>() == null) // 有忽略更新种子特性时则不更新
                //    storage.AsUpdateable
                //        .IgnoreColumns(entityInfo.Columns.Where(c => c.PropertyInfo.GetCustomAttribute<IgnoreUpdateSeedColumnAttribute>() != null).Select(c => c.PropertyName).ToArray())
                //        .ExecuteCommand();
            }
            else
            {
                // 无主键则只进行插入
                if (!db.Queryable(entityInfo.DbTableName, entityInfo.DbTableName).Any())
                    db.InsertableByObject(seedData.ToList()).ExecuteCommand();
            }
        }
    }

    /// <summary>
    /// 配置Sugar AOP
    /// </summary>
    /// <param name="db"></param>
    private static void SetupSugarAop(SqlSugarProvider db)
    {
        db.QueryFilter.AddTableFilter<ISoftDelete>(t => t.IsDeleted == false);
        db.Aop.OnLogExecuting = (sql, paras) =>
        {
            var rawSql = UtilMethods.GetNativeSql(sql, paras);
            var log = $"【{DateTime.Now} Execute SQL】【{db.CurrentConnectionConfig.DbType}】\r\n{rawSql}\r\n";
            var originColor = Console.ForegroundColor;
            if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Green;
            if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Yellow;
            if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(log);
            Log.Information(log);
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
            Log.Information(log);
            Console.ForegroundColor = originColor;
        };
    }
}