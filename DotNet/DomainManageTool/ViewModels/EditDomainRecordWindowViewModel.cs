using DomainManageTool.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using TencentCloud.Dnspod.V20210323.Models;
using TencentCloud.Dnspod.V20210323;
using System.Threading.Tasks;
using Mapster;
using System.Windows;

namespace DomainManageTool.ViewModels
{
    public class EditDomainRecordWindowViewModel : BindableBase, IDialogAware
    {

        private List<string> _recordTypeList;

        public List<string> RecordTypeList
        {
            get => _recordTypeList;
            set => SetProperty(ref _recordTypeList, value);
        }

        private RecordDto _record;

        public RecordDto Record
        {
            get { return _record; }
            set => SetProperty(ref _record, value);
        }


        public DelegateCommand CancelCommand { get; private set; }

        private readonly PlatFormSecret _secret;
        public EditDomainRecordWindowViewModel(PlatFormSecret platFormSecret)
        {
            _secret = platFormSecret;

            RecordTypeList = new List<string>() { "A", "CNAME", "TXT" };

            CancelCommand = new DelegateCommand(() =>
            {
                RequestClose.Invoke(ButtonResult.Cancel);
            });

        }

        public DialogCloseListener RequestClose { get; }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var domainName = parameters.GetValue<string>("DomainName");
            var domainId = parameters.GetValue<ulong>("DomainId");
            var recordId = parameters.GetValue<ulong>("RecordId");
            Task.Run(async () =>
            {
                try
                {
                    Record = await GetRecordById(domainName, domainId, recordId);
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
            // 实例化一个client选项，可选的，没有特殊需求可以跳过
            ClientProfile clientProfile = new ClientProfile();
            // 实例化一个http选项，可选的，没有特殊需求可以跳过
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("dnspod.tencentcloudapi.com");
            clientProfile.HttpProfile = httpProfile;

            // 实例化要请求产品的client对象,clientProfile是可选的
            DnspodClient client = new DnspodClient(cred, "", clientProfile);
            // 实例化一个请求对象,每个接口都会对应一个request对象
            DescribeRecordRequest req = new DescribeRecordRequest();
            req.Domain = domainName;
            req.DomainId = domainId;
            req.RecordId = recordId;
            try
            {
                // 返回的resp是一个DescribeRecordResponse的实例，与请求对象对应
                DescribeRecordResponse resp = await client.DescribeRecord(req);

                TypeAdapterConfig<RecordInfo, RecordDto>.NewConfig().Map(dst => dst.Name, src => src.SubDomain);
                return resp.RecordInfo.Adapt<RecordDto>();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
