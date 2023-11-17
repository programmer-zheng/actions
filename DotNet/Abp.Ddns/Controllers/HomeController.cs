using Abp.Ddns.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tea;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.Ddns.Controllers
{
    public class HomeController : AbpController
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/test")]
        public async Task<IActionResult> Test()
        {
            // https://next.api.aliyun.com/document/Ecs/2014-05-26/overview
            // 请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_ID 和 ALIBABA_CLOUD_ACCESS_KEY_SECRET。
            // 工程代码泄露可能会导致 AccessKey 泄露，并威胁账号下所有资源的安全性。以下代码示例使用环境变量获取 AccessKey 的方式进行调用，仅供参考，建议使用更安全的 STS 方式，更多鉴权访问方式请参见：https://help.aliyun.com/document_detail/378671.html
            // AlibabaCloud.SDK.Ecs20140526.Client client = CreateClient(Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_ID"), Environment.GetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_SECRET"));
            AlibabaCloud.SDK.Ecs20140526.Client client = CreateClient(_configuration.GetValue<string>("Aliyun:AccessKeyId"), _configuration.GetValue<string>("Aliyun:AccessKeySecret"));
            AlibabaCloud.SDK.Ecs20140526.Models.ModifySecurityGroupRuleRequest modifySecurityGroupRuleRequest = new AlibabaCloud.SDK.Ecs20140526.Models.ModifySecurityGroupRuleRequest
            {
                RegionId = "your_value",
                 
            };
            try
            {
                // 复制代码运行请自行打印 API 的返回值
                client.ModifySecurityGroupRuleWithOptions(modifySecurityGroupRuleRequest, new AlibabaCloud.TeaUtil.Models.RuntimeOptions());
            }
            catch (TeaException error)
            {
                // 如有需要，请打印 error
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
            }
            catch (Exception _error)
            {
                TeaException error = new TeaException(new Dictionary<string, object>
                {
                    { "message", _error.Message }
                });
                // 如有需要，请打印 error
                AlibabaCloud.TeaUtil.Common.AssertAsString(error.Message);
            }

            return Content("");
        }
        
        public static AlibabaCloud.SDK.Ecs20140526.Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 必填，您的 AccessKey ID
                AccessKeyId = accessKeyId,
                // 必填，您的 AccessKey Secret
                AccessKeySecret = accessKeySecret,
            };
            // Endpoint 请参考 https://api.aliyun.com/product/Ecs
            config.Endpoint = "ecs-cn-hangzhou.aliyuncs.com";
            return new AlibabaCloud.SDK.Ecs20140526.Client(config);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}