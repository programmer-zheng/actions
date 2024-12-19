using CloudManageTool.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using TencentCloud.Dnspod.V20210323.Models;
using TencentCloud.Dnspod.V20210323;
using System.Windows;

namespace CloudManageTool.ViewModels;

public class CreateDomainRecordWindowViewModel : BindableBase, IDialogAware
{
    public string Title { get; set; } = string.Empty;

    private List<string> _recordTypeList;

    public List<string> RecordTypeList
    {
        get => _recordTypeList;
        set => SetProperty(ref _recordTypeList, value);
    }

    private List<string> _lineType;

    public List<string> RecordLineList
    {
        get => _lineType;
        set => SetProperty(ref _lineType, value);
    }

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

    public DialogCloseListener RequestClose { get; }

    public AsyncDelegateCommand SaveDomainRecordCommand { get; private set; }

    public DelegateCommand CancelCommand { get; private set; }

    private readonly PlatFormSecret _secret;
    public CreateDomainRecordWindowViewModel(PlatFormSecret secret)
    {
        RecordTypeList = new List<string> { "A", "CNAME", "MX", "TXT", "AAAA", "NS", "CAA", "SRV", "HTTPS", "SVCB", "SPF", "显性URL", "隐性URL" };
        RecordLineList = new List<string> { "默认" };
        RecordTTL = "600";
        CancelCommand = new DelegateCommand(() =>
        {
            RequestClose.Invoke(ButtonResult.Cancel);
        });
        SaveDomainRecordCommand = new AsyncDelegateCommand(SaveRecord);
        _secret = secret;
    }

    private bool Valid()
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

    private async Task SaveRecord()
    {
        if (Valid())
        {
            // TODO 添加记录逻辑
            Credential cred = new Credential
            {
                SecretId = _secret.SecretId,
                SecretKey = _secret.SecretKey
            };
            DnspodClient client = new DnspodClient(cred, "");
            CreateRecordRequest req = new CreateRecordRequest();
            req.Domain = DomainName;
            req.RecordType = RecordType;
            req.RecordLine = RecordLine;
            req.Value = RecordValue;
            req.SubDomain = RecordName;
            if (!string.IsNullOrWhiteSpace(RecordTTL))
            {
                req.TTL = uint.Parse(RecordTTL);
            }
            req.Remark = RecordRemark;
            try
            {
                CreateRecordResponse resp = client.CreateRecordSync(req);
                RequestClose.Invoke(ButtonResult.OK);
            }
            catch (System.Exception e)
            {
                MessageBox.Show($"添加记录失败：{e.Message}");

            }
        }
    }

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {

    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        DomainId = parameters.GetValue<ulong>("DomainId");
        DomainName = parameters.GetValue<string>("DomainName");
    }
}