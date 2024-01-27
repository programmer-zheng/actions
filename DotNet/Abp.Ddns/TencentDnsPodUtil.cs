using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Dnspod.V20210323;
using TencentCloud.Dnspod.V20210323.Models;
using Volo.Abp;

namespace Abp.Ddns;

public class TencentDnsPodUtil
{
    private readonly DnspodClient _dnspodClient;

    public TencentDnsPodUtil(string secretId, string secretKey)
    {
        Check.NotNullOrWhiteSpace(secretId, nameof(secretId));
        Check.NotNullOrWhiteSpace(secretKey, nameof(secretKey));
        _dnspodClient = new DnspodClient(new Credential { SecretId = secretId, SecretKey = secretKey }, "", new ClientProfile());
    }


    public async Task TencentModifyDynamicDns(string domain, string subdomain, string ip, bool isIPv6 = false)
    {
        var recordId = await GetTencentDnsPodRecordId(domain, subdomain, isIPv6);
        ModifyDynamicDNSRequest req = new ModifyDynamicDNSRequest();
        req.Domain = domain;
        req.RecordId = recordId;
        req.SubDomain = subdomain;
        req.Value = ip;
        req.RecordLine = "默认";
        req.Ttl = 120;
        await _dnspodClient.ModifyDynamicDNS(req);
    }


    private async Task<ulong> GetTencentDnsPodRecordId(string domain, string subdomain, bool isIPv6)
    {
        DescribeRecordListRequest req = new DescribeRecordListRequest();
        req.Domain = domain;
        req.Subdomain = subdomain;
        if (isIPv6)
        {
            req.RecordType = "AAAA";
        }
        else
        {
            req.RecordType = "A";
        }
        DescribeRecordListResponse resp = await _dnspodClient.DescribeRecordList(req);

        if (resp.RecordList != null && resp.RecordList.Length > 0)
        {
            return resp.RecordList.First().RecordId!.Value;
        }

        return 0;
    }
}