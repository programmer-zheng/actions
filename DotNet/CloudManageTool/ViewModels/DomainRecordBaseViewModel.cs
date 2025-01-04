using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CloudManageTool.ViewModels
{
    public class DomainRecordBaseViewModel : BindableBase
    {
        public List<string> RecordTypeList => new List<string> { "A", "CNAME", "MX", "TXT", "AAAA", "HTTPS", "显性URL", "隐性URL" };

        public List<string> RecordLineList => new List<string> { "默认" };

        private string _recordType;
        private string _recordName;
        private string _recordLine;
        private string _recordValue;
        private string _recordTTL;
        private string _recordRemark;

        public string RecordType { get => _recordType; set => SetProperty(ref _recordType, value); }
        public string RecordName { get => _recordName; set => SetProperty(ref _recordName, value); }
        public string RecordLine { get => _recordLine; set => SetProperty(ref _recordLine, value); }
        public string RecordValue { get => _recordValue; set => SetProperty(ref _recordValue, value); }
        public string RecordTTL { get => _recordTTL; set => SetProperty(ref _recordTTL, value); }
        public string RecordRemark { get => _recordRemark; set => SetProperty(ref _recordRemark, value); }

        public string DomainName { get; set; }

        public ulong DomainId { get; set; }


        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        protected PlatFormSecret _secret;

        public DelegateCommand<string> ShowDescriptionCommand { get; protected set; }


        public DelegateCommand CancelCommand { get; protected set; }

        public DomainRecordBaseViewModel(PlatFormSecret secret)
        {
            _secret = secret;
            ShowDescriptionCommand = new DelegateCommand<string>(ShowDecription);

        }

        protected void ShowDecription(string fieldName)
        {
            Description = string.Empty;
            if (fieldName.Equals("RecordType", StringComparison.OrdinalIgnoreCase))
            {
                Description = @"A：将域名指向一个 IP 地址。
CNAME：将域名指向另一个域名，再由另一个域名提供 IP 地址。
MX：设置邮箱，让邮箱能收到邮件。
TXT：对域名进行标识和说明，绝大多数的 TXT 记录是用来做 SPF 记录（反垃圾邮件）。
AAAA：将域名指向一个 IPv6 地址，如 2400:da00::dbf:0:100。
HTTPS：将域名指向另一个域名指定值，再由另一个域名提供 IP 地址，就需要添加 HTTPS 记录。
隐、显性 URL：将一个域名指向另外一个已经存在的站点。
";
            }
            else if (fieldName.Equals("RecordName", StringComparison.OrdinalIgnoreCase))
            {
                Description = @"主机记录就是域名前缀，常见用法有：
www ：解析后的域名为 www.dnspod.cn
@ ：直接解析主域名 dnspod.cn
*：泛解析，匹配其他所有域名 *.dnspod.cn";
            }
            else if (fieldName.Equals("RecordLine", StringComparison.OrdinalIgnoreCase))
            {
                Description = "默认线路，一般情况下保留默认即可，每一条记录有默认线路才可以让全球用户都正常解析";
            }
            else if (fieldName.Equals("RecordTTL", StringComparison.OrdinalIgnoreCase))
            {
                Description = @"60 当记录值频繁变动，可选择 60 秒，但解析速度可能略受影响
600 一般默认值，如不了解请保留 600 秒即可
3600 当记录值较少变动时，建议选择 3600 秒，有利于提升解析速度";
            }
        }

        protected bool Valid()
        {
            if (string.IsNullOrWhiteSpace(RecordType))
            {
                MessageBox.Show("请选择记录类型");
                return false;
            }
            if (string.IsNullOrWhiteSpace(RecordLine))
            {
                MessageBox.Show("请选择线路类型");
                return false;
            }
            if (string.IsNullOrWhiteSpace(RecordValue))
            {
                MessageBox.Show("请输入记录值");
                return false;
            }
            if (!string.IsNullOrWhiteSpace(RecordTTL))
            {
                if (!uint.TryParse(RecordTTL, out var ttl))
                {
                    MessageBox.Show("记录的TTL值必须为整数");
                    return false;
                }
            }
            return true;
        }
    }
}
