using System;
using System.Collections.Generic;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using TencentCloud.Dnspod.V20210323.Models;
using TencentCloud.Dnspod.V20210323;
using Mapster;
using System.Threading.Tasks;
using System.Linq;
using DomainManageTool.Models;
using DomainManageTool.Views;
using Prism.Dialogs;

namespace DomainManageTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private List<RecordDto> _domainRecords;

        public List<RecordDto> DomainRecords
        {
            get => _domainRecords;
            set => SetProperty(ref _domainRecords, value);
        }

        private List<DomainDto> _domainList;

        public List<DomainDto> DomainList
        {
            get => _domainList;
            set => SetProperty(ref _domainList, value);
        }

        private DomainDto _selectedDomain;

        public DomainDto SelectedDomain
        {
            get => _selectedDomain;
            set => SetProperty(ref _selectedDomain, value);
        }

        private string _keywords;

        public string Keywords
        {
            get { return _keywords; }
            set { SetProperty(ref _keywords, value); }
        }



        public AsyncDelegateCommand CreateDomainRecordCommand { get; private set; }

        public AsyncDelegateCommand DomainChangedCommand { get; private set; }

        public AsyncDelegateCommand<string> DeleteRecordCommand { get; private set; }
        public AsyncDelegateCommand<string> EditRecordCommand { get; private set; }


        public AsyncDelegateCommand SearchCommand { get; private set; }

        private readonly PlatFormSecret _secret;

        private readonly IDialogService _dialogService;

        public MainWindowViewModel(PlatFormSecret platFormSecret, IDialogService dialogService)
        {
            CreateDomainRecordCommand = new AsyncDelegateCommand(ShowCreateDomainRecordWindowDialog);
            DomainChangedCommand = new AsyncDelegateCommand(LoadDomainRecordList);
            DeleteRecordCommand = new AsyncDelegateCommand<string>(DeleteRecord);
            EditRecordCommand = new AsyncDelegateCommand<string>(EditRecord);
            SearchCommand = new AsyncDelegateCommand(LoadDomainRecordList);
            _secret = platFormSecret;
            _dialogService = dialogService;
            DomainList = LoadDomainList();
        }

        private async Task EditRecord(string recordId)
        {
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("DomainName", SelectedDomain.DomainName);
            dialogParameters.Add("DomainId", SelectedDomain.DomainId);
            dialogParameters.Add("RecordId", recordId);

            var dialogResult = await _dialogService.ShowDialogAsync("EditDomainRecordWindow", dialogParameters);
            if (dialogResult.Result == ButtonResult.OK)
            {
                await LoadDomainRecordList();
            }
        }

        private async Task DeleteRecord(string recordName)
        {
            var result = MessageBox.Show($"确认删除吗？", "确认", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.OK)
            {

                await LoadDomainRecordList();
            }
        }

        private List<DomainDto> LoadDomainList()
        {
            Credential cred = new Credential
            {
                SecretId = _secret.SecretId,
                SecretKey = _secret.SecretKey,
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
            DescribeDomainListRequest req = new DescribeDomainListRequest();

            // 返回的resp是一个DescribeDomainListResponse的实例，与请求对象对应
            DescribeDomainListResponse resp = client.DescribeDomainListSync(req);
            TypeAdapterConfig<DomainListItem, DomainDto>.NewConfig().Map(dst => dst.DomainName, src => src.Name);
            var result = resp.DomainList.Adapt<List<DomainDto>>();
            return result;
        }

        private async Task LoadDomainRecordList()
        {
            if (SelectedDomain == null)
            {
                MessageBox.Show("请先选择域名");
                return;
            }
            Credential cred = new Credential
            {
                SecretId = _secret.SecretId,
                SecretKey = _secret.SecretKey,
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
            DescribeRecordListRequest req = new DescribeRecordListRequest();
            req.Domain = SelectedDomain.DomainName;
            if (!string.IsNullOrWhiteSpace(Keywords))
            {
                req.Keyword = Keywords;
            }
            req.SortField = "updated_on";
            req.SortType = "desc";
            try
            {
                // 返回的resp是一个DescribeRecordLineCategoryListResponse的实例，与请求对象对应
                DescribeRecordListResponse resp = await client.DescribeRecordList(req);

                TypeAdapterConfig<RecordListItem, RecordDto>.NewConfig().Map(dst => dst.Name, src => src.Name);
                DomainRecords = resp.RecordList.Adapt<List<RecordDto>>();
            }
            catch (TencentCloud.Common.TencentCloudSDKException)
            {

            }
            catch (Exception e)
            {
                MessageBox.Show($"加载记录列表出现异常：{e.Message}");
            }
        }

        private async Task ShowCreateDomainRecordWindowDialog()
        {
            if (SelectedDomain == null)
            {
                MessageBox.Show("请先选择域名");
                return;
            }
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("DomainId", SelectedDomain.DomainId);
            dialogParameters.Add("DomainName", SelectedDomain.DomainName);

            var dialogResult = await _dialogService.ShowDialogAsync("CreateDomainRecordWindow", dialogParameters);
            if (dialogResult.Result == ButtonResult.OK)
            {
                await LoadDomainRecordList();
            }
        }
    }
}