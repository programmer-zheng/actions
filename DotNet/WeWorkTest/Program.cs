using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Cache;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.Work;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using Senparc.Weixin.Work.Containers;
using Senparc.Weixin.Work.Entities;

namespace WeWorkTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.secret.json", true, true);

            var config = configBuilder.Build();
            var serviceProvider = new ServiceCollection()
                .AddMemoryCache()
                .AddSenparcGlobalServices(config)
                .AddSenparcWeixinServices(config)
                .BuildServiceProvider();
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


            var appKey = AccessTokenContainer.BuildingKey(senparcWeixinSetting.WeixinCorpId, senparcWeixinSetting.WeixinCorpSecret);

            var accessToken = AccessTokenContainer.GetToken(appKey);


            var departmentList = MailListApi.GetDepartmentList(accessToken);

            var rootUsers = MailListApi.GetDepartmentMemberInfo(accessToken, departmentList.department.First().id, 0);

            Console.WriteLine(rootUsers.ToJson(true));
            // var list = new List<GetMemberResult>();
            // foreach (var department in departmentList?.department)
            // {
            //     var memberInfoResult = MailListApi.GetDepartmentMemberInfo(accessToken, department.id, 0);
            //     var users = memberInfoResult.userlist;
            //     list.AddRange(users);
            // }
            //
            // Console.WriteLine(list.ToJson(true));
        }
    }
}