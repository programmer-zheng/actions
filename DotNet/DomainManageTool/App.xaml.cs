using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using System.IO;
using System.Windows;
using DomainManageTool.Views;

namespace DomainManageTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string secretId = string.Empty, secretKey = string.Empty;
            try
            {
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                var configuration = configBuilder.Build();

                secretId = configuration.GetValue<string>("SecretId");
                secretKey = configuration.GetValue<string>("SecretKey");
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("缺少配置文件appsettings.json");
                Application.Current.Shutdown();
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
                Application.Current.Shutdown();
            }
            var secret = new PlatFormSecret(secretId, secretKey);
            containerRegistry.RegisterInstance<PlatFormSecret>(secret);
        }
    }
}
