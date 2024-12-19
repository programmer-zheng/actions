using CloudManageTool.Models;
using Mapster;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Dnspod.V20210323;
using TencentCloud.Dnspod.V20210323.Models;

namespace CloudManageTool.ViewModels
{
    public class DomainManageWindowViewModel : BindableBase, IDialogAware
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

        public DialogCloseListener RequestClose => throw new NotImplementedException();

        private readonly PlatFormSecret _secret;

        private readonly IDialogService _dialogService;

        public DomainManageWindowViewModel(PlatFormSecret platFormSecret, IDialogService dialogService)
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

        private async Task DeleteRecord(string param)
        {
            ulong recordId = ulong.Parse(param);
            var result = MessageBox.Show($"确认删除吗？", "确认", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Credential cred = new Credential
                {
                    SecretId = _secret.SecretId,
                    SecretKey = _secret.SecretKey
                };
                DnspodClient client = new DnspodClient(cred, "");
                DeleteRecordRequest req = new DeleteRecordRequest();
                req.Domain = SelectedDomain.DomainName;
                req.RecordId = recordId;
                try
                {
                    DeleteRecordResponse resp = await client.DeleteRecord(req);
                    await LoadDomainRecordList();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"删除记录失败：{e.Message}");
                }
            }
        }

        private List<DomainDto> LoadDomainList()
        {
            Credential cred = new Credential
            {
                SecretId = _secret.SecretId,
                SecretKey = _secret.SecretKey,
            };
            DnspodClient client = new DnspodClient(cred, "");
            DescribeDomainListRequest req = new DescribeDomainListRequest();
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
            DnspodClient client = new DnspodClient(cred, "");
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
                DescribeRecordListResponse resp = await client.DescribeRecordList(req);
                TypeAdapterConfig<RecordListItem, RecordDto>.NewConfig()
                    .Map(dst => dst.RecordId, src => src.RecordId)
                    .Map(dst => dst.Name, src => src.Name)
                    ;
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

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            
        }
    }
}