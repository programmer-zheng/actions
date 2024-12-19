using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManageTool.Models;
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
    public DvAuthMethodEnum DvAuthMethod {  get; set; }


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
