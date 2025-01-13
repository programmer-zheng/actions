using Scalar.AspNetCore;

namespace Scalar
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.WithTitle("文档中心") //设置浏览器标签页标题
                        .WithForceThemeMode(ThemeMode.Dark) // 强制页面只能使用深色模式
                        .WithTheme(ScalarTheme.Kepler) // 设置默认主题
                        // .WithEndpointPrefix("/swagger/{documentName}") // 默认为/scalar/v1，更改后使用/swagger/v1访问
                        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios)
                        ;
                    ;

                    // options.EnabledClients = new[] { ScalarClient.HttpClient, ScalarClient.Axios, ScalarClient.Curl, ScalarClient.AsyncHttp, ScalarClient.Request };
                    // options.EnabledTargets = new[] { ScalarTarget.CSharp, ScalarTarget.Java, ScalarTarget.JavaScript, ScalarTarget.Shell,  };

                    /*
                     主题
                    Alternate：
                    Default：黑夜
                    Moon：月亮
                    Purple：紫色（默认）
                    Solarized：Solarized
                    BluePlanet：蓝星
                    Saturn：土星
                    Kepler：开普勒
                    Mars：火星
                    DeepSpace：深空
                    */
                });
            }

            app.UseAuthorization();

            app.UseRouting();

            // app.MapControllers();

            app.UseEndpoints(endpoints => { endpoints.MapControllerRoute("Default", "{controller}/{action}/{id?}"); });
            app.Run();
        }
    }
}