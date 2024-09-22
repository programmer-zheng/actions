using System;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using System.IO;
using System.Windows;
using DomainManageTool.Views;
using DomainManageTool.ViewModels;

namespace DomainManageTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public IConfiguration Configuration { get; private set; }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void Initialize()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(Environment.GetCommandLineArgs());
            Configuration = configurationBuilder.Build();
            base.Initialize();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string secretId = string.Empty, secretKey = string.Empty;
            secretId = Configuration.GetValue<string>(PlatFormSecret.SecretIdVariable);
            secretKey = Configuration.GetValue<string>(PlatFormSecret.SecretKeyVariable);
            if (string.IsNullOrWhiteSpace(secretId) || string.IsNullOrWhiteSpace(secretKey))
            {
                MessageBox.Show("未能从配置中获取Secret信息，请检查配置\n支持appsettgins.json、环境变量、命令行参数！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            containerRegistry.RegisterInstance(Configuration);

            var secret = new PlatFormSecret(secretId, secretKey);
            containerRegistry.RegisterInstance<PlatFormSecret>(secret);

            containerRegistry.RegisterDialog<CreateDomainRecordWindow, CreateDomainRecordWindowViewModel>();
        }
    }
}