using Dm;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using SqlSugar.TDengine;
using System;
using System.Diagnostics;
using System.Text;

//using TDengine.TMQ;

namespace sqlsugar_test;

[STableAttribute(STableName = "BulkDemo2", Tag1 = nameof(Sno), Tag2 = nameof(PointNumber))]
//[STableAttribute(STableName = "BulkDemo2", Tag1 = nameof(Sno))]
public class BulkDemo2
{
    [SugarColumn(IsPrimaryKey = true, SqlParameterDbType = typeof(DateTime19))]
    public DateTime Ts { get; set; }

    public bool Boolean { get; set; }
    public string Sno { get; set; }
    public string PointNumber { get; set; }
    
    [SugarColumn(ColumnName = "double_value")]
    public double Double { get; set; } 

    
    [SugarColumn(ColumnName = "null_double")]
    public double? NullableDouble { get; set; }

    public DateTime? Dt { get; set; }
}

internal class Program
{
    static async Task Main(string[] args)
    {
        var tdEngineServerIp = "127.0.0.1";
        // tdEngineServerIp = "192.168.110.128";
        var services = new ServiceCollection();
        var db = new SqlSugarClient(new ConnectionConfig()
        {
            ConfigId = "tdengine",
            DbType = SqlSugar.DbType.TDengine,
            ConnectionString = $"Host={tdEngineServerIp};Port=6030;Username=root;Password=taosdata;Database=sqlsugartest;TsType=config_ns",
            IsAutoCloseConnection = true,
        });

        services.AddScoped<ITenant>(t => db);
        services.AddScoped<ISqlSugarClient>(t =>
        {
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConfigId = "tdengine",
                DbType = SqlSugar.DbType.TDengine,
                ConnectionString = $"Host={tdEngineServerIp};Port=6030;Username=root;Password=taosdata;Database=sqlsugartest;TsType=config_ns",
                IsAutoCloseConnection = true,
            });
            return db;
        });

        db.DbMaintenance.CreateDatabase();
        if (db.DbMaintenance.IsAnyTable<BulkDemo2>())
        {
            db.DbMaintenance.DropTable<BulkDemo2>();
        }

        db.CodeFirst.InitTables(typeof(BulkDemo2));


        var serviceProvider = services.BuildServiceProvider();

        int i = 0;
        while (true)
        {
            i++;
            //await Insertable(serviceProvider, i);
            //await BulkCopyAsync(serviceProvider, i);
            // await RawSqlAsync(serviceProvider, i);
            // await GenSqlAndExecuteAsync(serviceProvider, i);
            await ExecuteSqlAsync(serviceProvider, i);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    static List<BulkDemo2> GetData()
    {
        var now = DateTime.Now;
        var list = new List<BulkDemo2>();
        for (int x = 1; x <= 254; x++)
        {
            for (int y = 1; y <= 40; y++)
            {
                list.Add(new BulkDemo2()
                {
                    Ts = now, // now.AddMicroseconds(i * x * y),
                    Sno = x.ToString("D3"),
                    PointNumber = y.ToString("D5"),
                    Double = Random.Shared.NextDouble()
                });
            }
        }

        return list;
    }

    static async Task ExecuteSqlAsync(IServiceProvider serviceProvider, int i)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            Console.WriteLine($"第{i}次：");
            // 准备数据
            var sw = Stopwatch.StartNew();

            var list = GetData();
            sw.Stop();
            Console.WriteLine($"{list.Count}条 数据准备完毕，用时  {sw.Elapsed.TotalSeconds} s");
            sw.Restart();
            var result = await db.Insertable<BulkDemo2>(list).ToSTableChild((stable, it) => $"{stable}_{it.Sno}_{it.PointNumber}").ExecuteInsertAsync(5000);
            sw.Stop();
            Console.WriteLine($"插入 {result}条 用时 {sw.Elapsed.TotalSeconds} s");

            var count = await db.Queryable<BulkDemo2>().CountAsync();
            Console.WriteLine($"当前库中有{count}条数据");

            Console.WriteLine();
        }
    }

    static async Task GenSqlAndExecuteAsync(IServiceProvider serviceProvider, int i)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            // 准备数据
            var sw = Stopwatch.StartNew();

            var list = GetData();
            sw.Stop();
            Console.WriteLine($"数据准备完毕，用时  {sw.Elapsed.TotalSeconds} s");
            sw.Restart();
            var result = db.Insertable<BulkDemo2>(list).ToSTableChild((stable, it) => $"{stable}_{it.Sno}_{it.PointNumber}").GetExecuteSql(5000);
            sw.Stop();
            Console.WriteLine($"获取插入 插入{list.Count}条 用时 {sw.Elapsed.TotalSeconds} s");
            foreach (var item in result)
            {
                Console.WriteLine();
                sw.Restart();
                await db.Ado.ExecuteCommandAsync(item);
                sw.Stop();
                Console.WriteLine($"执行sql用时 {sw.Elapsed.TotalSeconds} s");
            }

            Console.WriteLine($"{i} 数据准备完毕");
        }
    }

    static async Task RawSqlAsync(IServiceProvider serviceProvider, int i)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var _db = serviceProvider.GetRequiredService<ISqlSugarClient>();
            // 准备数据
            var list = GetData();

            Console.WriteLine($"{i} 数据准备完毕");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("INSERT INTO");
            var groups = list.GroupBy(t => new { t.Sno, t.PointNumber }).ToList();
            Console.WriteLine($"分组数量:{groups.Count}");
            var sw = Stopwatch.StartNew();
            foreach (var tagGroup in groups)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"`bulkdemo2_{tagGroup.Key.Sno}_{tagGroup.Key.PointNumber}` "); //指定子表名称
                stringBuilder.Append($" USING `bulkdemo2` tags('{tagGroup.Key.Sno}','{tagGroup.Key.PointNumber}') "); //tags值
                stringBuilder.AppendLine(" (`ts`,`boolean`) "); //指定插入的字段
                stringBuilder.Append($" VALUES ");
                foreach (var item in tagGroup)
                {
                    // stringBuilder.Append($" (now,{item.Id},'{item.PointType}',{item.PointValue},'{item.Day}',");
                    stringBuilder.Append($" ('{item.Ts:yyyy-MM-dd HH:mm:ss.fffffff}',0)");
                }
            }

            sw.Stop();
            Console.WriteLine($"第{i}次 rawsql 生成sql {sw.Elapsed.TotalSeconds} s");
            sw.Restart();
            //Console.WriteLine(stringBuilder.ToString());
            await _db.Ado.ExecuteCommandAsync(stringBuilder.ToString());

            sw.Stop();
            Console.WriteLine($"第{i}次 rawsql 插入{list.Count}条 用时 {sw.Elapsed.TotalSeconds} s");

            var count = _db.Queryable<BulkDemo2>().Count();
            Console.WriteLine($"查询到{count}条数据");
        }

        Console.WriteLine();
    }

    static async Task Insertable(IServiceProvider serviceProvider, int i)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var _db = serviceProvider.GetRequiredService<ISqlSugarClient>();
            // 准备数据
            var list = GetData();

            Console.WriteLine($"{i} 数据准备完毕");
            var sw = Stopwatch.StartNew();
            await _db.Insertable<BulkDemo2>(list)
                .SetTDengineChildTableName((tbname, it) => $"{tbname}_{it.Sno}_{it.PointNumber}")
                .ExecuteCommandAsync();

            sw.Stop();
            Console.WriteLine($"第{i}次 bulkcopy 插入{list.Count}条 用时 {sw.Elapsed.TotalSeconds} s");

            var count = _db.Queryable<BulkDemo2>().Count();
            Console.WriteLine($"查询到{count}条数据");
        }
    }

    static async Task BulkCopyAsync(IServiceProvider serviceProvider, int i)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var _db = serviceProvider.GetRequiredService<ISqlSugarClient>();
            TDengineFastBuilder.SetTags(_db, (tag, stable) => $"{stable}_{tag}", nameof(BulkDemo2.Sno), nameof(BulkDemo2.PointNumber));
            // 准备数据
            var list = GetData();

            Console.WriteLine($"{i} 数据准备完毕");
            var sw = Stopwatch.StartNew();
            await _db.Fastest<BulkDemo2>().BulkCopyAsync(list);

            sw.Stop();
            Console.WriteLine($"第{i}次 bulkcopy 插入{list.Count}条 用时 {sw.Elapsed.TotalSeconds} s");

            var count = _db.Queryable<BulkDemo2>().Count();
            Console.WriteLine($"查询到{count}条数据");
        }
    }
}