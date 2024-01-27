using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.Ddns.Controllers;

public class DdnsController : AbpController
{
    private readonly IConfiguration _configuration;

    public DdnsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Route("/ddns")]
    [HttpGet]
    public async Task<IActionResult> Ddns([FromQuery] string ip)
    {
        Check.NotNullOrWhiteSpace(ip, nameof(ip));
        try
        {
            var record = _configuration.GetValue<string>("Tencent:Record");
            var domain = _configuration.GetValue<string>("Tencent:Domain");
            Check.NotNullOrWhiteSpace(record, nameof(record));
            Check.NotNullOrWhiteSpace(domain, nameof(domain));
            var dnspodSecretId = _configuration.GetValue<string>("Tencent:SecretId");
            var dnspodSecretKey = _configuration.GetValue<string>("Tencent:SecretKey");
            Check.NotNullOrWhiteSpace(dnspodSecretId, nameof(dnspodSecretId));
            Check.NotNullOrWhiteSpace(dnspodSecretKey, nameof(dnspodSecretKey));
            var dnsPodUtil = new TencentDnsPodUtil(dnspodSecretId!, dnspodSecretKey!);
            // 更新dnspod中的域名解析
            await dnsPodUtil.TencentModifyDynamicDns(domain!, record!, ip);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "腾讯DnsPod动态Dns更新出错");
        }

        try
        {
            var aliyunAccessKeyId = _configuration.GetValue<string>("Aliyun:AccessKeyId");
            var aliyunAccessKeySecret = _configuration.GetValue<string>("Aliyun:AccessKeySecret");
            Check.NotNullOrWhiteSpace(aliyunAccessKeyId, nameof(aliyunAccessKeyId));
            Check.NotNullOrWhiteSpace(aliyunAccessKeySecret, nameof(aliyunAccessKeySecret));
            var aliyunSecurityGroupUtil = new AliyunSecurityGroupUtil(aliyunAccessKeyId!, aliyunAccessKeySecret!);
            var securityGroupName = _configuration.GetValue<string>("Aliyun:SecurityGroupName");
            // 修改安全组中的授权对象IP
            await aliyunSecurityGroupUtil.ModifySecurityGroupRuleSourceIp(securityGroupName!, ip);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "阿里云安全组策略更新出错");
        }

        return Content("success");
    }

    [Route("/ddns_ipv6")]
    [HttpGet]
    public async Task<IActionResult> DdnsForIPv6([FromQuery] string ip)
    {
        Check.NotNullOrWhiteSpace(ip, nameof(ip));
        try
        {
            var record = _configuration.GetValue<string>("Tencent:RecordV6");
            var domain = _configuration.GetValue<string>("Tencent:Domain");
            Check.NotNullOrWhiteSpace(record, nameof(record));
            Check.NotNullOrWhiteSpace(domain, nameof(domain));
            var dnspodSecretId = _configuration.GetValue<string>("Tencent:SecretId");
            var dnspodSecretKey = _configuration.GetValue<string>("Tencent:SecretKey");
            Check.NotNullOrWhiteSpace(dnspodSecretId, nameof(dnspodSecretId));
            Check.NotNullOrWhiteSpace(dnspodSecretKey, nameof(dnspodSecretKey));
            var dnsPodUtil = new TencentDnsPodUtil(dnspodSecretId!, dnspodSecretKey!);
            // 更新dnspod中的域名解析
            await dnsPodUtil.TencentModifyDynamicDns(domain!, record!, ip, true);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "IPv6 腾讯DnsPod动态Dns更新出错");
        }

        return Content("success");
    }
}