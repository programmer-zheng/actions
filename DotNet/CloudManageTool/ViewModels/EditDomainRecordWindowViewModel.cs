using CloudManageTool.Models;
using Mapster;
using Prism.Commands;
using Prism.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using TencentCloud.Common;
using TencentCloud.Dnspod.V20210323;
using TencentCloud.Dnspod.V20210323.Models;

namespace CloudManageTool.ViewModels
{
    public class EditDomainRecordWindowViewModel : DomainRecordBaseViewModel, IDialogAware
    {
        public DialogCloseListener RequestClose { get; }


        private RecordDto _record;

        public RecordDto Record
        {
            get { return _record; }
            set => SetProperty(ref _record, value);
        }

        public AsyncDelegateCommand SaveDomainRecordCommand { get; private set; }


        public ulong RecordId { get; set; }

        public EditDomainRecordWindowViewModel(PlatFormSecret platFormSecret) : base(platFormSecret)
        {

            CancelCommand = new DelegateCommand(() =>
            {
                RequestClose.Invoke(ButtonResult.Cancel);
            });

            SaveDomainRecordCommand = new AsyncDelegateCommand(SaveDomainRecordAsync);

        }

        private async Task SaveDomainRecordAsync()
        {
            if (Valid())
            {
                Credential cred = new Credential
                {
                    SecretId = _secret.SecretId,
                    SecretKey = _secret.SecretKey
                };
                DnspodClient client = new DnspodClient(cred, "");
                ModifyRecordRequest req = new ModifyRecordRequest();
                req.RecordId = RecordId;
                req.Domain = DomainName;
                req.RecordType = Record.Type;
                req.RecordLine = Record.Line;
                req.Value = Record.Value;
                req.SubDomain = Record.Name;
                req.Remark = Record.Remark;
                try
                {
                    ModifyRecordResponse resp = await client.ModifyRecord(req);
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
            DomainName = parameters.GetValue<string>("DomainName");
            DomainId = parameters.GetValue<ulong>("DomainId");
            RecordId = parameters.GetValue<ulong>("RecordId");
            Task.Run(async () =>
            {
                try
                {
                    Record = await GetRecordById(DomainName, DomainId, RecordId);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"查询域名信息失败{e.Message}", "错误", MessageBoxButton.OK);
                }
            });

        }

        private async Task<RecordDto> GetRecordById(string domainName, ulong domainId, ulong recordId)
        {
            Credential cred = new Credential
            {
                SecretId = _secret.SecretId,
                SecretKey = _secret.SecretKey
            };
            DnspodClient client = new DnspodClient(cred, "");
            DescribeRecordRequest req = new DescribeRecordRequest();
            req.Domain = domainName;
            req.DomainId = domainId;
            req.RecordId = recordId;
            try
            {
                DescribeRecordResponse resp = await client.DescribeRecord(req);
                TypeAdapterConfig<RecordInfo, RecordDto>.NewConfig()
                    .Map(dst => dst.RecordId, src => src.Id)
                    .Map(dst => dst.Name, src => src.SubDomain)
                    .Map(dst => dst.Line, src => src.RecordLine)
                    .Map(dst => dst.Type, src => src.RecordType);
                return resp.RecordInfo.Adapt<RecordDto>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
