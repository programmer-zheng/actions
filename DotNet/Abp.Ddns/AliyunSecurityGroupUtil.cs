using AlibabaCloud.SDK.Ecs20140526;
using AlibabaCloud.SDK.Ecs20140526.Models;
using AlibabaCloud.TeaUtil.Models;

namespace Abp.Ddns;

public class AliyunSecurityGroupUtil
{
    private readonly Client _client;
    private readonly string _regionId;

    public AliyunSecurityGroupUtil(string accessKeyId, string accessKeySecret)
    {
        _regionId = "cn-shanghai";
        _client = CreateClient(accessKeyId, accessKeySecret);
    }

    private Client CreateClient(string accessKeyId, string accessKeySecret)
    {
        var config = new AlibabaCloud.OpenApiClient.Models.Config
        {
            AccessKeyId = accessKeyId,
            AccessKeySecret = accessKeySecret,
        };
        // Endpoint 请参考 https://api.aliyun.com/product/Ecs
        config.Endpoint = "ecs.cn-shanghai.aliyuncs.com";
        return new Client(config);
    }

    public async Task ModifySecurityGroupRuleSourceIp(string securityGroupName, string ip)
    {
        var securityGroupId = await GetSecurityGroupId(securityGroupName);
        var ruleId = await GetSecurityGroupRuleId(securityGroupId);
        var modifySecurityGroupRuleRequest = new ModifySecurityGroupRuleRequest
        {
            RegionId = _regionId,
            SecurityGroupId = securityGroupId,
            SecurityGroupRuleId = ruleId,
            SourceCidrIp = ip,
        };
        await _client.ModifySecurityGroupRuleWithOptionsAsync(modifySecurityGroupRuleRequest, new RuntimeOptions());
    }

    private async Task<string> GetSecurityGroupRuleId(string securityGroupId)
    {
        var describeSecurityGroupAttributeRequest = new DescribeSecurityGroupAttributeRequest
        {
            RegionId = _regionId,
            SecurityGroupId = securityGroupId,
        };
        var response = await _client.DescribeSecurityGroupAttributeWithOptionsAsync(describeSecurityGroupAttributeRequest, new RuntimeOptions());
        var rules = response.Body.Permissions.Permission;
        if (rules?.Count > 0)
        {
            return rules.First().SecurityGroupRuleId;
        }

        return null!;
    }

    private async Task<string> GetSecurityGroupId(string securityGroupName)
    {
        var describeSecurityGroupsRequest = new DescribeSecurityGroupsRequest
        {
            RegionId = _regionId,
            SecurityGroupName = securityGroupName,
        };
        var response = await _client.DescribeSecurityGroupsWithOptionsAsync(describeSecurityGroupsRequest, new RuntimeOptions());
        if (response.Body.SecurityGroups.SecurityGroup?.Count > 0)
        {
            return response.Body.SecurityGroups.SecurityGroup.First().SecurityGroupId;
        }

        return null!;
    }
}