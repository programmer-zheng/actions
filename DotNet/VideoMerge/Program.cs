// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoMerge;
using Volo.Abp;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureServices(services =>
            {
                services.AddApplicationAsync<AppModule>(options =>
                {
                    options.Services.ReplaceConfiguration(services.GetConfiguration());
                    options.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
                });
            })
                .UseAutofac().UseConsoleLifetime();
            var host = builder.Build();

            await host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().InitializeAsync(host.Services);

            await host.RunAsync();
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 1;
        }
        //using (var application = AbpApplicationFactory.Create<AppModule>(options =>
        //{
        //    options.UseAutofac(); //Autofac integration
        //}))
        //{
        //    application.Initialize();

        //    Console.WriteLine("Press ENTER to stop application...");
        //    Console.ReadLine();
        //}

    }
}