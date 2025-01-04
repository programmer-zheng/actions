using Prism.Commands;
using Prism.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using TencentCloud.Common;
using TencentCloud.Dnspod.V20210323;
using TencentCloud.Dnspod.V20210323.Models;

namespace CloudManageTool.ViewModels;

public class CreateDomainRecordWindowViewModel : DomainRecordBaseViewModel, IDialogAware
{

    public DialogCloseListener RequestClose { get; }

    public AsyncDelegateCommand SaveDomainRecordCommand { get; private set; }

    public CreateDomainRecordWindowViewModel(PlatFormSecret secret) : base(secret)
    {
        RecordTTL = "600";
        CancelCommand = new DelegateCommand(() =>
        {
            RequestClose.Invoke(ButtonResult.Cancel);
        });
        SaveDomainRecordCommand = new AsyncDelegateCommand(SaveRecord);
        _secret = secret;

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
                CreateRecordResponse resp = await client.CreateRecord(req);
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