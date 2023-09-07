using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.Work;

namespace WeWorkTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var webAppBuilder = WebApplication.CreateBuilder(args);
            webAppBuilder.Configuration.AddJsonFile("appsettings.secret.json", true);

            var config = webAppBuilder.Configuration;

            webAppBuilder.Services
                .AddControllersWithViews();

            webAppBuilder.Services.AddMemoryCache()
                .AddSenparcGlobalServices(config)
                .AddSenparcWeixinServices(config);

            var corpId = config.GetValue<string>("CorpId");
            var appSecret = config.GetValue<string>("AppSecret");
            var agentId = config.GetValue<string>("AgentId");
            var senparcSetting = new SenparcSetting();
            config.GetSection("SenparcSetting").Bind(senparcSetting);
            var senparcWeixinSetting = new SenparcWeixinSetting() { WeixinCorpId = corpId, WeixinCorpSecret = appSecret, WeixinCorpAgentId = agentId };

            //创建注册服务
            IRegisterService register = RegisterService.Start(senparcSetting).UseSenparcGlobal();

            //开始注册微信信息
            register.UseSenparcWeixin(senparcWeixinSetting, senparcSetting)
                //注册企业微信（可注册多个）
                .RegisterWorkAccount(senparcWeixinSetting, "企业微信应用名"); // 注册企业微信应用信息，同时获取了 access token

            var app = webAppBuilder.Build();
            app.MapDefaultControllerRoute();

            app.Run();

        }
    }
}