using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TencentCloud.Ssl.V20191205.Models;

namespace CloudManageTool.Models;

public class SslListDto : Certificates
{
    public bool CanDownload
    {
        get
        {
            var flag = false;
            switch (Status)
            {
                case 1://已通过
                case 4://验证方式为 DNS_AUTO 类型的证书，已添加DNS记录

                //case 0://审核中
                //case 2://审核失败
                //case 3://已过期
                //case 5://企业证书，待提交
                //case 6://订单取消中
                //case 7://已取消
                //case 8://已提交资料，待上传确认函
                //case 9://证书吊销中
                //case 10://已吊销
                //case 11://重颁发中
                //case 12://待上传吊销确认函
                //case 13://免费证书待提交资料状态
                //case 14://已退款
                    flag =true;
                    break;
                default:
                    break;
            }
            return flag;
        }
    }
}


public class SslDownloadDto
{
    /// <summary>
    /// 描述信息
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 证书服务类型（
    /// </summary>
    public string ServiceType { get; set; }
}

/// <summary>
/// 创建免费证书
/// </summary>
public class CreateFreeSslDto
{
    /// <summary>
    /// 域名
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// 验证方式
    /// </summary>
    public DvAuthMethodEnum DvAuthMethod { get; set; }


}

public enum DvAuthMethodEnum
{
    /// <summary>
    /// 自动DNS验证
    /// </summary>
    DNS_AUTO,

    /// <summary>
    /// 手动DNS验证
    /// </summary>
    DNS,

    /// <summary>
    /// 文件验证
    /// </summary>
    FILE,
}
