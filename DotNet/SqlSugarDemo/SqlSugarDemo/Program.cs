using SqlSugar;
using SqlSugarDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
});

builder.Services.AddSingleton<ISqlSugarClient>(
    s =>
    {
        SqlSugarScope sqlSugar = new SqlSugarScope(new ConnectionConfig()
            {
                DbType = SqlSugar.DbType.MySql,
                ConnectionString = "server=db.local.host;Database=SqlSugarDemo;Uid=root;Pwd=123qwe",
                // DbType = SqlSugar.DbType.Sqlite,
                // ConnectionString = "Data Source=:memory:",
                IsAutoCloseConnection = true,
            },
            db =>
            {
                db.DbMaintenance.CreateDatabase();
                db.CodeFirst.InitTables<Student>();
                db.CodeFirst.InitTables<Book>();
                //单例参数配置，所有上下文生效
                db.Aop.OnLogExecuting = (sql, pars) => { Console.WriteLine(sql); };
            });
        return sqlSugar;
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();